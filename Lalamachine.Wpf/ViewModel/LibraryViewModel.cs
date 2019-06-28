using LalaDb.Data;
using LalaDb.Model;
using LibLala;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
            Scanning = false;
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

        private bool _scanning;
        private string _searchString;
        private string _genreFilter;
        private string _artistFilter;
        private string _albumFilter;

        public bool Scanning { get => _scanning; set { _scanning = value; NotifyPropertyChanged(); } }
        public string SearchString
        {
            get => _searchString;
            set
            {
                _searchString = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("DisplayLib");
                NotifyPropertyChanged("GenreList");
                NotifyPropertyChanged("ArtistList");
                NotifyPropertyChanged("AlbumList");
            }
        }

        public string GenreFilter
        {
            get => _genreFilter;
            set
            {
                _genreFilter = EmptyWhenNullOrAll(value);
                NotifyPropertyChanged();
                NotifyPropertyChanged("DisplayLib");
                NotifyPropertyChanged("GenreList");
                NotifyPropertyChanged("ArtistList");
                NotifyPropertyChanged("AlbumList");
            }
        }

        public string ArtistFilter
        {
            get => _artistFilter;
            set
            {
                _artistFilter = EmptyWhenNullOrAll(value);
                NotifyPropertyChanged();
                NotifyPropertyChanged("DisplayLib");
                NotifyPropertyChanged("ArtistList");
                NotifyPropertyChanged("AlbumList");
            }
        }

        public string AlbumFilter
        {
            get => _albumFilter;
            set
            {
                _albumFilter = EmptyWhenNullOrAll(value);
                NotifyPropertyChanged();
                NotifyPropertyChanged("DisplayLib");
                NotifyPropertyChanged("AlbumList");
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
            Scanning = true;
            await _model.scanAsync(path);
            Scanning = false;
        }

        public ObservableCollection<string> GenreList
        {
            get
            {
                var list = _model.genreList(SearchString);
                list = list.Prepend(Constants.ALL).ToArray();
                return new ObservableCollection<string>(list);
            }
        }

        public ObservableCollection<string> ArtistList
        {
            get
            {
                string[] list;
                list = _model.artistList(GenreFilter, SearchString);
                list = list.Prepend(Constants.ALL).ToArray();
                return new ObservableCollection<string>(list);
            }
        }

        public ObservableCollection<string> AlbumList
        {
            get
            {
                string[] list;
                list = _model.albumList(ArtistFilter, GenreFilter, SearchString);
                list = list.Prepend(Constants.ALL).ToArray();
                return new ObservableCollection<string>(list);
            }
        }

        public ObservableCollection<PlaylistTags> DisplayLib
        {
            get
            {
                ObservableCollection<PlaylistTags> displayLib = new ObservableCollection<PlaylistTags>();

                Track[] list = _model.displayLib(AlbumFilter, ArtistFilter, GenreFilter, SearchString);
                foreach (Track track in list)
                {
                    displayLib.Add(new PlaylistTags(new LalaTags(track)));
                }

                return displayLib;
            }
        }
    }
}
