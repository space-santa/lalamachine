using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using LalaDb.Data;
using LalaDb.Model;
using LibLala;
using LibLala.MusicScanner;

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

    public class ScanningChangedEventArgs : EventArgs
    {
        public ScanningChangedEventArgs(bool scanning)
        {
            Scanning = scanning;
        }
        public bool Scanning { get; }
    }

    internal class LibraryViewModel : BaseNotifyPropertyChanged
    {
        private readonly MusicLibModel _model;

        private string _genreFilter = "";
        private string _artistFilter = "";
        private string _albumFilter = "";
        public int _filesToScanCount = 0;
        public int _filesScannedCount = 0;
        private bool _scanning = false;
        private int _filesToScanCount1;
        private int _filesScannedCount1;
        private string _searchString = "";
        private int _numberOfGenres;
        private int _numberOfArtists;
        private int _numberOfAlbums;

        public LibraryViewModel(LalaContext context)
        {
            _model = new MusicLibModel(context);
            _model.FileScannedEvent += FileScannedEventHandler;
            _model.FilesToScanChangedEvent += FilesToScanChangedEventHandler;

            _setGenreFilterCommand = new DelegateCommand(OnSetGenreFilter);
            _setArtistFilterCommand = new DelegateCommand(OnSetArtistFilter);
            _setAlbumFilterCommand = new DelegateCommand(OnSetAlbumFilter);
            _startScanCommand = new DelegateCommand(OnStartScan);
            _clearSearchCommand = new DelegateCommand(ClearSearch);
            _updateListsCommand = new DelegateCommand(OnUpdateLists);
            _createNewPlaylistFromAlbumCommand = new DelegateCommand(OnCreateNewPlaylistFromAlbumCommand);
            _addTracksFromAlbumCommand = new DelegateCommand(OnAddTracksFromAlbumCommand);

            PropertyChanged += HandlePropertyChanged;

            NotifyDisplayLibChanged();
        }

        private void HandlePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(DisplayLib):
                    NotifyDisplayLibChanged();
                    break;
                case nameof(Scanning):
                    NotifyScanningChanged();
                    break;
                case nameof(SearchString):
                    NotifyListsChanged();
                    break;
                case nameof(GenreFilter):
                    NotifyListsChanged();
                    break;
                case nameof(ArtistFilter):
                    NotifyPropertyChanged(nameof(DisplayLib));
                    NotifyPropertyChanged(nameof(ArtistList));
                    NotifyPropertyChanged(nameof(AlbumList));
                    break;
                case nameof(AlbumFilter):
                    NotifyPropertyChanged(nameof(DisplayLib));
                    NotifyPropertyChanged(nameof(AlbumList));
                    break;
                case nameof(NumberOfGenres):
                    NotifyPropertyChanged(nameof(GenreHeader));
                    break;
                case nameof(NumberOfArtists):
                    NotifyPropertyChanged(nameof(ArtistHeader));
                    break;
                case nameof(NumberOfAlbums):
                    NotifyPropertyChanged(nameof(AlbumHeader));
                    break;
            }
        }

        #region Commands
        private readonly DelegateCommand _setGenreFilterCommand;
        public ICommand SetGenreFilterCommand => _setGenreFilterCommand;
        private void OnSetGenreFilter(object? obj)
        {
            if (obj is null)
            {
                GenreFilter = "";
                return;
            }
            GenreFilter = (string)obj;
        }

        private readonly DelegateCommand _setArtistFilterCommand;
        public ICommand SetArtistFilterCommand => _setArtistFilterCommand;
        private void OnSetArtistFilter(object? obj)
        {
            if (obj is null)
            {
                ArtistFilter = "";
                return;
            }
            ArtistFilter = (string)obj;
        }

        private readonly DelegateCommand _setAlbumFilterCommand;
        public ICommand SetAlbumFilterCommand => _setAlbumFilterCommand;
        private void OnSetAlbumFilter(object? obj)
        {
            if (obj is null)
            {
                AlbumFilter = "";
                return;
            }
            AlbumFilter = (string)obj;
        }

        private readonly DelegateCommand _clearSearchCommand;
        public ICommand ClearSearchCommand => _clearSearchCommand;
        private void ClearSearch(object? obj)
        {
            SearchString = "";
            _genreFilter = "";
            _albumFilter = "";
            _artistFilter = "";
            NotifyListsChanged();
        }

        private readonly DelegateCommand _startScanCommand;
        public ICommand StartScanCommand => _startScanCommand;
        private void OnStartScan(object? obj)
        {
            if (obj is null) { return; }
            ScanAsync((string)obj);
        }

        private readonly DelegateCommand _updateListsCommand;
        public ICommand UpdateListsCommand => _updateListsCommand;
        private void OnUpdateLists(object? obj) { NotifyListsChanged(); }

        private readonly DelegateCommand _createNewPlaylistFromAlbumCommand;
        public ICommand CreateNewPlaylistFromAlbumCommand => _createNewPlaylistFromAlbumCommand;
        private void OnCreateNewPlaylistFromAlbumCommand(object? obj)
        {
            if (obj is null) { return; }
            InvokeAddTrackToPlaylistEvent((string)obj, true);
        }

        private readonly DelegateCommand _addTracksFromAlbumCommand;
        public ICommand AddTracksFromAlbumCommand => _addTracksFromAlbumCommand;
        private void OnAddTracksFromAlbumCommand(object? obj)
        {
            if (obj is null) { return; }
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

        public event EventHandler<DisplayLibChangedEventArgs>? DisplayLibChanged;
        private void NotifyDisplayLibChanged()
        {
            DisplayLibChanged?.Invoke(this, new DisplayLibChangedEventArgs(DisplayLib));
        }

        public event EventHandler<ScanningChangedEventArgs>? ScanningChanged;
        private void NotifyScanningChanged()
        {
            ScanningChanged?.Invoke(this, new ScanningChangedEventArgs(Scanning));
        }
        #endregion

        internal void StartScanHandler(object? sender, StartScanEventArgs e)
        {
            ScanAsync(e.Path);
        }

        internal void FilesToScanChangedEventHandler(object? sender, FileScannedEventArgs args)
        {
            FilesToScanCount = args.Count;
        }

        internal void FileScannedEventHandler(object? sender, FileScannedEventArgs args)
        {
            FilesScannedCount = args.Count;
        }

        private void NotifyListsChanged()
        {
            NotifyPropertyChanged(nameof(DisplayLib));
            NotifyPropertyChanged(nameof(GenreList));
            NotifyPropertyChanged(nameof(ArtistList));
            NotifyPropertyChanged(nameof(AlbumList));
        }

        public bool Scanning
        {
            get => _scanning;
            set
            {
                _scanning = value;
                NotifyPropertyChanged();
            }
        }
        public int FilesToScanCount
        {
            get => _filesToScanCount1;
            set
            {
                _filesToScanCount1 = value;
                NotifyPropertyChanged();
            }
        }

        public int FilesScannedCount
        {
            get => _filesScannedCount1;
            set
            {
                _filesScannedCount1 = value;
                NotifyPropertyChanged();
            }
        }

        public string SearchString
        {
            get => _searchString;
            set
            {
                _searchString = value;
                NotifyPropertyChanged();
            }
        }

        public string GenreFilter
        {
            get => _genreFilter;
            set
            {
                _genreFilter = EmptyWhenNullOrAll(value);
                NotifyPropertyChanged();
            }
        }

        public string ArtistFilter
        {
            get => _artistFilter;
            set
            {
                _artistFilter = EmptyWhenNullOrAll(value);
                NotifyPropertyChanged();
            }
        }

        public string AlbumFilter
        {
            get => _albumFilter;
            set
            {
                _albumFilter = EmptyWhenNullOrAll(value);
                NotifyPropertyChanged();
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
            }
        }
        public string GenreHeader => $" Genre ({NumberOfGenres}) ";

        public int NumberOfArtists
        {
            get => _numberOfArtists;
            set
            {
                _numberOfArtists = value;
                NotifyPropertyChanged();
            }
        }
        public string ArtistHeader => $" Artist ({NumberOfArtists}) ";

        public int NumberOfAlbums
        {
            get => _numberOfAlbums;
            set
            {
                _numberOfAlbums = value;
                NotifyPropertyChanged();
            }
        }
        public string AlbumHeader => $" Album ({NumberOfAlbums}) ";

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

        public ObservableCollection<string?> AlbumList
        {
            get
            {
                var list = _model.AlbumList(ArtistFilter, GenreFilter, SearchString);

                if (list is null)
                {
                    list = Array.Empty<string>();
                }

                NumberOfAlbums = list.Length;
                list = list.Prepend(Constants.ALL).ToArray();
                return new ObservableCollection<string?>(list);
            }
        }

        public ObservableCollection<PlaylistTags> DisplayLib
        {
            get
            {
                var displayLib = new ObservableCollection<PlaylistTags>();

                var list = _model.DisplayLib(AlbumFilter, ArtistFilter, GenreFilter, SearchString);

                if (list is null) { return displayLib; }

                foreach (var track in list)
                {
                    if (track is { })
                    {
                        try
                        {
                            displayLib.Add(new PlaylistTags(LalaTags.Build(track)));
                        }
                        catch (ArgumentException e)
                        {
                            System.Windows.MessageBox.Show($"{e.Message}.\nYou may need to rescan the library.");
                        }
                    }
                }

                return displayLib;
            }
        }
    }
}
