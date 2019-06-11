﻿using LibLala.TagReader;
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
            CurrentIndex = -1;
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
                if (CurrentIndex < Playlist.Count - 1)
                {
                    CurrentIndex += 1;
                }
                return CurrentTrack; // TODO: This must be conditional. E.g. if repeat all, return first track.
            }
        }
        public Tags PreviousTrack
        {
            get
            {
                if (CurrentIndex > 0)
                {
                    CurrentIndex -= 1;
                }
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
            OnPlayTrack(NextTrack.path);
        }

        public void PlayLastTrackHandler(object sender, EventArgs e)
        {
            OnPlayTrack(PreviousTrack.path);
        }
    }
}
