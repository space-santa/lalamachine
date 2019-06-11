using LibLala.TagReader;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Lalamachine.Wpf.ViewModel
{
    public class PlayTrackEventArgs : EventArgs
    {
        public string Path { get; set; }
    }
    class PlaylistViewModel : INotifyPropertyChanged
    {
        public PlaylistViewModel()
        {
            _playlist = new ObservableCollection<Tags>();
        }

        private ObservableCollection<Tags> _playlist;

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

        public event EventHandler<PlayTrackEventArgs> PlayTrackEvent;
        protected virtual void OnPlayTrack(string path)
        {
            PlayTrackEvent?.Invoke(this, new PlayTrackEventArgs { Path = path });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Tags CurrentTrack => Playlist[CurrentIndex];
        public Tags NextTrack
        {
            get
            {
                CurrentIndex += 1;
                return CurrentTrack;
            }
        }
        public Tags PreviousTrack
        {
            get
            {
                CurrentIndex -= 1;
                return CurrentTrack;
            }
        }

        public ObservableCollection<Tags> Playlist
        {
            get => _playlist;
            set
            {
                _playlist = value;
                NotifyPropertyChanged();
            }
        }

        public void AddTrack(Tags tags)
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
            OnPlayTrack(NextTrack.path);
        }

        public void PlayLastTrackHandler(object sender, EventArgs e)
        {
            OnPlayTrack(PreviousTrack.path);
        }
    }
}
