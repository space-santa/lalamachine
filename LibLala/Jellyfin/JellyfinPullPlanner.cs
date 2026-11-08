using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LibLala.Jellyfin
{
    public enum JellyfinPullStatus
    {
        Found,
        // No track here is at its path below the library folder.
        NotFound,
        // Jellyfin has its path more than once. Pushing can't tell which one the track here is, so pulling doesn't guess either.
        AmbiguousInJellyfin,
        // More than one track here has its path, e.g. they only differ in case.
        AmbiguousHere,
        // Not one of the audio tracks Jellyfin listed, e.g. a video, or the library changed while it was being read.
        Unknown,
    }

    // A track of a playlist in Jellyfin, and which one it is here.
    // Name: its title in Jellyfin. JellyfinPath: where it is there, empty when unknown. TrackPath: the track here, when found.
    // Others: when not found, the tracks here with the same file name, to see why. When ambiguous, the paths it could be.
    public sealed record JellyfinPullMatch(string ItemId, string Name, string JellyfinPath, JellyfinPullStatus Status, string TrackPath, IReadOnlyList<string> Others);

    // What pulling a playlist is going to do, for the user to decide on before anything changes.
    // TrackCount: how many different tracks the playlist has in Jellyfin. TrackPaths: the ones found here, in the order of the playlist.
    // Unresolved: the ones that will be missing. LibraryTracksNotRead: how many of the tracks Jellyfin reports it didn't give.
    public sealed record JellyfinPullPlan(string PlaylistName, int TrackCount, IReadOnlyList<string> TrackPaths, IReadOnlyList<JellyfinPullMatch> Unresolved, int LibraryTracksNotRead);

    // The other way round from the publisher. A track in Jellyfin is the one here that pushing would find it for, so pushing a
    // pulled playlist gives the same playlist again.
    public sealed class JellyfinPullPlanner
    {
        private readonly IJellyfinClient _client;
        private readonly string _userId;

        public JellyfinPullPlanner(IJellyfinClient client, string userId)
        {
            _client = client;
            _userId = userId;
        }

        // Only reads. libraryTrackPaths: all tracks of the library here, a playlist can only have those. The tracks of the playlist
        // first: that's one request, and an empty playlist is the end of it. The library takes many.
        public async Task<JellyfinPullPlan> PrepareAsync(JellyfinPlaylist playlist, string libraryRoot, IEnumerable<string> libraryTrackPaths, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(playlist);
            ArgumentNullException.ThrowIfNull(libraryTrackPaths);

            // A playlist here has each track once, where it comes first in Jellyfin.
            var entries = (await _client.GetPlaylistEntriesAsync(playlist.Id, _userId, cancellationToken).ConfigureAwait(false))
                .DistinctBy(x => x.ItemId, StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (entries.Count == 0)
            {
                return new JellyfinPullPlan(playlist.Name, 0, [], [], 0);
            }

            var library = await _client.GetAudioLibraryAsync(_userId, cancellationToken).ConfigureAwait(false);
            var tracks = new LibraryTracks(libraryRoot, library.Items, libraryTrackPaths);
            var matches = entries.Select(tracks.Match).ToList();

            return new JellyfinPullPlan(
                playlist.Name,
                entries.Count,
                matches.Where(x => x.Status == JellyfinPullStatus.Found).Select(x => x.TrackPath).ToList(),
                matches.Where(x => x.Status != JellyfinPullStatus.Found).ToList(),
                Math.Max(0, library.ReportedTotal - library.Items.Count));
        }

        // Every track here gets matched like for a push. The folder in Jellyfin that most of them are in decides, so with the whole
        // library that's the one it really is in.
        private sealed class LibraryTracks
        {
            private readonly Dictionary<string, string> _jellyfinPaths;
            private readonly ILookup<string, string> _found;
            private readonly ILookup<string, IReadOnlyList<JellyfinAudioItem>> _ambiguous;
            private readonly ILookup<string, string> _byFileName;

            public LibraryTracks(string libraryRoot, IReadOnlyList<JellyfinAudioItem> items, IEnumerable<string> trackPaths)
            {
                // The library can have a path twice, that's still one track.
                var paths = trackPaths.Distinct().ToList();
                var matches = new JellyfinTrackMatcher(libraryRoot, items).MatchAll(paths);

                _jellyfinPaths = items.DistinctBy(x => x.Id, StringComparer.OrdinalIgnoreCase).ToDictionary(x => x.Id, x => x.Path, StringComparer.OrdinalIgnoreCase);
                _found = matches
                    .Where(x => x.Status == JellyfinMatchStatus.Found)
                    .ToLookup(x => x.Candidates[0].Id, x => x.TrackPath, StringComparer.OrdinalIgnoreCase);
                _ambiguous = matches
                    .Where(x => x.Status == JellyfinMatchStatus.Ambiguous)
                    .SelectMany(match => match.Candidates.Select(candidate => (candidate.Id, match.Candidates)))
                    .ToLookup(x => x.Id, x => x.Candidates, StringComparer.OrdinalIgnoreCase);
                _byFileName = paths.ToLookup(x => JellyfinTrackMatcher.FileName(JellyfinTrackMatcher.Normalize(x)), StringComparer.OrdinalIgnoreCase);
            }

            public JellyfinPullMatch Match(JellyfinPlaylistEntry entry)
            {
                var here = _found[entry.ItemId].ToList();
                if (!_jellyfinPaths.TryGetValue(entry.ItemId, out var jellyfinPath))
                {
                    return new JellyfinPullMatch(entry.ItemId, entry.Name, "", JellyfinPullStatus.Unknown, "", []);
                }

                if (here.Count == 1)
                {
                    return new JellyfinPullMatch(entry.ItemId, entry.Name, jellyfinPath, JellyfinPullStatus.Found, here[0], []);
                }

                if (here.Count > 1)
                {
                    return new JellyfinPullMatch(entry.ItemId, entry.Name, jellyfinPath, JellyfinPullStatus.AmbiguousHere, "", here);
                }

                if (_ambiguous[entry.ItemId].FirstOrDefault() is { } candidates)
                {
                    return new JellyfinPullMatch(entry.ItemId, entry.Name, jellyfinPath, JellyfinPullStatus.AmbiguousInJellyfin, "", candidates.Select(x => x.Path).ToList());
                }

                var sameName = _byFileName[JellyfinTrackMatcher.FileName(JellyfinTrackMatcher.Normalize(jellyfinPath))].ToList();
                return new JellyfinPullMatch(entry.ItemId, entry.Name, jellyfinPath, JellyfinPullStatus.NotFound, "", sameName);
            }
        }
    }
}
