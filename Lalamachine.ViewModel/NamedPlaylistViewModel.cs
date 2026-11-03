using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using LalaDb.Model;
using LibLala.LibLalaTagReader;

namespace Lalamachine.ViewModel
{
    // One of the playlists in the playlists tab. Each has its own tracks, so that it can play on while another one is on
    // screen, and so that a new order stays around until it is saved or discarded.
    public sealed class NamedPlaylistViewModel : PlaylistBaseViewModel
    {
        private readonly PlaylistsViewModel _playlists;
        private readonly PlaylistModel _model;
        // The order in the database. Playlist is the one on screen.
        private List<PlaylistTags> _savedTracks = new List<PlaylistTags>();
        private bool _isLoaded;

        internal NamedPlaylistViewModel(string name, PlaylistsViewModel playlists, PlaylistModel model, IDialogService dialogs) : base(name, dialogs, playlists)
        {
            _playlists = playlists;
            _model = model;
            _saveOrderCommand = new DelegateCommand(OnSaveOrder);
            _discardOrderCommand = new DelegateCommand(OnDiscardOrder);

            PropertyChanged += PlaylistChangedHandler;
        }

        private void PlaylistChangedHandler(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Playlist))
            {
                NotifyPropertyChanged(nameof(HasUnsavedOrder));
            }
        }

        public bool IsLikedPlaylist => Name == LibLala.Constants.LIKEDPLAYLISTNAME;
        public bool CanRenameAndDelete => !IsLikedPlaylist;

        public bool HasUnsavedOrder => !Playlist.SequenceEqual(_savedTracks);

        private readonly DelegateCommand _saveOrderCommand;
        public ICommand SaveOrderCommand => _saveOrderCommand;
        private void OnSaveOrder(object? obj)
        {
            _model.SaveOrder(Name, Playlist.Select(x => x.Path));
            _savedTracks = Playlist.ToList();
            NotifyPropertyChanged(nameof(HasUnsavedOrder));
        }

        private readonly DelegateCommand _discardOrderCommand;
        public ICommand DiscardOrderCommand => _discardOrderCommand;
        private void OnDiscardOrder(object? obj)
        {
            ReplacePlaylist(_savedTracks);
            NotifyPropertyChanged(nameof(Playlist));
        }

        protected override void RemoveTrack(PlaylistTags track)
        {
            ArgumentNullException.ThrowIfNull(track);

            _playlists.SetContainsTrack(this, track, false);
        }

        internal void Rename(string name)
        {
            Name = name;
        }

        internal void EnsureLoaded()
        {
            if (!_isLoaded)
            {
                Load();
            }
        }

        // One that isn't loaded yet gets the new tracks anyway, when it does load.
        internal void Reload()
        {
            if (_isLoaded)
            {
                Load();
            }
        }

        // With new objects, a scan can have changed the tags of the tracks.
        private void Load()
        {
            var playingPath = Playlist.FirstOrDefault(x => x.IsPlaying)?.Path;
            _savedTracks = _model.GetPlaylistTracks(Name).Select(x => new PlaylistTags(x)).ToList();
            foreach (var track in _savedTracks)
            {
                track.IsPlaying = track.Path == playingPath;
            }
            MarkLiked(_savedTracks);
            _isLoaded = true;
            ReplacePlaylist(_savedTracks);
            NotifyPropertyChanged(nameof(Playlist));
        }

        // A playlist that isn't loaded yet gets its tracks from the database when it is.
        internal void TrackAdded(LibLalaTags track)
        {
            if (!_isLoaded) { return; }

            var added = new PlaylistTags(track);
            // First, so that adding it to the list doesn't look like a change of the order.
            _savedTracks.Add(added);
            AddTrack(added);
        }

        internal void TrackRemoved(string path)
        {
            _savedTracks.RemoveAll(x => x.Path == path);
            foreach (var track in Playlist.Where(x => x.Path == path).ToList())
            {
                base.RemoveTrack(track);
            }
        }

        internal void ClearPlaying()
        {
            foreach (var track in Playlist)
            {
                track.IsPlaying = false;
            }
        }

        // The playlist is gone. Without tracks, if it was playing there is just nothing to play next.
        internal void Close()
        {
            TrackPlaylists.LikedChanged -= LikedChangedHandler;
            _savedTracks.Clear();
            DeleteAllTracks();
        }
    }
}
