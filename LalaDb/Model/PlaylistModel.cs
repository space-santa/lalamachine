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

        // To the end, in the given order. A playlist has each track once, so it skips the ones that are already there.
        public void AddTracks(string name, IEnumerable<string> paths)
        {
            var playlist = Playlists.FirstOrDefault(x => x.Name == name);
            if (playlist is null)
            {
                return;
            }

            var rows = PlaylistTracks.Where(x => x.PlaylistId == playlist.PlaylistId);
            var present = rows.Select(x => x.TrackPath).ToHashSet();
            var order = rows.Max(x => (int?)x.Order) ?? 0;
            foreach (var path in paths ?? [])
            {
                if (present.Add(path))
                {
                    PlaylistTracks.Add(new PlaylistTrack { PlaylistId = playlist.PlaylistId, TrackPath = path, Order = ++order });
                }
            }
            _context.SaveChanges();
        }

        // In the given order, each once. All at once, so that it is either the old tracks or the new ones.
        public void ReplaceTracks(string name, IEnumerable<string> paths)
        {
            var playlist = Playlists.FirstOrDefault(x => x.Name == name);
            if (playlist is null)
            {
                return;
            }

            PlaylistTracks.RemoveRange(PlaylistTracks.Where(x => x.PlaylistId == playlist.PlaylistId));
            var order = 0;
            foreach (var path in (paths ?? []).Distinct())
            {
                PlaylistTracks.Add(new PlaylistTrack { PlaylistId = playlist.PlaylistId, TrackPath = path, Order = ++order });
            }
            _context.SaveChanges();
        }

        public void RemoveTracks(string name, IEnumerable<string> paths)
        {
            var removed = (paths ?? []).ToList();
            PlaylistTracks.RemoveRange(PlaylistTracks.Where(x => x.Playlist!.Name == name && removed.Contains(x.TrackPath!)));
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

        // The ones that have all of these tracks, not just some of them.
        public IList<string> PlaylistsContainingAll(IEnumerable<string> paths)
        {
            var distinctPaths = (paths ?? []).Distinct().ToList();
            return PlaylistTracks
                .Where(x => distinctPaths.Contains(x.TrackPath!) && x.Playlist!.Name != null)
                .GroupBy(x => x.Playlist!.Name!)
                .Where(group => group.Select(x => x.TrackPath).Distinct().Count() == distinctPaths.Count)
                .Select(group => group.Key)
                .ToList();
        }

        // In the order of the playlist. All of them, also the ones the library doesn't have at the moment.
        public IList<string> TrackPaths(string name)
        {
            return PlaylistTracks
                .Where(x => x.Playlist!.Name == name && x.TrackPath != null)
                .OrderBy(x => x.Order)
                .Select(x => x.TrackPath!)
                .ToList();
        }

        // All the tracks a playlist can be made of.
        public IList<string> LibraryTrackPaths()
        {
            return Tracks.Where(x => x.Path != null).Select(x => x.Path!).ToList();
        }

        public IList<LalaTags> GetPlaylistTracks(string name)
        {
            var trackPaths = TrackPaths(name);

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
