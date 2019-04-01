using dotnet.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace dotnet
{
    public class MusicLib
    {
        LalaContext _context;
        ScannerDb _scannerDb;

        public MusicLib()
        {
            _context = new LalaContext();
            _context.Database.Migrate();
            _scannerDb = new ScannerDb(_context);
        }

        public bool scanning { get; set; }

        public async void scanAsync(string path)
        {
            if (scanning)
            {
                return;
            }
            scanning = true;
            await Task.Run(() => MusicScanner.MusicScanner.ProcessDirectory(path, _scannerDb));
            scanning = false;
        }

        public void getMetadataForMrl(string mrl)
        {

        }

    }
}