using System;
using NUnit.Framework;
using LibLala.MusicScanner;
using System.Collections.Generic;
using LibLala.TagReader;
using System.Linq;

namespace LibLala.Unittests.MusicScanner
{
    public class MusicDatabaseMock : IMusicDatabase
    {
        List<Tags> args;

        public List<Tags> Args
        {
            get
            {
                List<Tags> SortedList = args.OrderBy(o => o.comment).ToList();
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

        public void SaveChanges()
        {
        }
    }
    public class MusicScannerTest
    {

        [Test]
        public void Test1()
        {
            var tester = new MusicDatabaseMock();
            var path = "../../../MusicScanner/testdata/";
            LibLala.MusicScanner.MusicScanner.ProcessDirectory(path, tester);
            var args = tester.Args;
            Assert.AreEqual("10A", args[0].comment);
            Assert.AreEqual("1A/12A", args[1].comment);
            Assert.AreEqual("5A", args[2].comment);
        }
    }
}
