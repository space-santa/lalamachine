using System.Collections.Generic;
using LalaDb.Data;
using LalaDb.Model;
using LibLala.LibLalaTagReader;

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
