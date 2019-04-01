using System;
using System.Collections.Generic;
using System.Text;
using dotnet.Data;
using MusicScanner;
using TagReader;

namespace dotnet
{
    public class ScannerDb : IMusicDatabase
    {
        LalaContext _context;

        public ScannerDb(LalaContext Context)
        {
            _context = Context;
        }

        public void AddTagsToDatabase(Tags tags)
        {
            throw new NotImplementedException();
        }

        public void EnsureDatabase()
        {
            throw new NotImplementedException();
        }
    }
}
