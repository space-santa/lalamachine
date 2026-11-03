using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibLala.Jellyfin
{
    public enum JellyfinMatchStatus
    {
        Found,
        NotFound,
        // More than one track in Jellyfin has this path below its root. Not guessed, a wrong track would silently break a mix.
        Ambiguous,
        // Below no library folder, so there is no relative path to look for. E.g. it's still there from a library somewhere else.
        OutsideLibrary,
    }

    // Candidates: the track when found, all of them when ambiguous. When not found, the ones with the same file name in other folders, to see why.
    // JellyfinRoot: the folder in Jellyfin that the relative path is below, when found.
    public sealed record JellyfinTrackMatch(string TrackPath, JellyfinMatchStatus Status, string RelativePath, IReadOnlyList<JellyfinAudioItem> Candidates, string JellyfinRoot);

    // Finds the tracks of this computer in Jellyfin. The two have different root folders, e.g. D:\Music and /media/music, but
    // what's below them is the same. So it looks for the Jellyfin path that ends in the track's path relative to the library folder.
    public sealed class JellyfinTrackMatcher
    {
        private readonly string _libraryRoot;
        private readonly ILookup<string, (string Path, JellyfinAudioItem Item)> _itemsByFileName;

        public JellyfinTrackMatcher(string libraryRoot, IEnumerable<JellyfinAudioItem> items)
        {
            ArgumentNullException.ThrowIfNull(libraryRoot);
            ArgumentNullException.ThrowIfNull(items);

            _libraryRoot = $"{Normalize(libraryRoot).TrimEnd('/')}/";
            _itemsByFileName = items
                .Select(item => (Path: Normalize(item.Path), Item: item))
                .ToLookup(x => FileName(x.Path), StringComparer.OrdinalIgnoreCase);
        }

        public JellyfinTrackMatch Match(string trackPath)
        {
            ArgumentNullException.ThrowIfNull(trackPath);

            var path = Normalize(trackPath);
            if (!path.StartsWith(_libraryRoot, StringComparison.OrdinalIgnoreCase))
            {
                return new JellyfinTrackMatch(trackPath, JellyfinMatchStatus.OutsideLibrary, "", [], "");
            }

            var relativePath = path[_libraryRoot.Length..];
            // With the slash, so that A/b.mp3 doesn't match XA/b.mp3.
            return Result(trackPath, relativePath, x => x.EndsWith($"/{relativePath}", StringComparison.OrdinalIgnoreCase));
        }

        // One track alone can't tell when it's missing but its path is the end of another one deeper down, BPM/x.mp3 in
        // /media/music/Old/BPM/x.mp3. So the folder that most of the tracks are in decides, also between several matches of a track.
        // With as many in two folders it's the shorter one, the wrong one is the deeper one. A single track has nothing to go by.
        public IReadOnlyList<JellyfinTrackMatch> MatchAll(IEnumerable<string> trackPaths)
        {
            ArgumentNullException.ThrowIfNull(trackPaths);

            var matches = trackPaths.Select(Match).ToList();
            var root = matches
                .Where(x => x.Status == JellyfinMatchStatus.Found)
                .GroupBy(x => x.JellyfinRoot, StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(x => x.Count())
                .ThenBy(x => x.Key.Length)
                .FirstOrDefault()?.Key;
            if (root is null)
            {
                return matches;
            }

            return matches.Select(match => match.Status is JellyfinMatchStatus.Found or JellyfinMatchStatus.Ambiguous
                ? Result(match.TrackPath, match.RelativePath, x => x.Equals($"{root}{match.RelativePath}", StringComparison.OrdinalIgnoreCase))
                : match).ToList();
        }

        private JellyfinTrackMatch Result(string trackPath, string relativePath, Func<string, bool> isMatch)
        {
            var sameName = _itemsByFileName[FileName(relativePath)].ToList();
            var matches = sameName.Where(x => isMatch(x.Path)).ToList();

            return matches.Count switch
            {
                0 => new JellyfinTrackMatch(trackPath, JellyfinMatchStatus.NotFound, relativePath, sameName.Select(x => x.Item).ToList(), ""),
                1 => new JellyfinTrackMatch(trackPath, JellyfinMatchStatus.Found, relativePath, [matches[0].Item], matches[0].Path[..^relativePath.Length]),
                _ => new JellyfinTrackMatch(trackPath, JellyfinMatchStatus.Ambiguous, relativePath, matches.Select(x => x.Item).ToList(), ""),
            };
        }

        // Windows has backslashes, and macOS stores some letters as a base letter plus an accent. Case doesn't count either,
        // the same files can be on a file system that ignores it.
        private static string Normalize(string path)
        {
            return path.Replace('\\', '/').Normalize(NormalizationForm.FormC);
        }

        private static string FileName(string normalizedPath)
        {
            return normalizedPath[(normalizedPath.LastIndexOf('/') + 1)..];
        }
    }
}
