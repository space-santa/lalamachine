using System.Collections.Generic;
using System.Linq;
using LalaDb.Data;
using LibLala.LibLalaTagReader;
using Microsoft.EntityFrameworkCore;

namespace LalaDb.Model
{
    public class PlaylistModel
    {
        private readonly LalaContext _context;

        public PlaylistModel(LalaContext context)
        {
            _context = context;
        }

        private DbSet<Playlist> Playlists => _context.Set<Playlist>();
        private DbSet<PlaylistTrack> PlaylistTracks => _context.Set<PlaylistTrack>();
        private DbSet<Track> Tracks => _context.Set<Track>();

        // Not the queue, that is stored like a playlist but it isn't one of the user's.
        public IList<string> PlaylistNames()
        {
            return Playlists
                .Where(x => x.Name != null && x.Name != LibLala.Constants.MISCPLAYLISTNAME)
                .Select(x => x.Name!)
                .ToList();
        }

        public void CreatePlaylist(string name)
        {
            Playlists.Add(new Playlist { Name = name });
            _context.SaveChanges();
        }

        public void RenamePlaylist(string name, string newName)
        {
            foreach (var playlist in Playlists.Where(x => x.Name == name))
            {
                playlist.Name = newName;
            }
            _context.SaveChanges();
        }

        public void DeletePlaylist(string name)
        {
            PlaylistTracks.RemoveRange(PlaylistTracks.Where(x => x.Playlist!.Name == name));
            Playlists.RemoveRange(Playlists.Where(x => x.Name == name));
            _context.SaveChanges();
        }

        public void SavePlaylist(string name, IList<LibLalaTags> tracks)
        {
            if (string.IsNullOrEmpty(name) || tracks is null)
            {
                return;
            }

            DeletePlaylist(name);
            var playlist = Playlists.Add(new Playlist { Name = name });

            var i = 0;
            foreach (var track in tracks)
            {
                PlaylistTracks.Add(
                    new PlaylistTrack
                    {
                        Playlist = playlist.Entity,
                        TrackPath = track.Path,
                        Order = ++i
                    }
                );
            }
            _context.SaveChanges();
        }

        // Does nothing if the playlist already has the track.
        public void AddTrack(string name, string path)
        {
            var playlist = Playlists.FirstOrDefault(x => x.Name == name);
            if (playlist is null || PlaylistTracks.Any(x => x.PlaylistId == playlist.PlaylistId && x.TrackPath == path))
            {
                return;
            }

            var lastOrder = PlaylistTracks.Where(x => x.PlaylistId == playlist.PlaylistId).Max(x => (int?)x.Order) ?? 0;
            PlaylistTracks.Add(new PlaylistTrack { PlaylistId = playlist.PlaylistId, TrackPath = path, Order = lastOrder + 1 });
            _context.SaveChanges();
        }

        public void RemoveTrack(string name, string path)
        {
            PlaylistTracks.RemoveRange(PlaylistTracks.Where(x => x.Playlist!.Name == name && x.TrackPath == path));
            _context.SaveChanges();
        }

        // Tracks that aren't given keep their place at the end. They are the ones the library doesn't have at the moment,
        // e.g. because of a scan while a drive was unplugged, and they are back after the next one.
        public void SaveOrder(string name, IEnumerable<string> paths)
        {
            var positions = new Dictionary<string, int>();
            foreach (var path in paths ?? [])
            {
                positions.TryAdd(path, positions.Count);
            }

            var order = 0;
            var rows = PlaylistTracks.Where(x => x.Playlist!.Name == name).OrderBy(x => x.Order).ToList();
            foreach (var row in rows.OrderBy(x => positions.GetValueOrDefault(x.TrackPath ?? "", int.MaxValue)))
            {
                row.Order = ++order;
            }
            _context.SaveChanges();
        }

        public IList<string> PlaylistsContaining(string path)
        {
            return PlaylistTracks
                .Where(x => x.TrackPath == path && x.Playlist!.Name != null)
                .Select(x => x.Playlist!.Name!)
                .Distinct()
                .ToList();
        }

        public IList<string> TrackPaths(string name)
        {
            return PlaylistTracks
                .Where(x => x.Playlist!.Name == name && x.TrackPath != null)
                .Select(x => x.TrackPath!)
                .ToList();
        }

        public IList<LalaTags> GetPlaylistTracks(string name)
        {
            var trackPaths = PlaylistTracks
                .Where(x => x.Playlist!.Name == name && x.TrackPath != null)
                .OrderBy(x => x.Order)
                .Select(x => x.TrackPath!)
                .ToList();

            // All in one query. The library has no index on the path, so one query per track gets slow for a long playlist.
            var tracks = Tracks
                .Include(track => track.Album!)
                .Include(track => track.GenreTracks!)
                    .ThenInclude(gt => gt.Genre!)
                .Include(track => track.ArtistTracks!)
                    .ThenInclude(at => at.Artist!)
                .Where(track => track.Path != null && trackPaths.Contains(track.Path))
                .AsEnumerable()
                .GroupBy(track => track.Path!)
                .ToDictionary(group => group.Key, group => group.First());

            var tagList = new List<LalaTags>();
            foreach (var trackPath in trackPaths)
            {
                // Not in the library (any more): database upgraded, a scan without the drive, whatever reason, no harm but also nothing else to do here.
                if (!tracks.TryGetValue(trackPath, out var track))
                {
                    continue;
                }

                try
                {
                    tagList.Add(LalaTags.Build(track));
                }
                catch (System.ArgumentException)
                {
                    // A broken row in the library must not stop the app from starting.
                }
            }
            return tagList;
        }
    }
}
