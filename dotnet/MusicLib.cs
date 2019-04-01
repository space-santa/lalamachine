using dotnet.Data;
using Microsoft.EntityFrameworkCore;

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

        public void rescan(string path)
        {
            MusicScanner.MusicScanner.ProcessDirectory(path, _scannerDb);
        }

        public void getMetadataForMrl(string mrl)
        {

        }

    }
}