using System.IO;
using NUnit.Framework;

namespace LibLala.Unittests.MusicScanner
{
    public class MusicDatabaseTest
    {
        [Test]
        public void TestShouldCreateDatabase()
        {
            var db = new LibLala.MusicScanner.MusicDatabase();
            File.Delete(db.DbPath);
            Assert.False(File.Exists(db.DbPath));
            db.Connect("");
            db.EnsureDatabase();
            Assert.True(File.Exists(db.DbPath));
            var tags = new LibLala.TagReader.Tags();
            tags.Album = "the album";
            tags.Title = "the title";
            tags.length = 345;
            tags.Artist = new string[] { "the artist", "the other guy" };
            tags.path = "/this/is/the/path";
            db.AddTagsToDatabase(tags);
            var result = db.GetTag(tags.path);
            Assert.AreEqual(tags.Artist[0], result.Artist[0]);
        }
    }
}
