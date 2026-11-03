using System.Collections.Generic;
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
            LoadLastPlaylist();
        }

        private readonly DelegateCommand _saveAsPlaylistCommand;
        public ICommand SaveAsPlaylistCommand => _saveAsPlaylistCommand;
        private async void OnSaveAsPlaylist(object? obj)
        {
            if (HasTracks)
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
