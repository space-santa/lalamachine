using LibLala.TagReader;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Lalamachine.Wpf.ViewModel
{
    class PlaylistViewModel : INotifyPropertyChanged
    {
        private List<Tags> _playlist;

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

        public List<Tags> Playlist
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

        public void AddTracks(List<Tags> tags)
        {
            Playlist.AddRange(tags);
            NotifyPropertyChanged("Playlist");
        }

        public void DeleteTrack(int index)
        {
            Playlist.RemoveAt(index);
            NotifyPropertyChanged("Playlist");
        }

        public void DeleteTracks(int startIndex, int numberOfTracks)
        {
            Playlist.RemoveRange(startIndex, numberOfTracks);
            NotifyPropertyChanged("Playlist");
        }

        public void DeleteAllTracks()
        {
            Playlist.Clear();
            NotifyPropertyChanged("Playlist");
        }
    }
}
