using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using LalaDb.Data;
using LibLala.DomainPrimitives;
using LibLala.LibLalaTagReader;

namespace Lalamachine.ViewModel
{
    public sealed class SortMessage
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
        private readonly IDialogService _dialogs;
        private string _name;
        // Where the playing track was after it got removed, so that next and previous go on from there and not from the top.
        private int _removedPlayingIndex = -1;

        public PlaylistBaseViewModel(string name, IDialogService dialogs, ITrackPlaylists trackPlaylists)
        {
            ArgumentNullException.ThrowIfNull(trackPlaylists);

            _name = name;
            _dialogs = dialogs;
            TrackPlaylists = trackPlaylists;
            Playlist = new ObservableCollection<PlaylistTags>();
            _playTrackCommand = new DelegateCommand(OnPlayTrackCommandHandler);
            _removeTrackCommand = new DelegateCommand(OnRemoveTrackCommandHandler);
            _removeAllTracksCommand = new DelegateCommand(OnRemoveAllTrackCommandHandler);
            _replaceQueueWithSelectionCommand = new DelegateCommand(OnReplaceQueueWithSelectionCommand);
            _addSelectionToQueueCommand = new DelegateCommand(OnAddSelectionToQueueCommand);
            _toggleLikedCommand = new DelegateCommand(OnToggleLikedCommand);
            _sortCommand = new DelegateCommand(OnSortCommandHandler);

            PropertyChanged += UpdatePlaylistInfoHandler;
            TrackPlaylists.LikedChanged += LikedChangedHandler;
        }

        public ITrackPlaylists TrackPlaylists { get; }

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
        private void OnSortCommandHandler(object? obj)
        {
            if (obj is null) { return; }

            var sortMessage = obj as SortMessage;
            if (sortMessage is null)
            {
                return;
            }

            Sort(sortMessage);
            // Sorted, the place of a removed track doesn't mean anything any more.
            _removedPlayingIndex = -1;
            NotifyPropertyChanged(nameof(Playlist));
        }

        protected virtual void Sort(SortMessage sortMessage)
        {
            ArgumentNullException.ThrowIfNull(sortMessage);

            var ascending = sortMessage.Direction == ListSortDirection.Ascending;

            switch (sortMessage.Column.ColumnType)
            {
                case PlaylistColumnType.Album:
                    if (ascending)
                    {
                        Playlist = new ObservableCollection<PlaylistTags>(
                            Playlist
                                .OrderBy(x => x.Album)
                                .ThenBy(x => x.DiscNumber)
                                .ThenBy(x => x.Track)
                        );
                    }
                    else
                    {
                        Playlist = new ObservableCollection<PlaylistTags>(
                            Playlist
                                .OrderByDescending(x => x.Album)
                                .ThenBy(x => x.DiscNumber)
                                .ThenBy(x => x.Track)
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
                                .ThenBy(x => x.DiscNumber)
                                .ThenBy(x => x.Track)
                        );
                    }
                    else
                    {
                        Playlist = new ObservableCollection<PlaylistTags>(
                            Playlist
                                .OrderByDescending(x => x.ArtistString)
                                .ThenBy(x => x.Album)
                                .ThenBy(x => x.DiscNumber)
                                .ThenBy(x => x.Track)
                        );
                    }
                    break;
                case PlaylistColumnType.Bpm:
                    // Plenty of tracks have no BPM. They stay at the bottom either way, so that they don't bury the ones that do.
                    if (ascending)
                    {
                        Playlist = new ObservableCollection<PlaylistTags>(Playlist.OrderBy(x => x.BeatsPerMinute == 0).ThenBy(x => x.BeatsPerMinute));
                    }
                    else
                    {
                        Playlist = new ObservableCollection<PlaylistTags>(Playlist.OrderBy(x => x.BeatsPerMinute == 0).ThenByDescending(x => x.BeatsPerMinute));
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
                                .ThenBy(x => x.DiscNumber)
                                .ThenBy(x => x.Track)
                        );
                    }
                    else
                    {
                        Playlist = new ObservableCollection<PlaylistTags>(
                            Playlist
                                .OrderByDescending(x => x.GenreString)
                                .ThenBy(x => x.ArtistString)
                                .ThenBy(x => x.Album)
                                .ThenBy(x => x.DiscNumber)
                                .ThenBy(x => x.Track)
                            );
                    }
                    break;
                case PlaylistColumnType.Key:
                    // Like BPM, tracks without a key stay at the bottom either way.
                    if (ascending)
                    {
                        Playlist = new ObservableCollection<PlaylistTags>(
                            Playlist
                                .OrderBy(x => x.InitialKey.Length == 0)
                                .ThenBy(x => x.InitialKeyNumber)
                                .ThenBy(x => x.InitialKeyLetter)
                        );
                    }
                    else
                    {
                        Playlist = new ObservableCollection<PlaylistTags>(
                            Playlist
                                .OrderBy(x => x.InitialKey.Length == 0)
                                .ThenByDescending(x => x.InitialKeyNumber)
                                .ThenByDescending(x => x.InitialKeyLetter)
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
        }


        private readonly DelegateCommand _playTrackCommand;
        public ICommand PlayTrackCommand => _playTrackCommand;
        private void OnPlayTrackCommandHandler(object? obj)
        {
            if (obj is null) { return; }
            var track = (PlaylistTags)obj;
            InvokePlayTrackEvent(track);
        }

        private readonly DelegateCommand _removeTrackCommand;
        public ICommand RemoveTrackCommand => _removeTrackCommand;
        private void OnRemoveTrackCommandHandler(object? obj)
        {
            if (obj is null) { return; }
            RemoveTrack((PlaylistTags)obj);
        }

        protected virtual void RemoveTrack(PlaylistTags track)
        {
            ArgumentNullException.ThrowIfNull(track);

            var index = Playlist.IndexOf(track);
            if (track.IsPlaying)
            {
                _removedPlayingIndex = index;
            }
            else if (index >= 0 && index < _removedPlayingIndex)
            {
                --_removedPlayingIndex;
            }
            Playlist.Remove(track);
            NotifyPropertyChanged(nameof(Playlist));
        }

        private readonly DelegateCommand _removeAllTracksCommand;
        public ICommand RemoveAllTracksCommand => _removeAllTracksCommand;
        private void OnRemoveAllTrackCommandHandler(object? obj)
        {
            DeleteAllTracks();
        }

        private readonly DelegateCommand _replaceQueueWithSelectionCommand;
        public ICommand ReplaceQueueWithSelectionCommand => _replaceQueueWithSelectionCommand;
        private void OnReplaceQueueWithSelectionCommand(object? obj)
        {
            if (obj is null) { return; }
            InvokeAddTracksToQueueEvent(true, ConvertSelectedItemsToTagsList(obj));
        }

        private readonly DelegateCommand _addSelectionToQueueCommand;
        public ICommand AddSelectionToQueueCommand => _addSelectionToQueueCommand;
        private void OnAddSelectionToQueueCommand(object? obj)
        {
            if (obj is null) { return; }
            InvokeAddTracksToQueueEvent(false, ConvertSelectedItemsToTagsList(obj));
        }

        private readonly DelegateCommand _toggleLikedCommand;
        public ICommand ToggleLikedCommand => _toggleLikedCommand;
        private void OnToggleLikedCommand(object? obj)
        {
            if (obj is PlaylistTags { IsOutsideLibrary: false } track)
            {
                TrackPlaylists.ToggleLiked(track);
            }
        }
        #endregion

        #region events
        public event EventHandler<AddTracksToQueueEventArgs>? AddTracksToQueueEvent;
        private void InvokeAddTracksToQueueEvent(bool replaceQueue, List<LibLalaTags> tags)
        {
            var args = new AddTracksToQueueEventArgs(tags, replaceQueue);
            AddTracksToQueueEvent?.Invoke(this, args);
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
            _removedPlayingIndex = -1;
            NotifyPropertyChanged(nameof(CurrentIndex));
            PlayTrackEvent?.Invoke(this, new PlayTrackEventArgs(tags.Path, Name));
        }
        #endregion

        private void UpdatePlaylistInfo()
        {
            NotifyPropertyChanged(nameof(NumberOfTracks));
            NotifyPropertyChanged(nameof(TotalPlaytime));
            NotifyPropertyChanged(nameof(HasTracks));
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

        public void AddTracksToQueueHandler(object? sender, AddTracksToQueueEventArgs e)
        {
            if (e is null)
            {
                return;
            }

            // Nothing selected is no reason to lose the queue, that's what Remove All is for.
            if (e.ReplaceQueue && e.Tracks.Count > 0) { DeleteAllTracks(); }
            AddTracks(e.Tracks);
        }

        public void LikedChangedHandler(object? sender, LikedChangedEventArgs e)
        {
            if (e is null)
            {
                return;
            }

            foreach (var track in AllTracks.Where(x => e.Paths.Contains(x.Path)))
            {
                track.IsLiked = e.IsLiked;
            }
        }

        protected void MarkLiked(IEnumerable<PlaylistTags> tracks)
        {
            ArgumentNullException.ThrowIfNull(tracks);

            foreach (var track in tracks)
            {
                track.IsLiked = TrackPlaylists.IsLiked(track.Path);
            }
        }

        // The ones in Playlist, and whatever else a list has that it doesn't show right now.
        protected virtual IEnumerable<PlaylistTags> AllTracks => Playlist;

        // Moves it to where the track at that index is.
        public void MoveTrack(PlaylistTags track, int newIndex)
        {
            var oldIndex = Playlist.IndexOf(track);
            if (oldIndex < 0 || newIndex < 0 || newIndex >= Playlist.Count || oldIndex == newIndex)
            {
                return;
            }

            // Not Move, the grid doesn't show that.
            Playlist.RemoveAt(oldIndex);
            Playlist.Insert(newIndex, track);
            // What comes next after a removed playing track stays next, when another track moves across its place.
            _removedPlayingIndex += (newIndex < _removedPlayingIndex ? 1 : 0) - (oldIndex < _removedPlayingIndex ? 1 : 0);
            NotifyPropertyChanged(nameof(Playlist));
        }

        public PlaylistTags? NextTrack
        {
            get
            {
                if (Playlist.Count == 0)
                {
                    return null;
                }

                var currentIndex = CurrentIndex;

                // Nothing is playing yet (fresh start, playlist just replaced), so there is nothing to repeat.
                if (PlaylistShuffleRepeatState == ShuffleRepeatState.RepeatOne && currentIndex >= 0)
                {
                    return Playlist[currentIndex];
                }

                if (PlaylistShuffleRepeatState == ShuffleRepeatState.Shuffle)
                {
#pragma warning disable CA5394 // Do not use insecure randomness
                    var newIndex = Random.Shared.Next(Playlist.Count);
#pragma warning restore CA5394 // Do not use insecure randomness
                    return Playlist[newIndex];
                }

                // What took the place of a playing track that got removed comes next.
                var nextIndex = currentIndex >= 0 ? currentIndex + 1 : Math.Max(_removedPlayingIndex, 0);
                if (nextIndex < Playlist.Count)
                {
                    return Playlist[nextIndex];
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
                var previousIndex = Math.Clamp(PlayingPosition - 1, 0, Playlist.Count - 1);
                return Playlist[previousIndex];
            }
        }

        // Of the playing track, or of the one that was playing until it got removed. -1 if there is neither.
        protected int PlayingPosition => CurrentIndex >= 0 ? CurrentIndex : _removedPlayingIndex;

        public ObservableCollection<PlaylistTags> Playlist { get; private set; }

        // A place in the old list means nothing in a new one, so it takes the new place of a removed playing track, if it has one.
        protected void ReplacePlaylist(IEnumerable<PlaylistTags> tracks, int removedPlayingIndex = -1)
        {
            Playlist = new ObservableCollection<PlaylistTags>(tracks);
            _removedPlayingIndex = removedPlayingIndex;
        }

        public ShuffleRepeatState PlaylistShuffleRepeatState { get; set; }

        #region AddTrack(s)
        public void AddTrack(LibLalaTags tags)
        {
            AddTrack(new PlaylistTags(tags));
        }
        public void AddTrack(PlaylistTags tags)
        {
            ArgumentNullException.ThrowIfNull(tags);

            tags.IsLiked = TrackPlaylists.IsLiked(tags.Path);
            Playlist.Add(tags);
            NotifyPropertyChanged(nameof(Playlist));
        }

        public void AddTracks(IList<LalaTags> tagsRange)
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

        public void AddTracks(IList<LibLalaTags> tagsRange)
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
        public void AddTracks(IList<PlaylistTags> tagsRange)
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
        public void DeleteAllTracks()
        {
            Playlist.Clear();
            _removedPlayingIndex = -1;
            NotifyPropertyChanged(nameof(Playlist));
        }
        #endregion

        public async void ManualLoadHandler(object? sender, ManualLoadEventArgs e)
        {
            if (e is null)
            {
                return;
            }

            var path = e.Path;
            try
            {
                var tags = new TagReader().Read(path);
                AddTrack(new PlaylistTags(tags) { IsOutsideLibrary = true });
            }
            catch (TagReaderException ex)
            {
                await _dialogs.ShowMessageAsync($"{ex.Message}\n{ex.InnerException?.Message}").ConfigureAwait(true);
            }
        }

        // With NextTrackEventArgs, only says what it would play.
        public void PlayNextTrackHandler(object? sender, EventArgs e)
        {
            // Read NextTrack only once, in shuffle mode every read picks a different track.
            var nextTrack = NextTrack;
            if (e is NextTrackEventArgs request)
            {
                request.Track = nextTrack;
                request.List = nextTrack is null ? null : this;
                return;
            }
            if (nextTrack is { })
            {
                InvokePlayTrackEvent(nextTrack);
            }
        }

        // Plays it, like a double click.
        public void PlayTrack(PlaylistTags track) => InvokePlayTrackEvent(track);

        public void PlayLastTrackHandler(object? sender, EventArgs e)
        {
            if (HasTracks)
            {
                InvokePlayTrackEvent(PreviousTrack);
            }
        }

        public bool HasTracks => Playlist.Count > 0;

        public string Name
        {
            get => _name;
            protected set
            {
                _name = value;
                NotifyPropertyChanged();
            }
        }

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
