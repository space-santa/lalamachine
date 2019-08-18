using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Windows.Input;
using LalaDb.Data;
using LalaDb.Model;
using LibLala.TagReader;

namespace Lalamachine.Wpf.ViewModel
{
    public class PlayTrackEventArgs : EventArgs
    {
        public string Path { get; set; }
        public string ListName { get; set; }
    }

    public class PlaylistTags : Tags, INotifyPropertyChanged
    {
        public PlaylistTags() : base() { }
        public PlaylistTags(Tags other) : base(other) { }
        public PlaylistTags(PlaylistTags other) : base(other) { IsPlaying = other.IsPlaying; }

        private bool isPlaying;

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool IsPlaying { get => isPlaying; set { isPlaying = value; NotifyPropertyChanged(); } }
    }

    public class PlaylistViewModel : INotifyPropertyChanged
    {
        private readonly PlaylistModel _playlistModel;
        public PlaylistViewModel()
        {
            SetUp("LIBRARY");
        }

        public PlaylistViewModel(LalaContext context)
        {
            _playlistModel = new PlaylistModel(context);
            SetUp("MAIN");
            LoadLastPlaylist();
        }

        private void SetUp(string name)
        {
            Name = name;
            _playlist = new ObservableCollection<PlaylistTags>();
            _playTrackCommand = new DelegateCommand(OnPlayTrackCommandHandler);
            _removeTrackCommand = new DelegateCommand(OnRemoveTrackCommandHandler);
            _removeAllTracksCommand = new DelegateCommand(OnRemoveAllTrackCommandHandler);
            _createNewPlaylistFromSelectionCommand = new DelegateCommand(OnCreateNewPlaylistFromSelectionCommand);
            _addSelectionToPlaylistCommand = new DelegateCommand(OnAddSelectionToPlaylistCommand);
        }

        private void LoadLastPlaylist()
        {
            var tracks = _playlistModel.getPlaylistTracks(LibLala.Constants.MISC_PLAYLIST_NAME);
            AddTracks(tracks);
        }

        #region Commands
        private DelegateCommand _createNewPlaylistFromSelectionCommand;
        public ICommand CreateNewPlaylistFromSelectionCommand => _createNewPlaylistFromSelectionCommand;
        private void OnCreateNewPlaylistFromSelectionCommand(object obj)
        {
            InvokeAddLibraryTracksToPlaylistEvent(true, ConvertSelectedItemsToTagsList(obj));
        }

        private DelegateCommand _addSelectionToPlaylistCommand;
        public ICommand AddSelectionToPlaylistCommand => _addSelectionToPlaylistCommand;
        private void OnAddSelectionToPlaylistCommand(object obj)
        {
            InvokeAddLibraryTracksToPlaylistEvent(false, ConvertSelectedItemsToTagsList(obj));
        }
        #endregion

        private List<Tags> ConvertSelectedItemsToTagsList(object obj)
        {
            var items = (System.Collections.IList)obj;
            var tagsList = new List<Tags>();
            foreach (PlaylistTags tags in items)
            {
                tagsList.Add(tags);
            }
            return tagsList;
        }

        #region events
        public event EventHandler<AddTracksToPlaylistEventArgs> AddLibraryTracksToPlaylistEvent;
        private void InvokeAddLibraryTracksToPlaylistEvent(bool newPlaylist, List<Tags> tags)
        {
            var args = new AddTracksToPlaylistEventArgs { NewPlaylist = newPlaylist, Tracks = tags };
            AddLibraryTracksToPlaylistEvent?.Invoke(this, args);
        }
        #endregion
        private void OnRemoveAllTrackCommandHandler(object obj)
        {
            DeleteAllTracks();
        }

        private void OnRemoveTrackCommandHandler(object obj)
        {
            var selectedItem = (PlaylistTags)obj;
            _playlist.Remove(selectedItem);
        }

        private void OnPlayTrackCommandHandler(object obj)
        {
            var track = (PlaylistTags)obj;
            OnPlayTrack(track);
        }

        private ObservableCollection<PlaylistTags> _playlist;

        public int CurrentIndex => Playlist.IndexOf(Playlist.FirstOrDefault(x => x.IsPlaying));

        internal void DisplayChangedHandler(object sender, DisplayLibChangedEventArgs e)
        {
            DeleteAllTracks();
            AddTracks(e.Tracks.ToList());
        }

        private DelegateCommand _playTrackCommand;
        public ICommand PlayTrackCommand => _playTrackCommand;

        private DelegateCommand _removeTrackCommand;
        public ICommand RemoveTrackCommand => _removeTrackCommand;

        internal void AddTracksToPlaylistHandler(object sender, AddTracksToPlaylistEventArgs e)
        {
            if (e.NewPlaylist) { DeleteAllTracks(); }
            AddTracks(e.Tracks);
        }

        private DelegateCommand _removeAllTracksCommand;
        public ICommand RemoveAllTracksCommand => _removeAllTracksCommand;

        public event EventHandler<PlayTrackEventArgs> PlayTrackEvent;
        protected virtual void OnPlayTrack(PlaylistTags tags)
        {
            foreach (var item in _playlist)
            {
                item.IsPlaying = false;
            }
            tags.IsPlaying = true;
            NotifyPropertyChanged("CurrentIndex");
            PlayTrackEvent?.Invoke(this, new PlayTrackEventArgs { Path = tags.path, ListName = Name });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void SavePlaylist()
        {
            if (_playlistModel != null)
            {
                var tagsList = new List<Tags>();
                foreach (var item in Playlist) { tagsList.Add(item); }
                _playlistModel.savePlaylist(LibLala.Constants.MISC_PLAYLIST_NAME, tagsList);
            }
        }

        public PlaylistTags CurrentTrack => Playlist[CurrentIndex];
        public PlaylistTags NextTrack
        {
            get
            {
                if (PlaylistShuffleRepeatState == ShuffleRepeatState.RepeatOne)
                {
                    return Playlist[CurrentIndex];
                }

                if (PlaylistShuffleRepeatState == ShuffleRepeatState.Shuffle)
                {
                    var newIndex = new Random().Next(0, _playlist.Count - 1);
                    return Playlist[newIndex];
                }

                if (CurrentIndex < Playlist.Count - 1)
                {
                    return Playlist[CurrentIndex + 1];
                }

                if (CurrentIndex != Playlist.Count - 1)
                {
                    throw new InvalidOperationException("Playlist must be at the end to get here.");
                }
                if (PlaylistShuffleRepeatState == ShuffleRepeatState.RepeatAll)
                {
                    return Playlist[0];
                }

                throw new EndOfPlaylistException();
            }
        }
        public PlaylistTags PreviousTrack
        {
            get
            {
                var previousIndex = 0;
                if (CurrentIndex > 0)
                {
                    previousIndex = CurrentIndex - 1;
                }
                return Playlist[previousIndex];
            }
        }

        public ObservableCollection<PlaylistTags> Playlist
        {
            get => _playlist;
            set
            {
                _playlist = value;
                NotifyPropertyChanged();
            }
        }

        public ShuffleRepeatState PlaylistShuffleRepeatState { get; set; }

        public void AddTrack(Tags tags)
        {
            AddTrack(new PlaylistTags(tags));
        }
        public void AddTrack(PlaylistTags tags)
        {
            Playlist.Add(tags);
            NotifyPropertyChanged("Playlist");
        }

        public void AddTracks(List<LalaTags> tagsRange)
        {
            foreach (var tags in tagsRange)
            {
                AddTrack(tags);
            }
            NotifyPropertyChanged("Playlist");
        }

        public void AddTracks(List<Tags> tagsRange)
        {
            foreach (var tags in tagsRange)
            {
                AddTrack(tags);
            }
            NotifyPropertyChanged("Playlist");
        }
        public void AddTracks(List<PlaylistTags> tagsRange)
        {
            foreach (var tags in tagsRange)
            {
                AddTrack(tags);
            }
            NotifyPropertyChanged("Playlist");
        }

        public void DeleteTrack(int index)
        {
            Playlist.RemoveAt(index);
            NotifyPropertyChanged("Playlist");
        }

        public void DeleteTracks(int startIndex, int numberOfTracks)
        {
            for (var i = 0; i < numberOfTracks; ++i)
            {
                DeleteTrack(startIndex);
            }
            NotifyPropertyChanged("Playlist");
        }

        public void DeleteAllTracks()
        {
            Playlist.Clear();
            NotifyPropertyChanged("Playlist");
        }

        public void ManualLoadHandler(object sender, ManualLoadEventArgs e)
        {
            var path = e.Path;
            var tags = new TagReader().Read(path);
            AddTrack(tags);
        }

        public void PlayNextTrackHandler(object sender, EventArgs e)
        {
            if (HasTracks)
            {
                try
                {
                    OnPlayTrack(NextTrack);
                }
                catch (EndOfPlaylistException)
                {
                    // Nothing really.
                }
            }
        }

        public void PlayLastTrackHandler(object sender, EventArgs e)
        {
            if (HasTracks)
            {
                OnPlayTrack(PreviousTrack);
            }
        }

        public bool HasTracks => _playlist.Count > 0;
        public string Name { get; private set; }

        public void ShuffleRepeatChangedHandler(object sender, ChangeShuffleRepeatEventArgs e)
        {
            PlaylistShuffleRepeatState = e.ShuffleRepeatState;
        }
    }

    [Serializable]
    internal class EndOfPlaylistException : Exception
    {
        public EndOfPlaylistException()
        {
        }

        public EndOfPlaylistException(string message) : base(message)
        {
        }

        public EndOfPlaylistException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected EndOfPlaylistException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
