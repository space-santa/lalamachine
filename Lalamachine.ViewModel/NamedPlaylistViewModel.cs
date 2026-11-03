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

            _playlists.SetContainsTracks(this, [track], false);
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

        // With new objects, a scan can have changed the tags of the tracks. A new order that isn't saved stays.
        private void Load()
        {
            var shownTracks = Playlist.ToList();
            var hadUnsavedOrder = HasUnsavedOrder;
            var playingPath = shownTracks.FirstOrDefault(x => x.IsPlaying)?.Path;
            var playingPosition = PlayingPosition;

            _savedTracks = _model.GetPlaylistTracks(Name).Select(x => new PlaylistTags(x)).ToList();
            foreach (var track in _savedTracks)
            {
                track.IsPlaying = track.Path == playingPath;
            }
            MarkLiked(_savedTracks);
            _isLoaded = true;

            // A playing track that the scan left out is like one that got removed, next and previous go on from its place.
            var paths = _savedTracks.Select(x => x.Path).ToHashSet();
            var removedPlayingIndex = playingPosition < 0 || _savedTracks.Any(x => x.IsPlaying)
                ? -1
                : shownTracks.Take(playingPosition).Count(x => paths.Contains(x.Path));
            ReplacePlaylist(hadUnsavedOrder ? SavedTracksInOrderOf(shownTracks) : _savedTracks, removedPlayingIndex);
            NotifyPropertyChanged(nameof(Playlist));
        }

        // Ones that weren't there before come back with a scan, they go to the end.
        private IEnumerable<PlaylistTags> SavedTracksInOrderOf(IEnumerable<PlaylistTags> tracks)
        {
            var positions = new Dictionary<string, int>();
            foreach (var track in tracks)
            {
                positions.TryAdd(track.Path, positions.Count);
            }
            return _savedTracks.OrderBy(x => positions.GetValueOrDefault(x.Path, int.MaxValue));
        }

        // A playlist that isn't loaded yet gets its tracks from the database when it is.
        internal void TracksAdded(IEnumerable<LibLalaTags> tracks)
        {
            if (!_isLoaded) { return; }

            // Some of them can be there already, and it has each track once.
            var paths = _savedTracks.Select(x => x.Path).ToHashSet();
            var added = tracks.Where(x => paths.Add(x.Path)).Select(x => new PlaylistTags(x)).ToList();
            // First, so that adding them to the list doesn't look like a change of the order.
            _savedTracks.AddRange(added);
            AddTracks(added);
        }

        internal void TracksRemoved(IReadOnlySet<string> paths)
        {
            _savedTracks.RemoveAll(x => paths.Contains(x.Path));
            foreach (var track in Playlist.Where(x => paths.Contains(x.Path)).ToList())
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

        // The playlist is gone.
        internal void Close()
        {
            TrackPlaylists.LikedChanged -= LikedChangedHandler;
            _savedTracks.Clear();
            DeleteAllTracks();
        }
    }
}
