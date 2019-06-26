using LalaDb.Model;
using LibLala;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

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
        }

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
