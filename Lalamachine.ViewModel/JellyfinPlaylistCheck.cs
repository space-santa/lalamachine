using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using LibLala.Jellyfin;

namespace Lalamachine.ViewModel
{
    public sealed record PlaylistPaths(string Name, IEnumerable<string> TrackPaths);

    // What pushing the playlists would find in Jellyfin. Only reads, and only makes a text for the user to look at.
    public static class JellyfinPlaylistCheck
    {
        // A wrong library folder can make it thousands of tracks. Some are enough to see what's going on.
        private const int MaxListedTracks = 50;
        private const int MaxListedSameNames = 3;

        public static string Report(string libraryPath, JellyfinAudioLibrary library, string userName, IEnumerable<PlaylistPaths> playlists)
        {
            ArgumentNullException.ThrowIfNull(library);
            ArgumentNullException.ThrowIfNull(playlists);

            var matcher = new JellyfinTrackMatcher(libraryPath, library.Items);
            // A track can be in several playlists, the totals and lists have it once.
            var matches = new Dictionary<string, JellyfinTrackMatch>();
            var report = new StringBuilder();

            report.AppendLine(CultureInfo.InvariantCulture, $"Jellyfin has {library.Items.Count} tracks for {userName}.");
            if (library.ReportedTotal != library.Items.Count)
            {
                report.AppendLine(CultureInfo.InvariantCulture, $"It reports {library.ReportedTotal}, so some are missing below: they came without a path, or the library changed while reading it.");
            }
            report.AppendLine();

            foreach (var playlist in playlists.OrderBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase))
            {
                var paths = playlist.TrackPaths.ToList();
                if (paths.Count == 0)
                {
                    continue;
                }

                var found = 0;
                foreach (var path in paths)
                {
                    if (!matches.TryGetValue(path, out var match))
                    {
                        match = matcher.Match(path);
                        matches[path] = match;
                    }
                    found += match.Status == JellyfinMatchStatus.Found ? 1 : 0;
                }
                report.AppendLine(CultureInfo.InvariantCulture, $"{playlist.Name}: {found} of {paths.Count}");
            }

            if (matches.Count == 0)
            {
                report.AppendLine("The playlists have no tracks.");
            }
            else
            {
                var foundTracks = matches.Values.Where(x => x.Status == JellyfinMatchStatus.Found).ToList();
                report.AppendLine();
                report.AppendLine(CultureInfo.InvariantCulture, $"Altogether {foundTracks.Count} of {matches.Count} different tracks found.");
                foreach (var root in foundTracks.GroupBy(x => x.JellyfinRoot, StringComparer.OrdinalIgnoreCase).OrderByDescending(x => x.Count()))
                {
                    report.AppendLine(CultureInfo.InvariantCulture, $"In {root.Key}: {root.Count()}");
                }

                AppendUnresolved(report, libraryPath, matches.Values.ToList());
            }

            report.AppendLine();
            report.Append("Nothing was changed, neither in Jellyfin nor here.");
            return report.ToString();
        }

        // The tracks that can't be pushed, and why.
        internal static void AppendUnresolved(StringBuilder report, string libraryPath, IReadOnlyCollection<JellyfinTrackMatch> matches)
        {
            var notFound = WithStatus(matches, JellyfinMatchStatus.NotFound);
            AppendTracks(report, "Not found", notFound, match => match.RelativePath, match =>
                match.Candidates.Take(MaxListedSameNames).Select(x => $"same file name in Jellyfin: {x.Path}"));
            if (notFound.Count > 0)
            {
                report.AppendLine("Jellyfin doesn't know about tracks added since its last library scan.");
            }

            AppendTracks(report, "More than once in Jellyfin", WithStatus(matches, JellyfinMatchStatus.Ambiguous), match => match.RelativePath, match =>
                match.Candidates.Select(x => x.Path));
            AppendTracks(report, $"Outside the library folder {libraryPath}", WithStatus(matches, JellyfinMatchStatus.OutsideLibrary), match => match.TrackPath, _ => []);
        }

        private static List<JellyfinTrackMatch> WithStatus(IReadOnlyCollection<JellyfinTrackMatch> matches, JellyfinMatchStatus status)
        {
            return matches.Where(x => x.Status == status).ToList();
        }

        private static void AppendTracks(StringBuilder report, string title, List<JellyfinTrackMatch> tracks, Func<JellyfinTrackMatch, string> name, Func<JellyfinTrackMatch, IEnumerable<string>> details)
        {
            if (tracks.Count == 0)
            {
                return;
            }

            report.AppendLine();
            report.AppendLine(CultureInfo.InvariantCulture, $"{title} ({tracks.Count}):");
            foreach (var track in tracks.OrderBy(name, StringComparer.OrdinalIgnoreCase).Take(MaxListedTracks))
            {
                report.AppendLine(name(track));
                foreach (var detail in details(track))
                {
                    report.AppendLine(CultureInfo.InvariantCulture, $"    {detail}");
                }
            }
            if (tracks.Count > MaxListedTracks)
            {
                report.AppendLine(CultureInfo.InvariantCulture, $"...and {tracks.Count - MaxListedTracks} more.");
            }
        }
    }
}
