using System;
using System.Collections;
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

        private IReadOnlyList<string> _genreFilter = Array.Empty<string>();
        private IReadOnlyList<string> _artistFilter = Array.Empty<string>();
        private IReadOnlyList<string> _albumFilter = Array.Empty<string>();
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
            GenreFilter = SelectedNames(obj);
        }

        private readonly DelegateCommand _setArtistFilterCommand;
        public ICommand SetArtistFilterCommand => _setArtistFilterCommand;
        private void OnSetArtistFilter(object? obj)
        {
            ArtistFilter = SelectedNames(obj);
        }

        private readonly DelegateCommand _setAlbumFilterCommand;
        public ICommand SetAlbumFilterCommand => _setAlbumFilterCommand;
        private void OnSetAlbumFilter(object? obj)
        {
            AlbumFilter = SelectedNames(obj);
        }

        private readonly DelegateCommand _clearSearchCommand;
        public ICommand ClearSearchCommand => _clearSearchCommand;
        private void ClearSearch(object? obj)
        {
            _genreFilter = Array.Empty<string>();
            _albumFilter = Array.Empty<string>();
            _artistFilter = Array.Empty<string>();
            SearchString = "";
            FiltersCleared?.Invoke(this, EventArgs.Empty);
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
            InvokeAddTrackToPlaylistEvent(SelectedNames(obj), true);
        }

        private readonly DelegateCommand _addTracksFromAlbumCommand;
        public ICommand AddTracksFromAlbumCommand => _addTracksFromAlbumCommand;
        private void OnAddTracksFromAlbumCommand(object? obj)
        {
            if (obj is null) { return; }
            InvokeAddTrackToPlaylistEvent(SelectedNames(obj), false);
        }
        #endregion

        #region Events
        public event EventHandler<AddTracksToPlaylistEventArgs>? AddTracksToPlaylistEvent;
        private void InvokeAddTrackToPlaylistEvent(IEnumerable<string> albums, bool newPlaylist)
        {
            var args = new AddTracksToPlaylistEventArgs(albums.SelectMany(album => _model.GetAlbumTracks(album)).ToList(), newPlaylist);
            AddTracksToPlaylistEvent?.Invoke(this, args);
        }

        public event EventHandler<DisplayLibChangedEventArgs>? DisplayLibChanged;
        private void NotifyDisplayLibChanged()
        {
            DisplayLibChanged?.Invoke(this, new DisplayLibChangedEventArgs(DisplayLib));
        }

        // The filters of this view model are cleared by then. For the ones that live elsewhere, like the key filter.
        public event EventHandler? FiltersCleared;

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
            _genreFilter = OnlyListed(_genreFilter, () => GenreList);
            _artistFilter = OnlyListed(_artistFilter, () => ArtistList);
            _albumFilter = OnlyListed(_albumFilter, () => AlbumList);
        }

        private static IReadOnlyList<string> OnlyListed(IReadOnlyList<string> filter, Func<ObservableCollection<string?>> getList)
        {
            if (filter.Count == 0)
            {
                return filter;
            }

            var list = getList();
            return filter.Where(name => list.Contains(name)).ToArray();
        }

        // What shift-clicking an item does to what its list has selected: it adds the item, or takes it out again.
        // The list itself would select the whole range up to the item.
        // -- all -- is never selected along with anything else, and it's what is left when nothing else is selected.
        public static void ToggleFilterItem(IList selection, string item)
        {
            ArgumentNullException.ThrowIfNull(selection);

            if (item == Constants.ALL)
            {
                selection.Clear();
                selection.Add(item);
                return;
            }

            selection.Remove(Constants.ALL);
            if (selection.Contains(item))
            {
                selection.Remove(item);
            }
            else
            {
                selection.Add(item);
            }

            if (selection.Count == 0)
            {
                selection.Add(Constants.ALL);
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

        public IReadOnlyList<string> GenreFilter
        {
            get => _genreFilter;
            private set
            {
                // A list that gets replaced selects its selected items again, and that sets the same filter again.
                // Without this check, setting it replaces the lists again, and so on until the stack overflows.
                if (_genreFilter.ToHashSet().SetEquals(value)) { return; }
                _genreFilter = value;
                NotifyPropertyChanged();
            }
        }

        public IReadOnlyList<string> ArtistFilter
        {
            get => _artistFilter;
            private set
            {
                if (_artistFilter.ToHashSet().SetEquals(value)) { return; }
                _artistFilter = value;
                NotifyPropertyChanged();
            }
        }

        public IReadOnlyList<string> AlbumFilter
        {
            get => _albumFilter;
            private set
            {
                if (_albumFilter.ToHashSet().SetEquals(value)) { return; }
                _albumFilter = value;
                NotifyPropertyChanged();
            }
        }

        // The names in what a list has selected. -- all -- means no filter, whatever else is selected with it.
        private static string[] SelectedNames(object? selection)
        {
            var names = selection switch
            {
                string name => new[] { name },
                IEnumerable items => items.OfType<string>().ToArray(),
                _ => Array.Empty<string>(),
            };

            return names.Contains(Constants.ALL) ? Array.Empty<string>() : names;
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
