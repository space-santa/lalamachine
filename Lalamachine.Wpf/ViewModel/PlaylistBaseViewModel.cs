using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Linq;
using System.Windows.Input;
using LalaDb.Data;
using LalaDb.Model;
using LibLala.TagReader;

namespace Lalamachine.Wpf.ViewModel
{
    public class PlaylistBaseViewModel : INotifyPropertyChanged
    {
        public PlaylistBaseViewModel(string name)
        {
            Name = name;
            _playlist = new ObservableCollection<PlaylistTags>();
            _playTrackCommand = new DelegateCommand(OnPlayTrackCommandHandler);
            _removeTrackCommand = new DelegateCommand(OnRemoveTrackCommandHandler);
            _removeAllTracksCommand = new DelegateCommand(OnRemoveAllTrackCommandHandler);
            _createNewPlaylistFromSelectionCommand = new DelegateCommand(OnCreateNewPlaylistFromSelectionCommand);
            _addSelectionToPlaylistCommand = new DelegateCommand(OnAddSelectionToPlaylistCommand);
        }

        #region Commands
        private DelegateCommand _playTrackCommand;
        public ICommand PlayTrackCommand => _playTrackCommand;
        private void OnPlayTrackCommandHandler(object obj)
        {
            var track = (PlaylistTags)obj;
            InvokePlayTrackEvent(track);
        }

        private DelegateCommand _removeTrackCommand;
        public ICommand RemoveTrackCommand => _removeTrackCommand;
        private void OnRemoveTrackCommandHandler(object obj)
        {
            var selectedItem = (PlaylistTags)obj;
            _playlist.Remove(selectedItem);
        }

        private DelegateCommand _removeAllTracksCommand;
        public ICommand RemoveAllTracksCommand => _removeAllTracksCommand;
        private void OnRemoveAllTrackCommandHandler(object obj)
        {
            DeleteAllTracks();
        }

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

        #region events
        public event EventHandler<AddTracksToPlaylistEventArgs>? AddLibraryTracksToPlaylistEvent;
        private void InvokeAddLibraryTracksToPlaylistEvent(bool newPlaylist, List<Tags> tags)
        {
            var args = new AddTracksToPlaylistEventArgs(tags, newPlaylist);
            AddLibraryTracksToPlaylistEvent?.Invoke(this, args);
        }

        public event EventHandler<PlayTrackEventArgs>? PlayTrackEvent;
        protected virtual void InvokePlayTrackEvent(PlaylistTags tags)
        {
            foreach (var item in _playlist)
            {
                item.IsPlaying = false;
            }
            tags.IsPlaying = true;
            NotifyPropertyChanged("CurrentIndex");
            PlayTrackEvent?.Invoke(this, new PlayTrackEventArgs(tags.Path, Name));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (propertyName == "Playlist")
            {
                UpdatePlaylistInfo();
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private void UpdatePlaylistInfo()
        {
            NotifyPropertyChanged("NumberOfTracks");
            NotifyPropertyChanged("TotalPlaytime");
        }

        private List<Tags> ConvertSelectedItemsToTagsList(object obj)
        {
            var items = (System.Collections.IList)obj;
            var tagsList = new List<Tags>();
            foreach (PlaylistTags? tags in items)
            {
                if (tags is { }) tagsList.Add(tags);
            }
            return tagsList;
        }

        private ObservableCollection<PlaylistTags> _playlist;

        public int CurrentIndex => Playlist.IndexOf(Playlist.FirstOrDefault(x => x.IsPlaying));
        public int NumberOfTracks => _playlist.Count;
        public int TotalPlaytime => _playlist.Sum(track => track.length);

        internal void DisplayChangedHandler(object? sender, DisplayLibChangedEventArgs e)
        {
            DeleteAllTracks();
            AddTracks(e.Tracks.ToList());
        }

        internal void AddTracksToPlaylistHandler(object? sender, AddTracksToPlaylistEventArgs e)
        {
            if (e.NewPlaylist) { DeleteAllTracks(); }
            AddTracks(e.Tracks);
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

        #region AddTrack(s)
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
        #endregion

        #region DeleteTrack(s)
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
        #endregion

        public void ManualLoadHandler(object? sender, ManualLoadEventArgs e)
        {
            var path = e.Path;
            var tags = new TagReader().Read(path);
            AddTrack(tags);
        }

        public void PlayNextTrackHandler(object? sender, EventArgs e)
        {
            if (HasTracks)
            {
                try
                {
                    InvokePlayTrackEvent(NextTrack);
                }
                catch (EndOfPlaylistException)
                {
                    // Nothing really.
                }
            }
        }

        public void PlayLastTrackHandler(object? sender, EventArgs e)
        {
            if (HasTracks)
            {
                InvokePlayTrackEvent(PreviousTrack);
            }
        }

        public bool HasTracks => _playlist.Count > 0;
        public string Name { get; private set; }

        public void ShuffleRepeatChangedHandler(object? sender, ChangeShuffleRepeatEventArgs e)
        {
            PlaylistShuffleRepeatState = e.ShuffleRepeatState;
        }
    }
}
