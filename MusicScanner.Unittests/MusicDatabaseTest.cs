using System.IO;
using Xunit;
using TagReader;

namespace MusicScanner.Unittests
{
    public class MusicDatabaseTest
    {
        [Fact]
        public void TestShouldCreateDatabase()
        {
            var db = new MusicDatabase();
            File.Delete(db.DbPath);
            Assert.False(File.Exists(db.DbPath));
            db.Connect("");
            db.EnsureDatabase();
            Assert.True(File.Exists(db.DbPath));
            Tags tags = new Tags();
            tags.album = "the album";
            tags.title = "the title";
            tags.length = 345;
            tags.Artist = new string[] {"the artist", "the other guy"};
            tags.path = "/this/is/the/path";
            db.AddTagsToDatabase(tags);
            Tags result = db.GetTag(tags.path);
            Assert.Equal(tags.Artist[0], result.Artist[0]);
        }
    }
}
