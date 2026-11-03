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

        protected override IEnumerable<PlaylistTags> AllTracks => _allTracks;

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
            return selectedKeys.Count == 0 ? _allTracks : _allTracks.Where(x => selectedKeys.Contains(x.InitialKey));
        }
    }
}
