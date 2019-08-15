using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Windows.Input;
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
        public PlaylistViewModel(string name = "MAIN")
        {
            Name = name;
            _playlist = new ObservableCollection<PlaylistTags>();
            _playTrackCommand = new DelegateCommand(OnPlayTrackCommandHandler);
            _removeTrackCommand = new DelegateCommand(OnRemoveTrackCommandHandler);
            _removeAllTracksCommand = new DelegateCommand(OnRemoveAllTrackCommandHandler);
        }

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

        private readonly DelegateCommand _playTrackCommand;
        public ICommand PlayTrackCommand => _playTrackCommand;

        private readonly DelegateCommand _removeTrackCommand;
        public ICommand RemoveTrackCommand => _removeTrackCommand;

        private readonly DelegateCommand _removeAllTracksCommand;
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
