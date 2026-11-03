using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LibLala.Jellyfin
{
    public sealed class JellyfinClient : IJellyfinClient
    {
        private const string NotJellyfinMessage = "That doesn't look like a Jellyfin server. Is the address right?";
        private const string KeyRefusedMessage = "The server doesn't accept this API key.";
        private const string ChangeRefusedMessage = "Jellyfin doesn't allow this user to change that playlist.";
        private const int AudioItemsPageSize = 500;
        // These IDs go in the URL, which servers limit to about 8 KB. An ID takes 33 characters.
        private const int IdsPerRequest = 100;

        // One for the whole app, that's how HttpClient is meant to be used. It doesn't follow redirects because that would
        // drop the API key, e.g. from http to https, and then the server's answer would blame the key.
        private static readonly HttpClient s_sharedHttpClient = new HttpClient(new SocketsHttpHandler { AllowAutoRedirect = false })
        {
            Timeout = TimeSpan.FromSeconds(30),
        };

        private readonly string _serverAddress;
        private readonly AuthenticationHeaderValue _authorization;
        private readonly HttpClient _httpClient;

        public JellyfinClient(string serverAddress, string apiKey) : this(serverAddress, apiKey, s_sharedHttpClient)
        {
        }

        public JellyfinClient(string serverAddress, string apiKey, HttpClient httpClient)
        {
            ArgumentNullException.ThrowIfNull(serverAddress);
            ArgumentNullException.ThrowIfNull(apiKey);
            ArgumentNullException.ThrowIfNull(httpClient);

            _serverAddress = CheckedAddress(serverAddress);
            _authorization = new AuthenticationHeaderValue("MediaBrowser", $"Token=\"{CheckedApiKey(apiKey)}\"");
            _httpClient = httpClient;
        }

        public async Task<IReadOnlyList<JellyfinUser>> GetUsersAsync(CancellationToken cancellationToken = default)
        {
            var users = await GetAsync<List<JellyfinUser>>("Users", cancellationToken).ConfigureAwait(false);

            // Other servers can answer with JSON as well. Jellyfin always has a user, it can't be set up without one.
            var named = users?.Where(x => !string.IsNullOrEmpty(x.Id) && !string.IsNullOrEmpty(x.Name)).ToList();
            return named is { Count: > 0 } ? named : throw new JellyfinException(NotJellyfinMessage);
        }

        public async Task<JellyfinAudioLibrary> GetAudioLibraryAsync(string userId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(userId);

            // By ID, Jellyfin can't sort by anything unique, so a track could come up in two pages. Another one is left out then,
            // which ReportedTotal shows.
            var items = new Dictionary<string, JellyfinAudioItem>();
            var startIndex = 0;
            ItemsPage<JellyfinAudioItem> page;
            do
            {
                var path = string.Create(CultureInfo.InvariantCulture, $"Items?userId={Uri.EscapeDataString(userId)}&recursive=true&includeItemTypes=Audio&fields=Path&sortBy=SortName&enableImages=false&enableUserData=false&startIndex={startIndex}&limit={AudioItemsPageSize}");
                page = await GetItemsAsync<JellyfinAudioItem>(path, cancellationToken).ConfigureAwait(false);

                foreach (var item in page.Items!.Where(x => !string.IsNullOrEmpty(x.Id) && !string.IsNullOrEmpty(x.Path)))
                {
                    items[item.Id] = item;
                }
                startIndex += page.Items!.Count;
            }
            while (page.Items!.Count > 0 && startIndex < page.TotalRecordCount);

            if (items.Count == 0 && page.TotalRecordCount > 0)
            {
                throw new JellyfinException("The server lists its tracks but doesn't say where the files are.");
            }

            return new JellyfinAudioLibrary(items.Values.ToList(), page.TotalRecordCount);
        }

        public async Task<IReadOnlyList<JellyfinPlaylist>> GetPlaylistsAsync(string userId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(userId);

            var path = $"Items?userId={Uri.EscapeDataString(userId)}&recursive=true&includeItemTypes=Playlist&enableImages=false&enableUserData=false";
            var page = await GetItemsAsync<JellyfinPlaylist>(path, cancellationToken).ConfigureAwait(false);
            return page.Items!.Where(x => !string.IsNullOrEmpty(x.Id) && !string.IsNullOrEmpty(x.Name)).ToList();
        }

        public async Task<IReadOnlyList<JellyfinPlaylistEntry>> GetPlaylistEntriesAsync(string playlistId, string userId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(playlistId);
            ArgumentNullException.ThrowIfNull(userId);

            var path = $"Playlists/{Uri.EscapeDataString(playlistId)}/Items?userId={Uri.EscapeDataString(userId)}&enableImages=false&enableUserData=false";
            var page = await GetItemsAsync<PlaylistItem>(path, cancellationToken).ConfigureAwait(false);
            return page.Items!.Where(x => !string.IsNullOrEmpty(x.Id)).Select(x => new JellyfinPlaylistEntry(x.Id, x.PlaylistItemId ?? x.Id)).ToList();
        }

        public async Task<string> CreatePlaylistAsync(string name, string userId, IReadOnlyList<string> itemIds, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(name);
            ArgumentNullException.ThrowIfNull(userId);
            ArgumentNullException.ThrowIfNull(itemIds);

            // The default options keep the names as they are, Name and not name, that's how Jellyfin's API has them. As a string
            // for its known length, so that it isn't sent in chunks, which not every reverse proxy passes on.
            var body = JsonSerializer.Serialize(new { Name = name, Ids = itemIds, UserId = userId, MediaType = "Audio", IsPublic = false });
            using var content = new StringContent(body, Encoding.UTF8, "application/json");
            var json = await SendAsync(HttpMethod.Post, "Playlists", content, ChangeRefusedMessage, cancellationToken).ConfigureAwait(false);
            return Parse<CreatedPlaylist>(json)?.Id is { Length: > 0 } id ? id : throw new JellyfinException(NotJellyfinMessage);
        }

        public async Task AddToPlaylistAsync(string playlistId, IReadOnlyList<string> itemIds, string userId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(playlistId);
            ArgumentNullException.ThrowIfNull(itemIds);
            ArgumentNullException.ThrowIfNull(userId);

            // In order, each batch goes to the end of the playlist.
            foreach (var batch in itemIds.Chunk(IdsPerRequest))
            {
                var path = $"Playlists/{Uri.EscapeDataString(playlistId)}/Items?ids={string.Join(',', batch.Select(Uri.EscapeDataString))}&userId={Uri.EscapeDataString(userId)}";
                await SendAsync(HttpMethod.Post, path, null, ChangeRefusedMessage, cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task RemoveFromPlaylistAsync(string playlistId, IReadOnlyList<string> entryIds, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(playlistId);
            ArgumentNullException.ThrowIfNull(entryIds);

            foreach (var batch in entryIds.Chunk(IdsPerRequest))
            {
                var path = $"Playlists/{Uri.EscapeDataString(playlistId)}/Items?entryIds={string.Join(',', batch.Select(Uri.EscapeDataString))}";
                await SendAsync(HttpMethod.Delete, path, null, ChangeRefusedMessage, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task<ItemsPage<T>> GetItemsAsync<T>(string path, CancellationToken cancellationToken)
        {
            var page = await GetAsync<ItemsPage<T>>(path, cancellationToken).ConfigureAwait(false);
            return page?.Items is null ? throw new JellyfinException(NotJellyfinMessage) : page;
        }

        private async Task<T?> GetAsync<T>(string path, CancellationToken cancellationToken)
        {
            return Parse<T>(await SendAsync(HttpMethod.Get, path, null, KeyRefusedMessage, cancellationToken).ConfigureAwait(false));
        }

        private static T? Parse<T>(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(json, JsonSerializerOptions.Web);
            }
            catch (JsonException e)
            {
                throw new JellyfinException(NotJellyfinMessage, e);
            }
        }

        private async Task<string> SendAsync(HttpMethod method, string path, HttpContent? content, string forbiddenMessage, CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(method, new Uri($"{_serverAddress}/{path}")) { Content = content };
            request.Headers.Authorization = _authorization;

            try
            {
                using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new JellyfinException(KeyRefusedMessage);
                }
                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    throw new JellyfinException(forbiddenMessage);
                }
                if ((int)response.StatusCode >= 500)
                {
                    throw new JellyfinException($"The server answered with error {(int)response.StatusCode}.");
                }
                if (!response.IsSuccessStatusCode)
                {
                    throw new JellyfinException($"The server answered with error {(int)response.StatusCode}. Is the address right?");
                }

                return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            }
            // Its own message only says to see the inner exception, and this is what a self-signed certificate ends up as.
            catch (HttpRequestException e) when (e.HttpRequestError == HttpRequestError.SecureConnectionError)
            {
                throw new JellyfinException($"Can't connect securely to the server: {e.InnerException?.Message}", e);
            }
            catch (HttpRequestException e)
            {
                throw new JellyfinException($"Can't reach the server: {e.Message}", e);
            }
            catch (TaskCanceledException e) when (!cancellationToken.IsCancellationRequested)
            {
                throw new JellyfinException("The server didn't answer in time.", e);
            }
        }

        private sealed record ItemsPage<T>(IReadOnlyList<T>? Items, int TotalRecordCount);

        private sealed record PlaylistItem(string Id, string? PlaylistItemId);

        private sealed record CreatedPlaylist(string? Id);

        // Without the query and fragment, a copy from the browser can have those. With the path, a reverse proxy can have the server in a subfolder.
        private static string CheckedAddress(string serverAddress)
        {
            if (Uri.TryCreate(serverAddress.Trim(), UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                return uri.GetLeftPart(UriPartial.Path).TrimEnd('/');
            }

            throw new JellyfinException("Enter the server's whole address, like http://192.168.1.10:8096");
        }

        // What can't go in the header unchanged wouldn't be a key the server knows anyway.
        private static string CheckedApiKey(string apiKey)
        {
            var key = apiKey.Trim();
            if (key.Length > 0 && key.All(x => char.IsAscii(x) && !char.IsControl(x) && x != '"' && x != '\\'))
            {
                return key;
            }

            throw new JellyfinException("Enter the API key, you can create one in Jellyfin's dashboard.");
        }
    }
}
