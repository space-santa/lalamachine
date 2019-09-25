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
                _context.PlaylistTracks.RemoveRange(_context.PlaylistTracks.AsEnumerable().Where(x => x.Playlist?.Name == name));
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
                        TrackPath = track.Path,
                        Order = ++i
                    }
                ); ; ;
            }
            _context.SaveChanges();
        }

        public List<LalaTags> getPlaylistTracks(string name)
        {
            var trackPaths = _context.PlaylistTracks.AsEnumerable().Where(x => x.Playlist?.Name == name).OrderBy(x => x.Order).Select(x => x.TrackPath).ToArray();
            var tagList = new List<LalaTags>();
            foreach (var trackPath in trackPaths)
            {
                try
                {
                    var track = _context.Tracks.Single(x => x.Path == trackPath);
                    var lalaTags = new LalaTags(track);
                    tagList.Add(lalaTags);
                }
                catch (System.InvalidOperationException)
                {
                    // Empty playlist, database upgraded, whatever reason, no harm but also nothing else to do here.
                }
            }
            return tagList;
        }
    }
}
