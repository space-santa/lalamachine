using dotnet.Data;
using Microsoft.EntityFrameworkCore;
using Qml.Net;
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
                var list = _context.Genres.Where(x => x.Name.Contains(searchString)).Select(x => x.Name).ToArray();
                return Newtonsoft.Json.JsonConvert.SerializeObject(list);
            }
        }
        public string artistList
        {
            get
            {
                var list = _context.Artists.Where(x => x.Name.Contains(searchString)).Select(x => x.Name).ToArray();
                return Newtonsoft.Json.JsonConvert.SerializeObject(list);
            }
        }
        public string albumList
        {
            get
            {
                var list = _context.Albums.Where(x => x.Name.Contains(searchString)).Select(x => x.Name).ToArray();
                return Newtonsoft.Json.JsonConvert.SerializeObject(list);
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

    }
}