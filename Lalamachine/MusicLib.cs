using Lalamachine.Data;
using Microsoft.EntityFrameworkCore;
using Qml.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Lalamachine
{
    [Signal("scanDone")]
    public class MusicLib
    {
        private LalaContext _context;
        private ScannerDb _scannerDb;

        public MusicLib()
        {
            _context = new LalaContext();
            _context.Database.Migrate();
            _scannerDb = new ScannerDb(_context);
            searchString = "";
            genreFilter = "";
            artistFilter = "";
            albumFilter = "";
        }

        #region properties

        public string searchString { get; set; }

        private string _genreFilter;
        private string _artistFilter;
        private string _albumFilter;

        public string genreFilter
        {
            get => _genreFilter;
            set
            {
                _genreFilter = emptyWhenNullOrAll(value);
            }
        }

        public string artistFilter
        {
            get => _artistFilter;
            set
            {
                _artistFilter = emptyWhenNullOrAll(value);
            }
        }

        public string albumFilter
        {
            get => _albumFilter;
            set
            {
                _albumFilter = emptyWhenNullOrAll(value);
            }
        }

        private string emptyWhenNullOrAll(string value)
        {
            if (value == null || value == Constants.ALL)
            {
                return "";
            }

            return value;
        }

        public bool scanning { get; set; }

        public string genreList
        {
            get
            {
                var list = _context.Genres.Where(x => x.Name.Contains(searchString, System.StringComparison.OrdinalIgnoreCase)).Select(x => x.Name).OrderBy(x => x).ToArray();
                list = list.Prepend(Constants.ALL).ToArray();
                return Newtonsoft.Json.JsonConvert.SerializeObject(list);
            }
        }

        public string artistList
        {
            get
            {
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

                list = list.Prepend(Constants.ALL).ToArray();
                return Newtonsoft.Json.JsonConvert.SerializeObject(list);
            }
        }

        public string albumList
        {
            get
            {
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
                    catch (System.NullReferenceException)
                    {
                        list = new string[0];
                    }
                }

                list = list.Prepend(Constants.ALL).ToArray();
                return Newtonsoft.Json.JsonConvert.SerializeObject(list.ToArray());
            }
        }

        public string displayLib
        {
            get
            {
                Track[] list;

                if (albumFilter.Length > 0)
                {
                    try
                    {
                        list = _context.Albums.Single(x => x.Name == albumFilter).Tracks.ToArray();
                        return TracksToTagListString(list);
                    }
                    catch (InvalidOperationException)
                    {
                        //
                    }
                }

                if (artistFilter.Length > 0)
                {
                    list = _context.ArtistTracks.Where(x => x.Artist.Name == artistFilter).Select(x => x.Track).ToArray();
                    return TracksToTagListString(list);
                }

                if (genreFilter.Length > 0)
                {
                    list = _context.GenreTracks.Where(x => x.Genre.Name == genreFilter).Select(x => x.Track).ToArray();
                    return TracksToTagListString(list);
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
                        return "[]";
                    }
                }

                return TracksToTagListString(list);
            }
        }

        private string TracksToTagListString(Track[] tracks)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            var tagList = new List<LalaTags>();

            foreach (var track in tracks)
            {
                LalaTags lalaTags = new LalaTags(track, _context);
                tagList.Add(lalaTags);
            }

            sw.Stop();
            Console.WriteLine($"Creating tags took {sw.Elapsed}");

            return Newtonsoft.Json.JsonConvert.SerializeObject(tagList);
        }

        #endregion properties

        public async void scanAsync(string path)
        {
            if (scanning)
            {
                return;
            }
            scanning = true;
            await Task.Run(() => MusicScanner.MusicScanner.ProcessDirectory(path, _scannerDb));
            scanning = false;
            this.ActivateSignal("scanDone");
        }

        public string getMetadataForMrl(string path)
        {
            try
            {
                path = LibLala.Utils.RemoveFilePrefix(path);
                LalaTags tags = new LalaTags(_context.Tracks.Single(x => Path.GetFullPath(x.Path) == Path.GetFullPath(path)), _context);
                return tags.ToJson();
            }
            catch (System.InvalidOperationException)
            {
                return "{}";
            }
        }

        public string getAlbumTracks(string name)
        {
            if (name.Length < 1 || name == Constants.ALL)
            { return "{}"; }
            var tracks = _context.Albums.Single(x => x.Name == name).Tracks;
            var tagList = new List<LalaTags>();
            foreach (var track in tracks)
            {
                LalaTags lalaTags = new LalaTags(track, _context);
                tagList.Add(lalaTags);
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(tagList);
        }
    }
}
