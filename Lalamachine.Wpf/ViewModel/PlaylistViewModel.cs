﻿using LibLala.TagReader;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;

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
            _playTrackCommand = new DelegateCommand(OnPlayTrackCommandHandler, (object commandParameter) => { return true; } );
            _removeTrackCommand = new DelegateCommand(OnRemoveTrackCommandHandler, (object commandParameter) => { return true; } );
            _removeAllTracksCommand = new DelegateCommand(OnRemoveAllTrackCommandHandler, (object commandParameter) => { return true; } );
        }

        private void OnRemoveAllTrackCommandHandler(object obj)
        {
            DeleteAllTracks();
        }

        private void OnRemoveTrackCommandHandler(object obj)
        {
            Tags selectedItem = (Tags)obj;
            _playlist.Remove(selectedItem);
        }

        private void OnPlayTrackCommandHandler(object obj)
        {
            Tags track = (Tags)obj;
            OnPlayTrack(track.path);
        }

        private ObservableCollection<Tags> _playlist;

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

        public ShuffleRepeatState ShuffleRepeatState { get; set; }

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

        public void ShuffleRepeatChangedHandler(object sender, ChangeShuffleRepeatEventArgs e)
        {
            ShuffleRepeatState = e.ShuffleRepeatState;
        }
    }
}