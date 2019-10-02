using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using LalaDb.Data;
using LalaDb.Model;
using LibLala;

namespace Lalamachine.Wpf.ViewModel
{
    public class DisplayLibChangedEventArgs : EventArgs
    {
        public DisplayLibChangedEventArgs(ObservableCollection<PlaylistTags> tracks)
        {
            Tracks = tracks;
        }
        public ObservableCollection<PlaylistTags> Tracks { get; }
    }

    internal class LibraryViewModel : INotifyPropertyChanged
    {
        private readonly MusicLibModel _model;

        public LibraryViewModel(LalaContext context)
        {
            _model = new MusicLibModel(context);
            Scanning = false;
            _searchString = "";
            _genreFilter = "";
            _artistFilter = "";
            _albumFilter = "";

            _setGenreFilterCommand = new DelegateCommand(OnSetGenreFilter);
            _setArtistFilterCommand = new DelegateCommand(OnSetArtistFilter);
            _setAlbumFilterCommand = new DelegateCommand(OnSetAlbumFilter);
            _startScanCommand = new DelegateCommand(OnStartScan);
            _clearSearchCommand = new DelegateCommand(ClearSearch);
            _updateListsCommand = new DelegateCommand(OnUpdateLists);
            _createNewPlaylistFromAlbumCommand = new DelegateCommand(OnCreateNewPlaylistFromAlbumCommand);
            _addTracksFromAlbumCommand = new DelegateCommand(OnAddTracksFromAlbumCommand);

            NotifyDisplayLibChanged();
        }

        #region Commands
        private readonly DelegateCommand _setGenreFilterCommand;
        public ICommand SetGenreFilterCommand => _setGenreFilterCommand;
        private void OnSetGenreFilter(object obj) { GenreFilter = (string)obj; }

        private readonly DelegateCommand _setArtistFilterCommand;
        public ICommand SetArtistFilterCommand => _setArtistFilterCommand;
        private void OnSetArtistFilter(object obj) { ArtistFilter = (string)obj; }

        private readonly DelegateCommand _setAlbumFilterCommand;
        public ICommand SetAlbumFilterCommand => _setAlbumFilterCommand;
        private void OnSetAlbumFilter(object obj) { AlbumFilter = (string)obj; }

        private readonly DelegateCommand _clearSearchCommand;
        public ICommand ClearSearchCommand => _clearSearchCommand;
        private void ClearSearch(object obj)
        {
            SearchString = "";
            _genreFilter = "";
            _albumFilter = "";
            _artistFilter = "";
            NotifyListsChanged();
        }

        private readonly DelegateCommand _startScanCommand;
        public ICommand StartScanCommand => _startScanCommand;
        private void OnStartScan(object obj) { ScanAsync((string)obj); }

        private readonly DelegateCommand _updateListsCommand;
        public ICommand UpdateListsCommand => _updateListsCommand;
        private void OnUpdateLists(object obj) { NotifyListsChanged(); }

        private readonly DelegateCommand _createNewPlaylistFromAlbumCommand;
        public ICommand CreateNewPlaylistFromAlbumCommand => _createNewPlaylistFromAlbumCommand;
        private void OnCreateNewPlaylistFromAlbumCommand(object obj)
        {
            InvokeAddTrackToPlaylistEvent((string)obj, true);
        }

        private readonly DelegateCommand _addTracksFromAlbumCommand;
        public ICommand AddTracksFromAlbumCommand => _addTracksFromAlbumCommand;
        private void OnAddTracksFromAlbumCommand(object obj)
        {
            InvokeAddTrackToPlaylistEvent((string)obj, false);
        }
        #endregion

        #region Events
        public event EventHandler<AddTracksToPlaylistEventArgs>? AddTracksToPlaylistEvent;
        private void InvokeAddTrackToPlaylistEvent(string name, bool newPlaylist)
        {
            var args = new AddTracksToPlaylistEventArgs(_model.GetAlbumTracks(name), newPlaylist);
            AddTracksToPlaylistEvent?.Invoke(this, args);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event EventHandler<DisplayLibChangedEventArgs>? DisplayLibChanged;
        private void NotifyDisplayLibChanged()
        {
            DisplayLibChanged?.Invoke(this, new DisplayLibChangedEventArgs(DisplayLib));
        }
        #endregion

        internal void StartScanHandler(object? sender, StartScanEventArgs e)
        {
            ScanAsync(e.Path);
        }

        private bool _scanning;
        private string _searchString;
        private string _genreFilter;
        private string _artistFilter;
        private string _albumFilter;
        private int _numberOfGenres;
        private int _numberOfArtists;
        private int _numberOfAlbums;

        public bool Scanning
        {
            get => _scanning;
            set
            {
                _scanning = value;
                NotifyPropertyChanged();
                NotifyListsChanged();
                NotifyPropertyChanged(nameof(ScanVisible));
            }
        }

        public string ScanVisible
        {
            get
            {
                if (Scanning)
                {
                    return "Visible";
                }
                else
                {
                    return "Collapsed";
                }
            }
        }

        private void NotifyListsChanged()
        {
            NotifyPropertyChanged(nameof(DisplayLib));
            NotifyPropertyChanged(nameof(GenreList));
            NotifyPropertyChanged(nameof(ArtistList));
            NotifyPropertyChanged(nameof(AlbumList));
            NotifyDisplayLibChanged();
        }

        public string SearchString
        {
            get => _searchString;
            set
            {
                _searchString = value;
                NotifyPropertyChanged();
                NotifyListsChanged();
                NotifyDisplayLibChanged();
            }
        }

        public string GenreFilter
        {
            get => _genreFilter;
            set
            {
                _genreFilter = EmptyWhenNullOrAll(value);
                NotifyPropertyChanged();
                NotifyListsChanged();
                NotifyDisplayLibChanged();
            }
        }

        public string ArtistFilter
        {
            get => _artistFilter;
            set
            {
                _artistFilter = EmptyWhenNullOrAll(value);
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(DisplayLib));
                NotifyPropertyChanged(nameof(ArtistList));
                NotifyPropertyChanged(nameof(AlbumList));
                NotifyDisplayLibChanged();
            }
        }

        public string AlbumFilter
        {
            get => _albumFilter;
            set
            {
                _albumFilter = EmptyWhenNullOrAll(value);
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(DisplayLib));
                NotifyPropertyChanged(nameof(AlbumList));
                NotifyDisplayLibChanged();
            }
        }

        private static string EmptyWhenNullOrAll(string value)
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
            await _model.ScanAsync(path).ConfigureAwait(true);
            Scanning = false;
        }

        public ObservableCollection<string?> GenreList
        {
            get
            {
                var list = _model.GenreList(SearchString);
                NumberOfGenres = list.Length;
                list = list.Prepend(Constants.ALL).ToArray();
                return new ObservableCollection<string?>(list);
            }
        }

        public int NumberOfGenres
        {
            get => _numberOfGenres;
            set
            {
                _numberOfGenres = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(GenreHeader));
            }
        }
        public string GenreHeader => $" Genre ({NumberOfGenres}) ";

        public ObservableCollection<string?> ArtistList
        {
            get
            {
                var list = _model.ArtistList(GenreFilter, SearchString);
                NumberOfArtists = list.Length;
                list = list.Prepend(Constants.ALL).ToArray();
                return new ObservableCollection<string?>(list);
            }
        }

        public int NumberOfArtists
        {
            get => _numberOfArtists;
            set
            {
                _numberOfArtists = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(ArtistHeader));
            }
        }
        public string ArtistHeader => $" Artist ({NumberOfArtists}) ";

        public ObservableCollection<string?> AlbumList
        {
            get
            {
                var list = _model.AlbumList(ArtistFilter, GenreFilter, SearchString);
                NumberOfAlbums = list.Length;
                list = list.Prepend(Constants.ALL).ToArray();
                return new ObservableCollection<string?>(list);
            }
        }

        public int NumberOfAlbums
        {
            get => _numberOfAlbums;
            set
            {
                _numberOfAlbums = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(AlbumHeader));
            }
        }
        public string AlbumHeader => $" Album ({NumberOfAlbums}) ";

        public ObservableCollection<PlaylistTags> DisplayLib
        {
            get
            {
                var displayLib = new ObservableCollection<PlaylistTags>();

                var list = _model.DisplayLib(AlbumFilter, ArtistFilter, GenreFilter, SearchString);
                foreach (var track in list)
                {
                    if (track is { })
                    {
                        displayLib.Add(new PlaylistTags(LalaTags.Build(track)));
                    }
                }

                return displayLib;
            }
        }
    }
}
