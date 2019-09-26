using System.Collections.Generic;
using LalaDb.Data;
using LalaDb.Model;
using LibLala.TagReader;

namespace Lalamachine.Wpf.ViewModel
{

    public class PlaylistViewModel : PlaylistBaseViewModel
    {
        private readonly PlaylistModel _playlistModel;

        public PlaylistViewModel(LalaContext context) : base("MAIN")
        {
            _playlistModel = new PlaylistModel(context);
            LoadLastPlaylist();
        }

        private void LoadLastPlaylist()
        {
            var tracks = _playlistModel.getPlaylistTracks(LibLala.Constants.MISC_PLAYLIST_NAME);
            AddTracks(tracks);
        }

        public void SavePlaylist()
        {
            if (_playlistModel != null)
            {
                var tagsList = new List<Tags>();
                foreach (var item in Playlist) { tagsList.Add(item); }
                _playlistModel.savePlaylist(LibLala.Constants.MISC_PLAYLIST_NAME, tagsList);
            }
        }
    }
}
