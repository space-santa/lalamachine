using dotnet.Data;
using Microsoft.EntityFrameworkCore;

namespace dotnet
{
    public class MusicLib
    {
        LalaContext _context;

        public MusicLib()
        {
            _context = new LalaContext();
            _context.Database.Migrate();
        }

        public bool scanning { get; set; }

        public void getMetadataForMrl(string mrl)
        {

        }

    }
}