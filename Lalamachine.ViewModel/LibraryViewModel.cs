using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using LalaDb.Data;
using LalaDb.Model;
using LibLala;
using LibLala.MusicScanner;

namespace Lalamachine.ViewModel
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

    public sealed class LibraryViewModel : BaseNotifyPropertyChanged
    {
        private readonly MusicLibModel _model;
        private readonly IDialogService _dialogs;

        private string _genreFilter = "";
        private string _artistFilter = "";
        private string _albumFilter = "";
        private bool _scanning;
        private int _filesToScanCount1;
        private int _filesScannedCount1;
        private string _searchString = "";
        private int _numberOfGenres;
        private int _numberOfArtists;
        private int _numberOfAlbums;

        public LibraryViewModel(LalaContext context, Func<LalaContext> createScanContext, IDialogService dialogs)
        {
            _dialogs = dialogs;
            _model = new MusicLibModel(context, createScanContext);
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
                // A filter only changes the lists below it. Its own list stays, so that it keeps its selection, scroll position and focus.
                case nameof(GenreFilter):
                    ClearUnlistedFilters();
                    NotifyPropertyChanged(nameof(DisplayLib));
                    NotifyPropertyChanged(nameof(ArtistList));
                    NotifyPropertyChanged(nameof(AlbumList));
                    break;
                case nameof(ArtistFilter):
                    ClearUnlistedFilters();
                    NotifyPropertyChanged(nameof(DisplayLib));
                    NotifyPropertyChanged(nameof(AlbumList));
                    break;
                case nameof(AlbumFilter):
                    NotifyPropertyChanged(nameof(DisplayLib));
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
            _genreFilter = "";
            _albumFilter = "";
            _artistFilter = "";
            SearchString = "";
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

        public void StartScanHandler(object? sender, StartScanEventArgs e)
        {
            if (e is null)
            {
                return;
            }

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
            ClearUnlistedFilters();
            NotifyPropertyChanged(nameof(DisplayLib));
            NotifyPropertyChanged(nameof(GenreList));
            NotifyPropertyChanged(nameof(ArtistList));
            NotifyPropertyChanged(nameof(AlbumList));
        }

        // A filter that its list doesn't show any more can't be seen or changed by the user, but DisplayLib would keep applying it.
        // In this order, because the artists depend on the genre filter and the albums on both.
        private void ClearUnlistedFilters()
        {
            if (_genreFilter.Length > 0 && !GenreList.Contains(_genreFilter))
            {
                _genreFilter = "";
            }

            if (_artistFilter.Length > 0 && !ArtistList.Contains(_artistFilter))
            {
                _artistFilter = "";
            }

            if (_albumFilter.Length > 0 && !AlbumList.Contains(_albumFilter))
            {
                _albumFilter = "";
            }
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
                // A list that gets replaced selects its selected item again, and that sets the same filter again.
                // Without this check, setting it replaces the lists again, and so on until the stack overflows.
                var filter = EmptyWhenNullOrAll(value);
                if (filter == _genreFilter) { return; }
                _genreFilter = filter;
                NotifyPropertyChanged();
            }
        }

        public string ArtistFilter
        {
            get => _artistFilter;
            set
            {
                var filter = EmptyWhenNullOrAll(value);
                if (filter == _artistFilter) { return; }
                _artistFilter = filter;
                NotifyPropertyChanged();
            }
        }

        public string AlbumFilter
        {
            get => _albumFilter;
            set
            {
                var filter = EmptyWhenNullOrAll(value);
                if (filter == _albumFilter) { return; }
                _albumFilter = filter;
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
            try
            {
                var skippedFiles = await _model.ScanAsync(path).ConfigureAwait(true);
                if (skippedFiles.Count > 0)
                {
                    await _dialogs.ShowMessageAsync(SkippedFilesMessage(skippedFiles)).ConfigureAwait(true);
                }
            }
            finally
            {
                Scanning = false;
                NotifyListsChanged();
            }
        }

        private static string SkippedFilesMessage(IReadOnlyList<string> skippedFiles)
        {
            const int maxListed = 10;
            var listed = string.Join("\n", skippedFiles.Take(maxListed));
            var more = skippedFiles.Count > maxListed ? $"\n... and {skippedFiles.Count - maxListed} more." : "";
            return $"{skippedFiles.Count} file(s) couldn't be read and were left out of the library:\n\n{listed}{more}";
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
                        catch (ArgumentException)
                        {
                            // A broken row. Skip it rather than interrupting with a dialog on every refresh, a rescan rebuilds it.
                        }
                    }
                }

                return displayLib;
            }
        }
    }
}
