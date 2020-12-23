using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using LalaDb.Data;
using LibLala.DomainPrimitives;
using LibLala.LibLalaTagReader;

namespace Lalamachine.Wpf.ViewModel
{
    internal class SortMessage
    {
        public SortMessage(ListSortDirection direction, PlaylistColumn column)
        {
            Direction = direction;
            Column = column;
        }

        public ListSortDirection Direction { get; }
        public PlaylistColumn Column { get; }
    }

    public class PlaylistBaseViewModel : BaseNotifyPropertyChanged
    {
        public PlaylistBaseViewModel(string name)
        {
            Name = name;
            Playlist = new ObservableCollection<PlaylistTags>();
            _playTrackCommand = new DelegateCommand(OnPlayTrackCommandHandler);
            _removeTrackCommand = new DelegateCommand(OnRemoveTrackCommandHandler);
            _removeAllTracksCommand = new DelegateCommand(OnRemoveAllTrackCommandHandler);
            _createNewPlaylistFromSelectionCommand = new DelegateCommand(OnCreateNewPlaylistFromSelectionCommand);
            _addSelectionToPlaylistCommand = new DelegateCommand(OnAddSelectionToPlaylistCommand);
            _sortCommand = new DelegateCommand(OnSortCommandHandler);

            PropertyChanged += UpdatePlaylistInfoHandler;
        }

        private void UpdatePlaylistInfoHandler(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Playlist))
            {
                UpdatePlaylistInfo();
            }
        }

        #region Commands
        private readonly DelegateCommand _sortCommand;
        public ICommand SortCommand => _sortCommand;
        private void OnSortCommandHandler(object obj)
        {
            var sortMessage = obj as SortMessage;
            if (sortMessage is null)
            {
                return;
            }

            var ascending = sortMessage.Direction == ListSortDirection.Ascending;

            switch (sortMessage.Column.ColumnType)
            {
                case PlaylistColumnType.Album:
                    if (ascending)
                    {
                        Playlist = new ObservableCollection<PlaylistTags>(
                            Playlist
                                .OrderBy(x => x.Album)
                                .ThenBy(x => x.Track)
                                .ThenBy(x => x.DiscNumber)
                        );
                    }
                    else
                    {
                        Playlist = new ObservableCollection<PlaylistTags>(
                            Playlist
                                .OrderByDescending(x => x.Album)
                                .ThenBy(x => x.Track)
                                .ThenBy(x => x.DiscNumber)
                        );
                    }
                    break;
                case PlaylistColumnType.Artist:
                    if (ascending)
                    {
                        Playlist = new ObservableCollection<PlaylistTags>(
                            Playlist
                                .OrderBy(x => x.ArtistString)
                                .ThenBy(x => x.Album)
                                .ThenBy(x => x.Track)
                                .ThenBy(x => x.DiscNumber)
                        );
                    }
                    else
                    {
                        Playlist = new ObservableCollection<PlaylistTags>(
                            Playlist
                                .OrderByDescending(x => x.ArtistString)
                                .ThenBy(x => x.Album)
                                .ThenBy(x => x.Track)
                                .ThenBy(x => x.DiscNumber)
                        );
                    }
                    break;
                case PlaylistColumnType.Comment:
                    if (ascending)
                    {
                        Playlist = new ObservableCollection<PlaylistTags>(Playlist.OrderBy(x => x.Comment));
                    }
                    else
                    {
                        Playlist = new ObservableCollection<PlaylistTags>(Playlist.OrderByDescending(x => x.Comment));
                    }
                    break;
                case PlaylistColumnType.Disc:
                    if (ascending)
                    {
                        Playlist = new ObservableCollection<PlaylistTags>(Playlist.OrderBy(x => x.DiscNumber));
                    }
                    else
                    {
                        Playlist = new ObservableCollection<PlaylistTags>(Playlist.OrderByDescending(x => x.DiscNumber));
                    }
                    break;
                case PlaylistColumnType.Genre:
                    if (ascending)
                    {
                        Playlist = new ObservableCollection<PlaylistTags>(
                            Playlist
                                .OrderBy(x => x.GenreString)
                                .ThenBy(x => x.ArtistString)
                                .ThenBy(x => x.Album)
                                .ThenBy(x => x.Track)
                                .ThenBy(x => x.DiscNumber)
                        );
                    }
                    else
                    {
                        Playlist = new ObservableCollection<PlaylistTags>(
                            Playlist
                                .OrderByDescending(x => x.GenreString)
                                .ThenBy(x => x.ArtistString)
                                .ThenBy(x => x.Album)
                                .ThenBy(x => x.Track)
                                .ThenBy(x => x.DiscNumber)
                            );
                    }
                    break;
                case PlaylistColumnType.Length:
                    if (ascending)
                    {
                        Playlist = new ObservableCollection<PlaylistTags>(Playlist.OrderBy(x => x.Length));
                    }
                    else
                    {
                        Playlist = new ObservableCollection<PlaylistTags>(Playlist.OrderByDescending(x => x.Length));
                    }
                    break;
                case PlaylistColumnType.Title:
                    if (ascending)
                    {
                        Playlist = new ObservableCollection<PlaylistTags>(Playlist.OrderBy(x => x.Title));
                    }
                    else
                    {
                        Playlist = new ObservableCollection<PlaylistTags>(Playlist.OrderByDescending(x => x.Title));
                    }
                    break;
                case PlaylistColumnType.Track:
                    if (ascending)
                    {
                        Playlist = new ObservableCollection<PlaylistTags>(
                            Playlist
                                .OrderBy(x => x.Track)
                                .ThenBy(x => x.ArtistString)
                                .ThenBy(x => x.Album)
                                .ThenBy(x => x.DiscNumber)
                        );
                    }
                    else
                    {
                        Playlist = new ObservableCollection<PlaylistTags>(
                            Playlist
                                .OrderByDescending(x => x.Track)
                                .ThenBy(x => x.ArtistString)
                                .ThenBy(x => x.Album)
                                .ThenBy(x => x.DiscNumber)
                        );
                    }
                    break;
                case PlaylistColumnType.Year:
                    if (ascending)
                    {
                        Playlist = new ObservableCollection<PlaylistTags>(Playlist.OrderBy(x => x.Year));
                    }
                    else
                    {
                        Playlist = new ObservableCollection<PlaylistTags>(Playlist.OrderByDescending(x => x.Year));
                    }
                    break;
                default:
                    return;
            }

            NotifyPropertyChanged(nameof(Playlist));
        }


        private readonly DelegateCommand _playTrackCommand;
        public ICommand PlayTrackCommand => _playTrackCommand;
        private void OnPlayTrackCommandHandler(object obj)
        {
            var track = (PlaylistTags)obj;
            InvokePlayTrackEvent(track);
        }

        private readonly DelegateCommand _removeTrackCommand;
        public ICommand RemoveTrackCommand => _removeTrackCommand;
        private void OnRemoveTrackCommandHandler(object obj)
        {
            var selectedItem = (PlaylistTags)obj;
            Playlist.Remove(selectedItem);
        }

        private readonly DelegateCommand _removeAllTracksCommand;
        public ICommand RemoveAllTracksCommand => _removeAllTracksCommand;
        private void OnRemoveAllTrackCommandHandler(object obj)
        {
            DeleteAllTracks();
        }

        private readonly DelegateCommand _createNewPlaylistFromSelectionCommand;
        public ICommand CreateNewPlaylistFromSelectionCommand => _createNewPlaylistFromSelectionCommand;
        private void OnCreateNewPlaylistFromSelectionCommand(object obj)
        {
            InvokeAddLibraryTracksToPlaylistEvent(true, ConvertSelectedItemsToTagsList(obj));
        }

        private readonly DelegateCommand _addSelectionToPlaylistCommand;
        public ICommand AddSelectionToPlaylistCommand => _addSelectionToPlaylistCommand;
        private void OnAddSelectionToPlaylistCommand(object obj)
        {
            InvokeAddLibraryTracksToPlaylistEvent(false, ConvertSelectedItemsToTagsList(obj));
        }
        #endregion

        #region events
        public event EventHandler<AddTracksToPlaylistEventArgs>? AddLibraryTracksToPlaylistEvent;
        private void InvokeAddLibraryTracksToPlaylistEvent(bool newPlaylist, List<LibLalaTags> tags)
        {
            var args = new AddTracksToPlaylistEventArgs(tags, newPlaylist);
            AddLibraryTracksToPlaylistEvent?.Invoke(this, args);
        }

        public event EventHandler<PlayTrackEventArgs>? PlayTrackEvent;
        protected virtual void InvokePlayTrackEvent(PlaylistTags tags)
        {
            if (tags is null)
            {
                return;
            }

            foreach (var item in Playlist)
            {
                item.IsPlaying = false;
            }
            tags.IsPlaying = true;
            NotifyPropertyChanged(nameof(CurrentIndex));
            PlayTrackEvent?.Invoke(this, new PlayTrackEventArgs(tags.Path, Name));
        }
        #endregion

        private void UpdatePlaylistInfo()
        {
            NotifyPropertyChanged(nameof(NumberOfTracks));
            NotifyPropertyChanged(nameof(TotalPlaytime));
        }

        private static List<LibLalaTags> ConvertSelectedItemsToTagsList(object obj)
        {
            var items = (System.Collections.IList)obj;
            var tagsList = new List<LibLalaTags>();
            foreach (PlaylistTags? tags in items)
            {
                if (tags is { })
                {
                    tagsList.Add(tags);
                }
            }
            return tagsList;
        }

        public int CurrentIndex
        {
            get
            {
                PlaylistTags? tags = Playlist.FirstOrDefault(x => x.IsPlaying);
                if (tags is null) { return -1; }
                return Playlist.IndexOf(tags);
            }
        }
        public int NumberOfTracks => Playlist.Count;
        public int TotalPlaytime => Playlist.Sum(track => track.Length);

        internal void DisplayChangedHandler(object? sender, DisplayLibChangedEventArgs e)
        {
            DeleteAllTracks();
            AddTracks(e.Tracks.ToList());
        }

        internal void AddTracksToPlaylistHandler(object? sender, AddTracksToPlaylistEventArgs e)
        {
            if (e.NewPlaylist) { DeleteAllTracks(); }
            AddTracks(e.Tracks);
        }

        public PlaylistTags CurrentTrack => Playlist[CurrentIndex];
        public PlaylistTags? NextTrack
        {
            get
            {
                if (PlaylistShuffleRepeatState == ShuffleRepeatState.RepeatOne)
                {
                    return Playlist[CurrentIndex];
                }

                if (PlaylistShuffleRepeatState == ShuffleRepeatState.Shuffle)
                {
                    var newIndex = new Random().Next(0, Playlist.Count - 1);
                    return Playlist[newIndex];
                }

                if (CurrentIndex < Playlist.Count - 1)
                {
                    return Playlist[CurrentIndex + 1];
                }

                if (CurrentIndex != Playlist.Count - 1)
                {
                    return null;
                }
                if (PlaylistShuffleRepeatState == ShuffleRepeatState.RepeatAll)
                {
                    return Playlist[0];
                }

                return null;
            }
        }
        public PlaylistTags PreviousTrack
        {
            get
            {
                var previousIndex = 0;
                if (CurrentIndex > 0)
                {
                    previousIndex = CurrentIndex - 1;
                }
                return Playlist[previousIndex];
            }
        }

        public ObservableCollection<PlaylistTags> Playlist { get; private set; }

        public ShuffleRepeatState PlaylistShuffleRepeatState { get; set; }

        #region AddTrack(s)
        public void AddTrack(LibLalaTags tags)
        {
            AddTrack(new PlaylistTags(tags));
        }
        public void AddTrack(PlaylistTags tags)
        {
            Playlist.Add(tags);
            NotifyPropertyChanged(nameof(Playlist));
        }

        public void AddTracks(List<LalaTags> tagsRange)
        {
            if (tagsRange is null)
            {
                return;
            }

            foreach (var tags in tagsRange)
            {
                AddTrack(tags);
            }
            NotifyPropertyChanged(nameof(Playlist));
        }

        public void AddTracks(List<LibLalaTags> tagsRange)
        {
            if (tagsRange is null)
            {
                return;
            }

            foreach (var tags in tagsRange)
            {
                AddTrack(tags);
            }
            NotifyPropertyChanged(nameof(Playlist));
        }
        public void AddTracks(List<PlaylistTags> tagsRange)
        {
            if (tagsRange is null)
            {
                return;
            }

            foreach (var tags in tagsRange)
            {
                AddTrack(tags);
            }
            NotifyPropertyChanged(nameof(Playlist));
        }
        #endregion

        #region DeleteTrack(s)
        public void DeleteTrack(int index)
        {
            Playlist.RemoveAt(index);
            NotifyPropertyChanged(nameof(Playlist));
        }

        public void DeleteTracks(int startIndex, int numberOfTracks)
        {
            for (var i = 0; i < numberOfTracks; ++i)
            {
                DeleteTrack(startIndex);
            }
            NotifyPropertyChanged(nameof(Playlist));
        }

        public void DeleteAllTracks()
        {
            Playlist.Clear();
            NotifyPropertyChanged(nameof(Playlist));
        }
        #endregion

        public void ManualLoadHandler(object? sender, ManualLoadEventArgs e)
        {
            if (e is null)
            {
                return;
            }

            var path = e.Path;
            var tags = new TagReader().Read(path);
            AddTrack(tags);
        }

        public void PlayNextTrackHandler(object? sender, EventArgs e)
        {
            if (NextTrack is { })
            {
                InvokePlayTrackEvent(NextTrack);
            }
        }

        public void PlayLastTrackHandler(object? sender, EventArgs e)
        {
            if (HasTracks)
            {
                InvokePlayTrackEvent(PreviousTrack);
            }
        }

        public bool HasTracks => Playlist.Count > 0;
        public string Name { get; private set; }

        public void ShuffleRepeatChangedHandler(object? sender, ChangeShuffleRepeatEventArgs e)
        {
            if (e is null)
            {
                return;
            }

            PlaylistShuffleRepeatState = e.ShuffleRepeatState;
        }
    }
}
