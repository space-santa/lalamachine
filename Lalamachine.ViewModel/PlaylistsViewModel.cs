using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using LalaDb.Data;
using LalaDb.Model;
using LibLala.LibLalaTagReader;

namespace Lalamachine.ViewModel
{
    public sealed class PlaylistsViewModel : BaseNotifyPropertyChanged, ITrackPlaylists
    {
        // For the player to know where to get the next track from.
        public const string ListName = "PLAYLISTS";

        private readonly PlaylistModel _model;
        private readonly IDialogService _dialogs;
        private readonly HashSet<string> _likedPaths;
        private NamedPlaylistViewModel? _selectedPlaylist;
        // Not always the selected one, looking at another playlist doesn't stop this one from playing on.
        private NamedPlaylistViewModel? _playingPlaylist;
        private ShuffleRepeatState _shuffleRepeatState;

        public PlaylistsViewModel(LalaContext context, IDialogService dialogs)
        {
            _model = new PlaylistModel(context);
            _dialogs = dialogs;
            _addPlaylistCommand = new DelegateCommand(OnAddPlaylist);
            _renamePlaylistCommand = new DelegateCommand(OnRenamePlaylist);
            _deletePlaylistCommand = new DelegateCommand(OnDeletePlaylist);

            var names = _model.PlaylistNames();
            if (!names.Contains(LibLala.Constants.LIKEDPLAYLISTNAME))
            {
                _model.CreatePlaylist(LibLala.Constants.LIKEDPLAYLISTNAME);
                names.Add(LibLala.Constants.LIKEDPLAYLISTNAME);
            }
            _likedPaths = _model.TrackPaths(LibLala.Constants.LIKEDPLAYLISTNAME).ToHashSet();

            Playlists = new ObservableCollection<NamedPlaylistViewModel>(
                names
                    .OrderBy(x => x != LibLala.Constants.LIKEDPLAYLISTNAME)
                    .ThenBy(x => x, StringComparer.CurrentCultureIgnoreCase)
                    .Select(CreatePlaylist)
            );
            SelectedPlaylist = Playlists[0];
        }

        public ObservableCollection<NamedPlaylistViewModel> Playlists { get; }

        public NamedPlaylistViewModel? SelectedPlaylist
        {
            get => _selectedPlaylist;
            set
            {
                if (value == _selectedPlaylist) { return; }
                _selectedPlaylist = value;
                _selectedPlaylist?.EnsureLoaded();
                NotifyPropertyChanged();
            }
        }

        #region Commands
        private readonly DelegateCommand _addPlaylistCommand;
        public ICommand AddPlaylistCommand => _addPlaylistCommand;
        private async void OnAddPlaylist(object? obj)
        {
            var playlist = await AskForNewPlaylistAsync().ConfigureAwait(true);
            if (playlist is { })
            {
                SelectedPlaylist = playlist;
            }
        }

        private readonly DelegateCommand _renamePlaylistCommand;
        public ICommand RenamePlaylistCommand => _renamePlaylistCommand;
        private async void OnRenamePlaylist(object? obj)
        {
            if (SelectedPlaylist is not { CanRenameAndDelete: true } playlist) { return; }

            var name = await AskForNameAsync("New name of the playlist:", playlist.Name).ConfigureAwait(true);
            if (name is null) { return; }

            _model.RenamePlaylist(playlist.Name, name);
            playlist.Rename(name);
            Playlists.Move(Playlists.IndexOf(playlist), IndexFor(name, playlist));
            SelectedPlaylist = playlist;
        }

        private readonly DelegateCommand _deletePlaylistCommand;
        public ICommand DeletePlaylistCommand => _deletePlaylistCommand;
        private async void OnDeletePlaylist(object? obj)
        {
            if (SelectedPlaylist is not { CanRenameAndDelete: true } playlist) { return; }

            var isConfirmed = await _dialogs.ConfirmAsync($"Delete the playlist {playlist.Name}?").ConfigureAwait(true);
            if (!isConfirmed) { return; }

            _model.DeletePlaylist(playlist.Name);
            var index = Playlists.IndexOf(playlist);
            // Never the only one, Liked is always there.
            SelectedPlaylist = Playlists[index < Playlists.Count - 1 ? index + 1 : index - 1];
            Playlists.RemoveAt(index);
            playlist.Close();
        }
        #endregion

        #region Events
        public event EventHandler<LikedChangedEventArgs>? LikedChanged;

        public event EventHandler<PlayTrackEventArgs>? PlayTrackEvent;
        private void PlaylistPlayTrackHandler(object? sender, PlayTrackEventArgs e)
        {
            if (sender != _playingPlaylist)
            {
                _playingPlaylist?.ClearPlaying();
                _playingPlaylist = sender as NamedPlaylistViewModel;
            }

            PlayTrackEvent?.Invoke(this, new PlayTrackEventArgs(e.Path, ListName));
        }

        public event EventHandler<AddTracksToQueueEventArgs>? AddTracksToQueueEvent;
        private void PlaylistAddTracksToQueueHandler(object? sender, AddTracksToQueueEventArgs e)
        {
            AddTracksToQueueEvent?.Invoke(this, e);
        }
        #endregion

        public void PlayNextTrackHandler(object? sender, EventArgs e)
        {
            (_playingPlaylist ?? SelectedPlaylist)?.PlayNextTrackHandler(sender, e);
        }

        public void PlayLastTrackHandler(object? sender, EventArgs e)
        {
            (_playingPlaylist ?? SelectedPlaylist)?.PlayLastTrackHandler(sender, e);
        }

        public void ShuffleRepeatChangedHandler(object? sender, ChangeShuffleRepeatEventArgs e)
        {
            if (e is null)
            {
                return;
            }

            _shuffleRepeatState = e.ShuffleRepeatState;
            foreach (var playlist in Playlists)
            {
                playlist.ShuffleRepeatChangedHandler(sender, e);
            }
        }

        // A scan replaces the library, so the tracks can be gone or have new tags. What a playlist contains stays, that goes by the path.
        public void ScanningChangedHandler(object? sender, ScanningChangedEventArgs e)
        {
            if (e is null || e.Scanning)
            {
                return;
            }

            foreach (var playlist in Playlists)
            {
                playlist.Reload();
            }
        }

        public bool IsLiked(string path)
        {
            return _likedPaths.Contains(path);
        }

        public void ToggleLiked(LibLalaTags track)
        {
            ArgumentNullException.ThrowIfNull(track);

            var liked = Playlists.Single(x => x.IsLikedPlaylist);
            SetContainsTrack(liked, track, !IsLiked(track.Path));
        }

        public TrackPlaylistChoices ChoicesFor(LibLalaTags track)
        {
            ArgumentNullException.ThrowIfNull(track);

            var containing = _model.PlaylistsContaining(track.Path);
            var choices = new ObservableCollection<PlaylistChoice>(
                Playlists.Select(playlist => CreateChoice(playlist, track, containing.Contains(playlist.Name)))
            );

            async void OnNewPlaylist(object? obj)
            {
                var playlist = await AskForNewPlaylistAsync().ConfigureAwait(true);
                if (playlist is { })
                {
                    SetContainsTrack(playlist, track, true);
                    choices.Insert(Playlists.IndexOf(playlist), CreateChoice(playlist, track, true));
                }
            }

            return new TrackPlaylistChoices(choices, new DelegateCommand(OnNewPlaylist));
        }

        public async Task SaveAsNewPlaylistAsync(IEnumerable<LibLalaTags> tracks)
        {
            ArgumentNullException.ThrowIfNull(tracks);

            var playlist = await AskForNewPlaylistAsync().ConfigureAwait(true);
            if (playlist is null)
            {
                return;
            }

            // The queue can have a track more than once, a playlist can't.
            _model.SavePlaylist(playlist.Name, tracks.DistinctBy(x => x.Path).ToList());
            SelectedPlaylist = playlist;
        }

        internal void SetContainsTrack(NamedPlaylistViewModel playlist, LibLalaTags track, bool containsTrack)
        {
            // Before the playlist is told, that's what the heart of its new track goes by.
            if (playlist.IsLikedPlaylist)
            {
                if (containsTrack)
                {
                    _likedPaths.Add(track.Path);
                }
                else
                {
                    _likedPaths.Remove(track.Path);
                }
            }

            if (containsTrack)
            {
                _model.AddTrack(playlist.Name, track.Path);
                playlist.TrackAdded(track);
            }
            else
            {
                _model.RemoveTrack(playlist.Name, track.Path);
                playlist.TrackRemoved(track.Path);
            }

            if (playlist.IsLikedPlaylist)
            {
                LikedChanged?.Invoke(this, new LikedChangedEventArgs(track.Path, containsTrack));
            }
        }

        private PlaylistChoice CreateChoice(NamedPlaylistViewModel playlist, LibLalaTags track, bool containsTrack)
        {
            return new PlaylistChoice(playlist.Name, containsTrack, value => SetContainsTrack(playlist, track, value));
        }

        private NamedPlaylistViewModel CreatePlaylist(string name)
        {
            var playlist = new NamedPlaylistViewModel(name, this, _model, _dialogs) { PlaylistShuffleRepeatState = _shuffleRepeatState };
            playlist.PlayTrackEvent += PlaylistPlayTrackHandler;
            playlist.AddTracksToQueueEvent += PlaylistAddTracksToQueueHandler;
            return playlist;
        }

        // Null when the user changes their mind or the name is taken.
        private async Task<NamedPlaylistViewModel?> AskForNewPlaylistAsync()
        {
            var name = await AskForNameAsync("Name of the new playlist:", "").ConfigureAwait(true);
            if (name is null)
            {
                return null;
            }

            _model.CreatePlaylist(name);
            var playlist = CreatePlaylist(name);
            Playlists.Insert(IndexFor(name), playlist);
            return playlist;
        }

        private async Task<string?> AskForNameAsync(string prompt, string currentName)
        {
            var name = (await _dialogs.AskForTextAsync(prompt, currentName).ConfigureAwait(true))?.Trim();
            if (string.IsNullOrEmpty(name) || name == currentName)
            {
                return null;
            }

            if (IsNameTaken(name, currentName))
            {
                await _dialogs.ShowMessageAsync($"There already is a playlist called {name}.").ConfigureAwait(true);
                return null;
            }

            return name;
        }

        // Changing only the case of its own name is fine. The queue's name would turn that playlist into the queue.
        private bool IsNameTaken(string name, string currentName)
        {
            return name == LibLala.Constants.MISCPLAYLISTNAME
                || Playlists.Any(x => x.Name != currentName && x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        // Liked comes first, the others by name.
        private int IndexFor(string name, NamedPlaylistViewModel? playlistToMove = null)
        {
            return Playlists.Count(x => x != playlistToMove && (x.IsLikedPlaylist || StringComparer.CurrentCultureIgnoreCase.Compare(x.Name, name) < 0));
        }
    }
}
