using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibLala.Jellyfin;

namespace Lalamachine.ViewModel
{
    // TrackCount: all of them, also the ones the library doesn't have at the moment.
    public sealed record LocalPlaylist(string Name, int TrackCount, bool HasUnsavedOrder);

    // The playlists here, as far as pulling one from Jellyfin needs them.
    public interface ILocalPlaylists
    {
        // Ignoring case, like the names here and in Jellyfin do. Null when there is none.
        LocalPlaylist? Find(string name);

        // Creates it when there is none. In the given order, each once.
        void Replace(string name, IReadOnlyList<string> trackPaths);
    }

    public interface IPlaylistPuller
    {
        // Lets the user choose a playlist in Jellyfin, and puts its tracks into the one here with the same name, or into a new one.
        // Tells the user how it went, a failure isn't an exception.
        Task PullAsync(ILocalPlaylists playlists);
    }

    // Goes by the saved settings, like pushing does.
    public sealed class JellyfinPlaylistPuller : IPlaylistPuller
    {
        private readonly JellyfinSettings _settings;
        private readonly Func<string, string, IJellyfinClient> _createClient;
        private readonly IDialogService _dialogs;
        private readonly Func<string> _libraryPath;
        private readonly Func<IEnumerable<string>> _libraryTrackPaths;

        public JellyfinPlaylistPuller(JellyfinSettings settings, Func<string, string, IJellyfinClient> createClient, IDialogService dialogs, Func<string> libraryPath, Func<IEnumerable<string>> libraryTrackPaths)
        {
            _settings = settings;
            _createClient = createClient;
            _dialogs = dialogs;
            _libraryPath = libraryPath;
            _libraryTrackPaths = libraryTrackPaths;
        }

        public async Task PullAsync(ILocalPlaylists playlists)
        {
            ArgumentNullException.ThrowIfNull(playlists);

            if (_settings.ServerAddress.Length == 0 || _settings.ApiKey.Length == 0 || _settings.UserId.Length == 0)
            {
                await _dialogs.ShowMessageAsync("Set up Jellyfin in the settings tab first: the address, the API key and your user, then save.").ConfigureAwait(true);
                return;
            }

            var libraryPath = _libraryPath();
            if (libraryPath.Length == 0)
            {
                await _dialogs.ShowMessageAsync("Choose the library folder in the settings tab first, tracks are found by the paths below it.").ConfigureAwait(true);
                return;
            }

            try
            {
                var client = _createClient(_settings.ServerAddress, _settings.ApiKey);
                if (await ChooseAsync(client).ConfigureAwait(true) is not { } chosen)
                {
                    return;
                }

                // Like a new playlist here would get it. The queue is stored as a playlist with that name.
                var name = chosen.Name.Trim();
                if (name == LibLala.Constants.MISCPLAYLISTNAME)
                {
                    await _dialogs.ShowMessageAsync($"A playlist here can't be called {name}. Rename it in Jellyfin first.").ConfigureAwait(true);
                    return;
                }

                // Read here, the database isn't thread-safe, and preparing goes on in the background.
                var libraryTrackPaths = _libraryTrackPaths().ToList();
                var plan = await new JellyfinPullPlanner(client, _settings.UserId).PrepareAsync(chosen, libraryPath, libraryTrackPaths).ConfigureAwait(true);
                if (plan.TrackCount == 0)
                {
                    await _dialogs.ShowMessageAsync($"The playlist {name} in Jellyfin has no tracks, so nothing was changed.").ConfigureAwait(true);
                    return;
                }

                if (plan.TrackPaths.Count == 0)
                {
                    await _dialogs.ShowMessageAsync(NothingFoundMessage(plan, name, libraryPath)).ConfigureAwait(true);
                    return;
                }

                // Only now, the playlists here can have changed while Jellyfin was being read.
                var local = playlists.Find(name);
                if (local is { HasUnsavedOrder: true })
                {
                    await _dialogs.ShowMessageAsync($"The playlist {local.Name} here has a new order that isn't saved. Save or discard it first, pulling replaces its tracks.").ConfigureAwait(true);
                    return;
                }

                // A new playlist with everything in it is what the button says, that needs no question.
                if ((local is not null || plan.Unresolved.Count > 0)
                    && !await _dialogs.ConfirmAsync(Question(plan, name, local, libraryPath)).ConfigureAwait(true))
                {
                    return;
                }

                // It keeps its name here, also when it only differs in case.
                playlists.Replace(local?.Name ?? name, plan.TrackPaths);
                await _dialogs.ShowMessageAsync(DoneMessage(plan, local?.Name ?? name, local is null)).ConfigureAwait(true);
            }
            catch (JellyfinException e)
            {
                await _dialogs.ShowMessageAsync(e.Message).ConfigureAwait(true);
            }
        }

        // Null when the user changes their mind, or when there's nothing to choose from.
        private async Task<JellyfinPlaylist?> ChooseAsync(IJellyfinClient client)
        {
            var playlists = (await client.GetPlaylistsAsync(_settings.UserId).ConfigureAwait(true))
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .OrderBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
            if (playlists.Count == 0)
            {
                await _dialogs.ShowMessageAsync($"Jellyfin has no playlists that {_settings.UserName} can see, on {_settings.ServerAddress}.").ConfigureAwait(true);
                return null;
            }

            var index = await _dialogs.ChooseAsync($"Which playlist to pull from Jellyfin, {Target}?", playlists.Select(x => x.Name).ToList()).ConfigureAwait(true);
            return index is { } i && i >= 0 && i < playlists.Count ? playlists[i] : null;
        }

        // These can be other ones than what the settings tab shows right now, so it's said each time.
        private string Target => $"on {_settings.ServerAddress} for {_settings.UserName}";

        private static string NothingFoundMessage(JellyfinPullPlan plan, string name, string libraryPath)
        {
            var message = new StringBuilder();
            message.AppendLine(CultureInfo.InvariantCulture, $"None of the tracks of {name} in Jellyfin are in the library here, so nothing was changed.");
            AppendIncompleteRead(message, plan);
            AppendUnresolved(message, libraryPath, plan.Unresolved);
            return message.ToString().TrimEnd();
        }

        private string Question(JellyfinPullPlan plan, string name, LocalPlaylist? local, string libraryPath)
        {
            var tracks = plan.Unresolved.Count == 0 ? $"the {plan.TrackCount}" : $"{plan.TrackPaths.Count} of the {plan.TrackCount}";
            var question = new StringBuilder();
            if (local is null)
            {
                question.AppendLine(CultureInfo.InvariantCulture, $"Create the playlist {name} here with {tracks} tracks it has in Jellyfin, {Target}?");
            }
            else
            {
                question.AppendLine(CultureInfo.InvariantCulture, $"Replace the {local.TrackCount} tracks of the playlist {local.Name} here with {tracks} it has in Jellyfin, {Target}?");
                question.AppendLine("Changes made to it here get lost.");
                if (local.Name == LibLala.Constants.LIKEDPLAYLISTNAME)
                {
                    question.AppendLine("The hearts of the tracks go by this playlist, so they change as well.");
                }
            }
            AppendIncompleteRead(question, plan);
            AppendUnresolved(question, libraryPath, plan.Unresolved);
            return question.ToString().TrimEnd();
        }

        private string DoneMessage(JellyfinPullPlan plan, string name, bool isNew)
        {
            var done = isNew
                ? $"Created the playlist {name} with {plan.TrackPaths.Count} tracks from Jellyfin, {Target}."
                : $"The playlist {name} here has these {plan.TrackPaths.Count} tracks from Jellyfin now, {Target}.";
            return plan.Unresolved.Count == 0 ? done : $"{done} {plan.Unresolved.Count} tracks were left out.";
        }

        // Only when it makes a difference: a track that wasn't read is one that can't be told apart from a video.
        private static void AppendIncompleteRead(StringBuilder message, JellyfinPullPlan plan)
        {
            if (plan.LibraryTracksNotRead > 0 && plan.Unresolved.Any(x => x.Status == JellyfinPullStatus.Unknown))
            {
                message.AppendLine(CultureInfo.InvariantCulture, $"{plan.LibraryTracksNotRead} of the tracks Jellyfin reports couldn't be read: they came without a path, or the library changed in the meantime. So tracks listed as not among its audio tracks can be there after all.");
            }
        }

        // The tracks that can't be pulled, and why.
        private static void AppendUnresolved(StringBuilder report, string libraryPath, IReadOnlyCollection<JellyfinPullMatch> matches)
        {
            var notFound = WithStatus(matches, JellyfinPullStatus.NotFound);
            JellyfinPlaylistCheck.AppendTracks(report, $"Not in the library here, below {libraryPath}", notFound, match => match.JellyfinPath, match =>
                match.Others.Take(JellyfinPlaylistCheck.MaxListedSameNames).Select(x => $"same file name here: {x}"));
            if (notFound.Count > 0)
            {
                report.AppendLine("The library here doesn't know about tracks added since its last scan.");
            }

            JellyfinPlaylistCheck.AppendTracks(report, "More than once in Jellyfin", WithStatus(matches, JellyfinPullStatus.AmbiguousInJellyfin), match => match.JellyfinPath, match => match.Others);
            JellyfinPlaylistCheck.AppendTracks(report, "More than once here", WithStatus(matches, JellyfinPullStatus.AmbiguousHere), match => match.JellyfinPath, match => match.Others);
            JellyfinPlaylistCheck.AppendTracks(report, "Not among Jellyfin's audio tracks", WithStatus(matches, JellyfinPullStatus.Unknown), match => match.Name.Length > 0 ? match.Name : match.ItemId, _ => []);
        }

        private static List<JellyfinPullMatch> WithStatus(IReadOnlyCollection<JellyfinPullMatch> matches, JellyfinPullStatus status)
        {
            return matches.Where(x => x.Status == status).ToList();
        }
    }
}
