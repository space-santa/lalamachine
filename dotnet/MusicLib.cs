using dotnet.Data;
using Microsoft.EntityFrameworkCore;
using Qml.Net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnet
{
    [Signal("scanDone")]
    public class MusicLib
    {
        LalaContext _context;
        ScannerDb _scannerDb;

        public MusicLib()
        {
            _context = new LalaContext();
            _context.Database.Migrate();
            _scannerDb = new ScannerDb(_context);
            searchString = "";
        }

        public string searchString { get; set; }

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
                var list = _context.Artists.Where(x => x.Name.Contains(searchString)).Select(x => x.Name).OrderBy(x => x).ToArray();
                list = list.Prepend(Constants.ALL).ToArray();
                return Newtonsoft.Json.JsonConvert.SerializeObject(list);
            }
        }
        public string albumList
        {
            get
            {
                var list = _context.Albums.Where(x => x.Name.Contains(searchString, System.StringComparison.OrdinalIgnoreCase)).Select(x => x.Name).OrderBy(x => x).ToArray();
                list = list.Prepend(Constants.ALL).ToArray();
                return Newtonsoft.Json.JsonConvert.SerializeObject(list);
            }
        }
        public string displayLib
        {
            get
            {
                var list = _context.Tracks.Where(x => x.Title.Contains(searchString, System.StringComparison.OrdinalIgnoreCase)).ToArray();
                var tagList = new List<LalaTags>();

                foreach (var track in list)
                {
                    LalaTags lalaTags = new LalaTags(track, _context);
                    tagList.Add(lalaTags);
                }
                return Newtonsoft.Json.JsonConvert.SerializeObject(tagList);
            }
        }
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
            LalaTags tags = new LalaTags(_context.Tracks.Single(x => x.Path == path), _context);
            return tags.ToJson();
        }

        public string getAlbumTracks(string name)
        {
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
