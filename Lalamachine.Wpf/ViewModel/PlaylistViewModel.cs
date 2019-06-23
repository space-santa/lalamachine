using LibLala.TagReader;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Input;

namespace Lalamachine.Wpf.ViewModel
{
    public class PlayTrackEventArgs : EventArgs
    {
        public string Path { get; set; }
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
        public PlaylistViewModel()
        {
            _playlist = new ObservableCollection<PlaylistTags>();
            CurrentIndex = -1;
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
            PlaylistTags selectedItem = (PlaylistTags)obj;
            _playlist.Remove(selectedItem);
        }

        private void OnPlayTrackCommandHandler(object obj)
        {
            PlaylistTags track = (PlaylistTags)obj;
            CurrentIndex = _playlist.IndexOf(track);
            OnPlayTrack(track);
        }

        private ObservableCollection<PlaylistTags> _playlist;

        // TODO: Replace the current index with a way to get the currently playing track and a way to look for its index. This current index thing is fragile and breaks when sorting/re-ordering etc
        private int _currentIndex;
        public int CurrentIndex
        {
            get => _currentIndex;
            set
            {
                _currentIndex = value;
                NotifyPropertyChanged();
            }
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
            PlayTrackEvent?.Invoke(this, new PlayTrackEventArgs { Path = tags.path });
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
                    return CurrentTrack;
                }

                if (PlaylistShuffleRepeatState == ShuffleRepeatState.Shuffle)
                {
                    CurrentIndex = new Random().Next(0, _playlist.Count - 1);
                    return CurrentTrack;
                }

                if (CurrentIndex < Playlist.Count - 1)
                {
                    CurrentIndex += 1;
                    return CurrentTrack;
                }

                if (PlaylistShuffleRepeatState == ShuffleRepeatState.RepeatAll)
                {
                    CurrentIndex = 0;
                    return CurrentTrack;
                }

                throw new EndOfPlaylistException();
            }
        }
        public PlaylistTags PreviousTrack
        {
            get
            {
                if (CurrentIndex > 0)
                {
                    CurrentIndex -= 1;
                }
                else if (CurrentIndex < 0)
                {
                    CurrentIndex = 0;
                }
                return CurrentTrack;
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
            for (int i = 0; i < numberOfTracks; ++i)
            {
                DeleteTrack(startIndex);
            }

            if (CurrentIndex >= startIndex)
            {
                if (CurrentIndex < startIndex + numberOfTracks)
                {
                    CurrentIndex = startIndex - 1;
                }
                else
                {
                    CurrentIndex -= numberOfTracks;
                }
            }
            NotifyPropertyChanged("Playlist");
        }

        public void DeleteAllTracks()
        {
            Playlist.Clear();
            CurrentIndex = -1;
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

        public bool HasTracks { get => _playlist.Count > 0; }

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
