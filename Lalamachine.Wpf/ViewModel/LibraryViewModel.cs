﻿using LalaDb.Model;
using LibLala;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Lalamachine.Wpf.ViewModel
{
    class LibraryViewModel : INotifyPropertyChanged
    {
        private MusicLibModel _model;

        public LibraryViewModel()
        {
            _model = new MusicLibModel();
            SearchString = "";
            GenreFilter = "";
            ArtistFilter = "";
            AlbumFilter = "";

            _setGenreFilterCommand = new DelegateCommand(OnSetGenreFilter);
            _setArtistFilterCommand = new DelegateCommand(OnSetArtistFilter);
            _setAlbumFilterCommand = new DelegateCommand(OnSetAlbumFilter);
            _startScanCommand = new DelegateCommand(OnStartScanFilter);
        }

        private readonly DelegateCommand _setGenreFilterCommand;
        public ICommand SetGenreFilterCommand => _setGenreFilterCommand;
        private void OnSetGenreFilter(object obj) { GenreFilter = (string)obj; }

        private readonly DelegateCommand _setArtistFilterCommand;
        public ICommand SetArtistFilterCommand => _setArtistFilterCommand;
        private void OnSetArtistFilter(object obj) { ArtistFilter = (string)obj; }

        private readonly DelegateCommand _setAlbumFilterCommand;
        public ICommand SetAlbumFilterCommand => _setAlbumFilterCommand;
        private void OnSetAlbumFilter(object obj) { AlbumFilter = (string)obj; }

        private readonly DelegateCommand _startScanCommand;
        public ICommand StartScanCommand => _startScanCommand;
        private void OnStartScanFilter(object obj) { ScanAsync((string)obj); }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool Scanning { get => _model.Scanning; }

        public string SearchString { get; set; }

        private string _genreFilter;
        private string _artistFilter;
        private string _albumFilter;

        public string GenreFilter
        {
            get => _genreFilter;
            set
            {
                _genreFilter = EmptyWhenNullOrAll(value);
            }
        }

        public string ArtistFilter
        {
            get => _artistFilter;
            set
            {
                _artistFilter = EmptyWhenNullOrAll(value);
            }
        }

        public string AlbumFilter
        {
            get => _albumFilter;
            set
            {
                _albumFilter = EmptyWhenNullOrAll(value);
            }
        }

        private string EmptyWhenNullOrAll(string value)
        {
            if (value == null || value == Constants.ALL)
            {
                return "";
            }

            return value;
        }
        public async void ScanAsync(string path)
        {
            if (Scanning) { return; }
            await _model.scanAsync(path);
            NotifyPropertyChanged("Scanning");
        }
    }
}