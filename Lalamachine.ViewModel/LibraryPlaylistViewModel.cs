using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using LibLala.DomainPrimitives;

namespace Lalamachine.ViewModel
{
    public sealed class KeyFilterOption : BaseNotifyPropertyChanged
    {
        private readonly Action _selectionChanged;
        private bool _isSelected;

        public KeyFilterOption(string key, bool isSelected, Action selectionChanged)
        {
            Key = key;
            _isSelected = isSelected;
            _selectionChanged = selectionChanged;
        }

        // As the tracks have it, empty for the ones without a key.
        public string Key { get; }
        public string Label => Key.Length > 0 ? Key : "(no key)";

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (value == _isSelected) { return; }
                _isSelected = value;
                NotifyPropertyChanged();
                _selectionChanged();
            }
        }
    }

    public class LibraryPlaylistViewModel : PlaylistBaseViewModel
    {
        // All the tracks of the library's selection, in the order on screen. Playlist has the ones that the key filter lets through,
        // so that next, previous and adding the selection to the queue all go by what is on screen.
        private List<PlaylistTags> _allTracks = new List<PlaylistTags>();
        private ObservableCollection<KeyFilterOption> _keyFilterOptions = new ObservableCollection<KeyFilterOption>();
        private string? _playingPath;

        public LibraryPlaylistViewModel(IDialogService dialogs, ITrackPlaylists trackPlaylists) : base("LIBRARY", dialogs, trackPlaylists)
        {
            _clearKeyFilterCommand = new DelegateCommand(OnClearKeyFilter);
        }

        private readonly DelegateCommand _clearKeyFilterCommand;
        public ICommand ClearKeyFilterCommand => _clearKeyFilterCommand;
        private void OnClearKeyFilter(object? obj)
        {
            UpdateKeyFilterOptions(new HashSet<string>());
            ShowFilteredTracks();
        }

        public ObservableCollection<KeyFilterOption> KeyFilterOptions
        {
            get => _keyFilterOptions;
            private set
            {
                _keyFilterOptions = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsKeyFilterActive => KeyFilterOptions.Any(x => x.IsSelected);

        public void DisplayChangedHandler(object? sender, DisplayLibChangedEventArgs e)
        {
            if (e is null)
            {
                return;
            }

            _allTracks = e.Tracks.ToList();
            // These are new objects, also for the track that is playing. Next and previous go by which one is marked.
            foreach (var track in _allTracks.Where(x => x.Path == _playingPath))
            {
                track.IsPlaying = true;
            }
            MarkLiked(_allTracks);
            UpdateKeyFilterOptions(SelectedKeys());
            ShowFilteredTracks();
        }

        public void ClearKeyFilterHandler(object? sender, EventArgs e)
        {
            // Most of the time there is no key filter, and no reason to rebuild the list.
            if (IsKeyFilterActive)
            {
                ClearKeyFilterCommand.Execute(null);
            }
        }

        public override void TrackAnalyzedHandler(object? sender, TrackAnalyzedEventArgs e)
        {
            if (e is null)
            {
                return;
            }

            var selectedKeys = SelectedKeys();
            var tracks = _allTracks.Where(x => x.Path == e.Path).ToList();
            var wereLetThrough = tracks.Select(x => IsLetThrough(x, selectedKeys)).ToList();
            base.TrackAnalyzedHandler(sender, e);
            if (e.InitialKey is null || tracks.Count == 0)
            {
                return;
            }

            // The keys to filter by are the ones the tracks have now: one that no track had is new, and "(no key)" goes when
            // the last track without one got one. The selected ones stay.
            var keys = _allTracks.Select(x => x.InitialKey).Concat(selectedKeys).ToHashSet();
            if (!keys.SetEquals(KeyFilterOptions.Select(x => x.Key)))
            {
                UpdateKeyFilterOptions(selectedKeys);
            }

            // A track that the filter hid can have a selected key now, or one that it let through not any more. Only then is
            // the list replaced, that loses what is selected in it.
            if (!tracks.Select(x => IsLetThrough(x, selectedKeys)).SequenceEqual(wereLetThrough))
            {
                ShowFilteredTracks();
            }
        }

        protected override IEnumerable<PlaylistTags> AllTracks => _allTracks;

        // A scan makes new objects, the library has a track once.
        protected override PlaylistTags? Listed(PlaylistTags track) => base.Listed(track) ?? _allTracks.FirstOrDefault(x => x.Path == track?.Path);

        protected override void Sort(SortMessage sortMessage)
        {
            // All of them, so that the ones the filter hides are in the same order when it lets them through again.
            ReplacePlaylist(_allTracks);
            base.Sort(sortMessage);
            _allTracks = Playlist.ToList();
            ReplacePlaylist(FilteredTracks());
        }

        protected override void InvokePlayTrackEvent(PlaylistTags tags)
        {
            if (tags is null)
            {
                return;
            }

            // The track that was playing can be one that the filter hides.
            foreach (var track in _allTracks)
            {
                track.IsPlaying = false;
            }

            _playingPath = tags.Path;
            base.InvokePlayTrackEvent(tags);
        }

        private HashSet<string> SelectedKeys()
        {
            return KeyFilterOptions.Where(x => x.IsSelected).Select(x => x.Key).ToHashSet();
        }

        // The selected keys stay listed when no track has them any more. A filter can then let nothing through, but it can be seen and changed.
        private void UpdateKeyFilterOptions(HashSet<string> selectedKeys)
        {
            KeyFilterOptions = new ObservableCollection<KeyFilterOption>(
                _allTracks
                    .Select(x => x.InitialKey)
                    .Concat(selectedKeys)
                    .Distinct()
                    .Select(x => new InitialKey(x))
                    .OrderBy(x => x.ToString().Length == 0)
                    .ThenBy(x => x.Number)
                    .ThenBy(x => x.Letter)
                    .Select(x => new KeyFilterOption(x.ToString(), selectedKeys.Contains(x.ToString()), ShowFilteredTracks))
            );
        }

        private void ShowFilteredTracks()
        {
            ReplacePlaylist(FilteredTracks());
            NotifyPropertyChanged(nameof(Playlist));
            NotifyPropertyChanged(nameof(IsKeyFilterActive));
        }

        private IEnumerable<PlaylistTags> FilteredTracks()
        {
            var selectedKeys = SelectedKeys();
            return _allTracks.Where(x => IsLetThrough(x, selectedKeys));
        }

        // Without a key selected, the filter lets all tracks through.
        private static bool IsLetThrough(PlaylistTags track, HashSet<string> selectedKeys)
        {
            return selectedKeys.Count == 0 || selectedKeys.Contains(track.InitialKey);
        }
    }
}
