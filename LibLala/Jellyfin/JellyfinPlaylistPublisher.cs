using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LibLala.Jellyfin
{
    public sealed record JellyfinPlaylist(string Id, string Name);

    // EntryId is what removing it from the playlist goes by. Jellyfin uses the track's ID for that at the moment, but it hasn't always.
    // Name is the track's title, to show one that has no path.
    public sealed record JellyfinPlaylistEntry(string ItemId, string EntryId, string Name = "");

    // What pushing a playlist is going to do, for the user to decide on before anything changes.
    // ExistingPlaylistId: the playlist in Jellyfin that gets replaced, null when a new one gets created.
    // ItemIds: the tracks that were found, in the order of the playlist. Unresolved: the ones that will be missing.
    // LibraryTracksNotRead: how many of the tracks Jellyfin reports it didn't give, so a track that wasn't found can be there after all.
    public sealed record JellyfinPushPlan(string PlaylistName, string? ExistingPlaylistId, int ExistingTrackCount, IReadOnlyList<string> ItemIds, IReadOnlyList<JellyfinTrackMatch> Unresolved, int LibraryTracksNotRead);

    public sealed class JellyfinPlaylistPublisher
    {
        private readonly IJellyfinClient _client;
        private readonly string _userId;

        public JellyfinPlaylistPublisher(IJellyfinClient client, string userId)
        {
            _client = client;
            _userId = userId;
        }

        // Only reads. The playlists first: that's one request, and it can already be the end of it. The library takes many.
        public async Task<JellyfinPushPlan> PrepareAsync(string playlistName, string libraryRoot, IEnumerable<string> trackPaths, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(trackPaths);

            var playlists = await _client.GetPlaylistsAsync(_userId, cancellationToken).ConfigureAwait(false);
            var sameName = playlists.Where(x => x.Name.Equals(playlistName, StringComparison.OrdinalIgnoreCase)).ToList();
            if (sameName.Count > 1)
            {
                throw new JellyfinException($"Jellyfin has {sameName.Count} playlists called {playlistName} that this user can see. Which one to replace would be a guess, so nothing was changed. Rename or delete the others in Jellyfin.");
            }

            var library = await _client.GetAudioLibraryAsync(_userId, cancellationToken).ConfigureAwait(false);
            var matcher = new JellyfinTrackMatcher(libraryRoot, library.Items);
            var matches = matcher.MatchAll(trackPaths);
            var itemIds = matches.Where(x => x.Status == JellyfinMatchStatus.Found).Select(x => x.Candidates[0].Id).Distinct().ToList();
            var unresolved = matches.Where(x => x.Status != JellyfinMatchStatus.Found).ToList();

            var existing = sameName.FirstOrDefault();
            var existingTrackCount = existing is null ? 0 : (await _client.GetPlaylistEntriesAsync(existing.Id, _userId, cancellationToken).ConfigureAwait(false)).Count;
            return new JellyfinPushPlan(playlistName, existing?.Id, existingTrackCount, itemIds, unresolved, Math.Max(0, library.ReportedTotal - library.Items.Count));
        }

        public async Task PublishAsync(JellyfinPushPlan plan, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(plan);
            if (plan.ItemIds.Count == 0)
            {
                throw new ArgumentException("A plan without tracks would leave an empty playlist behind.", nameof(plan));
            }

            var playlistId = plan.ExistingPlaylistId is null
                ? await _client.CreatePlaylistAsync(plan.PlaylistName, _userId, plan.ItemIds, cancellationToken).ConfigureAwait(false)
                : await ReplaceAsync(plan.ExistingPlaylistId, plan, cancellationToken).ConfigureAwait(false);

            // The order is the point of all this, and servers of other versions can behave differently. So see what came out.
            var entries = await _client.GetPlaylistEntriesAsync(playlistId, _userId, cancellationToken).ConfigureAwait(false);
            var itemIds = entries.Select(x => x.ItemId).ToList();
            if (!itemIds.SequenceEqual(plan.ItemIds, StringComparer.OrdinalIgnoreCase))
            {
                throw new JellyfinException($"The playlist {plan.PlaylistName} in Jellyfin didn't come out as it should: {Difference(itemIds, plan.ItemIds)}. Push it again.");
            }
        }

        private static string Difference(List<string> itemIds, IReadOnlyList<string> planned)
        {
            if (itemIds.Count != planned.Count)
            {
                return $"it has {itemIds.Count} tracks instead of {planned.Count}";
            }

            return itemIds.ToHashSet(StringComparer.OrdinalIgnoreCase).SetEquals(planned) ? "its tracks are in another order" : "it has other tracks than it should";
        }

        // In this order, and in place so that the playlist keeps its ID. Adding new tracks before removing the old ones would be
        // nicer if it fails halfway, but removing goes by the track, so that would remove what was just added as well.
        private async Task<string> ReplaceAsync(string playlistId, JellyfinPushPlan plan, CancellationToken cancellationToken)
        {
            // Removing doesn't check whose playlist it is, adding does. This stops here, before anything changed, when it's
            // another user's public playlist that happens to have the same name.
            await _client.AddToPlaylistAsync(playlistId, plan.ItemIds.Take(1).ToList(), _userId, cancellationToken).ConfigureAwait(false);

            try
            {
                var entries = await _client.GetPlaylistEntriesAsync(playlistId, _userId, cancellationToken).ConfigureAwait(false);
                await _client.RemoveFromPlaylistAsync(playlistId, entries.Select(x => x.EntryId).Distinct().ToList(), cancellationToken).ConfigureAwait(false);
                await _client.AddToPlaylistAsync(playlistId, plan.ItemIds, _userId, cancellationToken).ConfigureAwait(false);
            }
            catch (JellyfinException e)
            {
                throw new JellyfinException($"{e.Message} The playlist {plan.PlaylistName} in Jellyfin can be incomplete now. Push it again when the server is back.", e);
            }

            return playlistId;
        }
    }
}
