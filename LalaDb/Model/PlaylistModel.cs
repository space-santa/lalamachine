using System.Collections.Generic;
using System.Linq;
using LalaDb.Data;
using LibLala.LibLalaTagReader;

namespace LalaDb.Model
{
    public class PlaylistModel
    {
        private readonly LalaContext _context;

        public PlaylistModel(LalaContext context)
        {
            _context = context;
        }

        public void DeletePlaylist(string name)
        {
            try
            {
                _context.PlaylistTracks?.RemoveRange(_context.PlaylistTracks.AsEnumerable().Where(x => x.Playlist?.Name == name));
                _context.Playlists?.Remove(_context.Playlists.Single(x => x.Name == name));
                _context.SaveChanges();
            }
            catch (System.InvalidOperationException)
            {
                //
            }

        }

        public void SavePlaylist(string name, List<LibLalaTags> tracks)
        {
            if (string.IsNullOrEmpty(name) || tracks is null)
            {
                return;
            }

            DeletePlaylist(name);
            var playlist = _context.Playlists?.Add(new Playlist { Name = name });
            if (playlist is null)
            {
                return;
            }

            var i = 0;
            foreach (var track in tracks)
            {
                _context.PlaylistTracks?.Add(
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

        public List<LalaTags> GetPlaylistTracks(string name)
        {
            var trackPaths = _context.PlaylistTracks?.AsEnumerable().Where(x => x.Playlist?.Name == name).OrderBy(x => x.Order).Select(x => x.TrackPath).ToArray();
            var tagList = new List<LalaTags>();

            if (trackPaths is null) { return tagList; }

            foreach (var trackPath in trackPaths)
            {
                try
                {
                    var track = _context.Tracks?.Single(x => x.Path == trackPath);
                    if (track != null)
                    {

                        var lalaTags = LalaTags.Build(track);
                        tagList.Add(lalaTags);
                    }
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
