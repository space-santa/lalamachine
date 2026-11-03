using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using LalaDb.Data;
using LalaDb.Model;
using LibLala.LibLalaTagReader;

namespace Lalamachine.ViewModel
{

    public class QueueViewModel : PlaylistBaseViewModel
    {
        private readonly PlaylistModel _playlistModel;

        public QueueViewModel(LalaContext context, IDialogService dialogs, ITrackPlaylists trackPlaylists) : base("MAIN", dialogs, trackPlaylists)
        {
            _playlistModel = new PlaylistModel(context);
            _saveAsPlaylistCommand = new DelegateCommand(OnSaveAsPlaylist);
            PropertyChanged += PlaylistChangedHandler;
            LoadLastPlaylist();
        }

        private void PlaylistChangedHandler(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Playlist))
            {
                NotifyPropertyChanged(nameof(CanSaveAsPlaylist));
            }
        }

        // The queue is the only place for a track that is outside the library.
        public bool CanSaveAsPlaylist => HasTracks && !Playlist.Any(x => x.IsOutsideLibrary);

        private readonly DelegateCommand _saveAsPlaylistCommand;
        public ICommand SaveAsPlaylistCommand => _saveAsPlaylistCommand;
        private async void OnSaveAsPlaylist(object? obj)
        {
            if (CanSaveAsPlaylist)
            {
                await TrackPlaylists.SaveAsNewPlaylistAsync(Playlist.ToList<LibLalaTags>()).ConfigureAwait(true);
            }
        }

        private void LoadLastPlaylist()
        {
            var tracks = _playlistModel.GetPlaylistTracks(LibLala.Constants.MISCPLAYLISTNAME);
            AddTracks(tracks);
        }

        public void SavePlaylist()
        {
            if (_playlistModel != null)
            {
                var tagsList = new List<LibLalaTags>();
                foreach (var item in Playlist) { tagsList.Add(item); }
                _playlistModel.SavePlaylist(LibLala.Constants.MISCPLAYLISTNAME, tagsList);
            }
        }
    }
}
