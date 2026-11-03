using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LibLala.Jellyfin
{
    public sealed class JellyfinClient : IJellyfinClient
    {
        private const string NotJellyfinMessage = "That doesn't look like a Jellyfin server. Is the address right?";

        // One for the whole app, that's how HttpClient is meant to be used. It doesn't follow redirects because that would
        // drop the API key, e.g. from http to https, and then the server's answer would blame the key.
        private static readonly HttpClient s_sharedHttpClient = new HttpClient(new SocketsHttpHandler { AllowAutoRedirect = false })
        {
            Timeout = TimeSpan.FromSeconds(15),
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

        private async Task<T?> GetAsync<T>(string path, CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, new Uri($"{_serverAddress}/{path}"));
            request.Headers.Authorization = _authorization;

            try
            {
                using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
                if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                {
                    throw new JellyfinException("The server doesn't accept this API key.");
                }
                if (!response.IsSuccessStatusCode)
                {
                    throw new JellyfinException($"The server answered with error {(int)response.StatusCode}. Is the address right?");
                }

                var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                return JsonSerializer.Deserialize<T>(json, JsonSerializerOptions.Web);
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
            catch (JsonException e)
            {
                throw new JellyfinException(NotJellyfinMessage, e);
            }
        }

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
