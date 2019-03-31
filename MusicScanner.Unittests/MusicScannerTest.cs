using System;
using Xunit;
using MusicScanner;
using System.Collections.Generic;
using TagReader;
using System.Linq;

namespace MusicScanner.Unittests
{
    public class MusicDatabaseMock : IMusicDatabase
    {
        List<Tags> args;

        public List<Tags> Args
        {
            get
            {
                List<Tags> SortedList = args.OrderBy(o=>o.comment).ToList();
                return SortedList;
            }
        }

        public MusicDatabaseMock()
        {
            args = new List<Tags>();
        }
        public void EnsureDatabase()
        {
            // do things to make the database
        }

        public void AddTagsToDatabase(Tags tags)
        {
            args.Add(tags);
        }
    }
    public class MusicScannerTest
    {

        [Fact]
        public void Test1()
        {
            var tester = new MusicDatabaseMock();
            var path = "../../../testdata/";
            MusicScanner.ProcessDirectory(path, tester);
            var args = tester.Args;
            Assert.Equal("10A", args[0].comment);
            Assert.Equal("1A/12A", args[1].comment);
            Assert.Equal("5A", args[2].comment);
        }
    }
}
