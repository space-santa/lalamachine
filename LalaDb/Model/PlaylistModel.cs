using System.Collections.Generic;
using System.Linq;
using LalaDb.Data;
using LibLala.TagReader;

namespace LalaDb.Model
{
    public class PlaylistModel
    {
        private readonly LalaContext _context;

        public PlaylistModel(LalaContext context)
        {
            _context = context;
        }

        public void deletePlaylist(string name)
        {
            try
            {
                _context.PlaylistTracks.RemoveRange(_context.PlaylistTracks.Where(x => x.Playlist.Name == name));
                _context.Playlists.Remove(_context.Playlists.Single(x => x.Name == name));
                _context.SaveChanges();
            }
            catch (System.InvalidOperationException)
            {
                //
            }

        }

        public void savePlaylist(string name, List<Tags> tracks)
        {
            deletePlaylist(name);
            var playlist = _context.Playlists.Add(new Playlist { Name = name });

            var i = 0;
            foreach (var track in tracks)
            {
                _context.PlaylistTracks.Add(
                    new PlaylistTrack
                    {
                        Playlist = playlist.Entity,
                        TrackId = track.TrackId,
                        Order = ++i
                    }
                ); ; ;
            }
            _context.SaveChanges();
        }

        public List<LalaTags> getPlaylistTracks(string name)
        {
            var trackIds = _context.PlaylistTracks.Where(x => x.Playlist.Name == name).OrderBy(x => x.Order).Select(x => x.TrackId).ToArray();
            var tagList = new List<LalaTags>();
            foreach (var trackId in trackIds)
            {
                var track = _context.Tracks.Single(x => x.TrackId == trackId);
                var lalaTags = new LalaTags(track);
                tagList.Add(lalaTags);
            }
            return tagList;
        }
    }
}
