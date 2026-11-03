using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibLala.Jellyfin;

namespace Lalamachine.ViewModel
{
    public interface IPlaylistPusher
    {
        // The paths in the order of the playlist. Tells the user how it went, a failure isn't an exception.
        Task PushAsync(string playlistName, IReadOnlyList<string> trackPaths);
    }

    // Goes by the saved settings, not by what's typed into the settings tab right now.
    public sealed class JellyfinPlaylistPusher : IPlaylistPusher
    {
        private readonly JellyfinSettings _settings;
        private readonly Func<string, string, IJellyfinClient> _createClient;
        private readonly IDialogService _dialogs;
        private readonly Func<string> _libraryPath;

        public JellyfinPlaylistPusher(JellyfinSettings settings, Func<string, string, IJellyfinClient> createClient, IDialogService dialogs, Func<string> libraryPath)
        {
            _settings = settings;
            _createClient = createClient;
            _dialogs = dialogs;
            _libraryPath = libraryPath;
        }

        public async Task PushAsync(string playlistName, IReadOnlyList<string> trackPaths)
        {
            ArgumentNullException.ThrowIfNull(trackPaths);

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

            if (trackPaths.Count == 0)
            {
                await _dialogs.ShowMessageAsync($"The playlist {playlistName} has no tracks, so there is nothing to push.").ConfigureAwait(true);
                return;
            }

            try
            {
                var publisher = new JellyfinPlaylistPublisher(_createClient(_settings.ServerAddress, _settings.ApiKey), _settings.UserId);
                var plan = await publisher.PrepareAsync(playlistName, libraryPath, trackPaths).ConfigureAwait(true);

                if (plan.ItemIds.Count == 0)
                {
                    await _dialogs.ShowMessageAsync(NothingFoundMessage(plan, libraryPath)).ConfigureAwait(true);
                    return;
                }

                // A new playlist with everything in it is what the button says, that needs no question.
                if ((plan.ExistingPlaylistId is not null || plan.Unresolved.Count > 0)
                    && !await _dialogs.ConfirmAsync(Question(plan, libraryPath, trackPaths.Count)).ConfigureAwait(true))
                {
                    return;
                }

                await publisher.PublishAsync(plan).ConfigureAwait(true);
                await _dialogs.ShowMessageAsync(DoneMessage(plan)).ConfigureAwait(true);
            }
            catch (JellyfinException e)
            {
                await _dialogs.ShowMessageAsync(e.Message).ConfigureAwait(true);
            }
        }

        private static string NothingFoundMessage(JellyfinPushPlan plan, string libraryPath)
        {
            var message = new StringBuilder();
            message.AppendLine(CultureInfo.InvariantCulture, $"None of the tracks of {plan.PlaylistName} are in Jellyfin, so nothing was changed.");
            AppendIncompleteRead(message, plan);
            JellyfinPlaylistCheck.AppendUnresolved(message, libraryPath, plan.Unresolved);
            return message.ToString().TrimEnd();
        }

        // Only when it makes a difference. When everything was found, it doesn't matter what else the server has.
        private static void AppendIncompleteRead(StringBuilder message, JellyfinPushPlan plan)
        {
            if (plan.LibraryTracksNotRead > 0 && plan.Unresolved.Any(x => x.Status == JellyfinMatchStatus.NotFound))
            {
                message.AppendLine(CultureInfo.InvariantCulture, $"{plan.LibraryTracksNotRead} of the tracks Jellyfin reports couldn't be read: they came without a path, or the library changed in the meantime. So tracks listed as not found can be there after all.");
            }
        }

        // These can be other ones than what the settings tab shows right now, so it's said each time.
        private string Target => $"on {_settings.ServerAddress} for {_settings.UserName}";

        private string Question(JellyfinPushPlan plan, string libraryPath, int trackCount)
        {
            var question = new StringBuilder();
            if (plan.ExistingPlaylistId is null)
            {
                question.AppendLine(CultureInfo.InvariantCulture, $"Create the playlist {plan.PlaylistName} in Jellyfin with {plan.ItemIds.Count} of its {trackCount} tracks, {Target}?");
            }
            else
            {
                question.AppendLine(CultureInfo.InvariantCulture, $"Replace the {plan.ExistingTrackCount} tracks of the playlist {plan.PlaylistName} in Jellyfin with these {plan.ItemIds.Count}, {Target}?");
                question.AppendLine("Changes made to it in Jellyfin get lost.");
            }
            AppendIncompleteRead(question, plan);
            JellyfinPlaylistCheck.AppendUnresolved(question, libraryPath, plan.Unresolved);
            return question.ToString().TrimEnd();
        }

        private string DoneMessage(JellyfinPushPlan plan)
        {
            var done = plan.ExistingPlaylistId is null
                ? $"Created the playlist {plan.PlaylistName} in Jellyfin with {plan.ItemIds.Count} tracks, {Target}."
                : $"The playlist {plan.PlaylistName} in Jellyfin has these {plan.ItemIds.Count} tracks now, {Target}.";
            return plan.Unresolved.Count == 0 ? done : $"{done} {plan.Unresolved.Count} tracks were left out.";
        }
    }
}
