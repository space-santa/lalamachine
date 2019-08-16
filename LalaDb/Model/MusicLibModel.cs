using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LalaDb.Data;
using Microsoft.EntityFrameworkCore;

namespace LalaDb.Model
{
    public class MusicLibModel
    {
        private readonly LalaContext _context;
        private readonly ScannerDb _scannerDb;
        public bool Scanning { get; set; }

        public MusicLibModel()
        {
            LibLala.Utils.EnsureLaladir();
            _context = new LalaContext();
            _context.Database.Migrate();
            _scannerDb = new ScannerDb(_context);
            Scanning = false;
        }

        public string[] genreList(string searchString)
        {
            if (Scanning) { return new string[0]; }

            var list = _context.Genres
                .Where(x => x.Name.Contains(searchString, System.StringComparison.OrdinalIgnoreCase))
                .Select(x => x.Name)
                .OrderBy(x => x)
                .ToArray();
            return list;
        }
        public string[] artistList(string genreFilter, string searchString)
        {
            if (Scanning) { return new string[0]; }

            string[] list;

            if (genreFilter.Length > 0)
            {
                list = _context.GenreTracks
                               .Where(x => x.Genre.Name == genreFilter)
                               .SelectMany(x => x.Track.ArtistTracks)
                               .Select(x => x.Artist.Name).Distinct().ToArray();
            }
            else
            {
                list = _context.Artists.Where(x => x.Name.Contains(searchString, System.StringComparison.OrdinalIgnoreCase)).Select(x => x.Name).OrderBy(x => x).ToArray();
            }

            return list;
        }

        public string[] albumList(string artistFilter, string genreFilter, string searchString)
        {
            if (Scanning) { return new string[0]; }

            string[] list;

            if (artistFilter.Length > 0)
            {
                list = _context.ArtistTracks
                               .Where(x => x.Artist.Name == artistFilter)
                               .Select(x => x.Track)
                               .Select(x => x.Album.Name).Distinct().ToArray();
            }
            else if (genreFilter.Length > 0)
            {
                list = _context.GenreTracks
                               .Where(x => x.Genre.Name == genreFilter)
                               .Select(x => x.Track)
                               .Select(x => x.Album.Name).Distinct().ToArray();
            }
            else if (searchString == "")
            {
                list = _context.Albums.Select(x => x.Name).ToArray();
            }
            else
            {
                try
                {
                    list = _context.Albums.Where(x => x.Name.Contains(searchString, System.StringComparison.OrdinalIgnoreCase)).Select(x => x.Name).OrderBy(x => x).ToArray();
                }
                catch (NullReferenceException)
                {
                    list = new string[0];
                }
            }

            return list;
        }

        public Track[] displayLib(string albumFilter, string artistFilter, string genreFilter, string searchString)
        {
            if (Scanning) { return new Track[0]; }

            Track[] list;

            if (albumFilter.Length > 0)
            {
                try
                {
                    list = _context.Albums.Single(x => x.Name == albumFilter).Tracks.ToArray();
                    return list;
                }
                catch (InvalidOperationException)
                {
                    //
                }
            }

            if (artistFilter.Length > 0)
            {
                list = _context.ArtistTracks.Where(x => x.Artist.Name == artistFilter).Select(x => x.Track).ToArray();
                return list;
            }

            if (genreFilter.Length > 0)
            {
                list = _context.GenreTracks.Where(x => x.Genre.Name == genreFilter).Select(x => x.Track).ToArray();
                return list;
            }

            if (searchString == "")
            {
                list = _context.Tracks
                            .Include(track => track.Album)
                            .Include(track => track.GenreTracks)
                                .ThenInclude(gt => gt.Genre)
                            .Include(track => track.ArtistTracks)
                                .ThenInclude(at => at.Artist)
                            .ToArray();
            }
            else
            {
                try
                {
                    list = _context.Tracks.Where(x => x.Title.Contains(searchString, System.StringComparison.OrdinalIgnoreCase)).ToArray();
                }
                catch (System.NullReferenceException)
                {
                    return new Track[0];
                }
            }

            return list;
        }

        public List<LalaTags> getAlbumTracks(string name)
        {
            if (name.Length < 1 || name == LibLala.Constants.ALL)
            { return new List<LalaTags>(); }
            var tracks = _context.Albums.Single(x => x.Name == name).Tracks;
            var tagList = new List<LalaTags>();
            foreach (var track in tracks)
            {
                var lalaTags = new LalaTags(track);
                tagList.Add(lalaTags);
            }
            return tagList;
        }

        public LalaTags getMetadataForMrl(string path)
        {
            path = LibLala.Utils.Utils.RemoveFilePrefix(path);
            var tags = new LalaTags(_context.Tracks.Single(x => Path.GetFullPath(x.Path) == Path.GetFullPath(path)));
            return tags;
        }

        public async Task scanAsync(string path)
        {
            if (Scanning)
            {
                return;
            }
            Scanning = true;
            await Task.Run(() => LibLala.MusicScanner.MusicScanner.ProcessDirectory(path, _scannerDb));
            Scanning = false;
        }
    }
}
