using System;
using NUnit.Framework;
using PlaylistProvider;

namespace Tests
{
    public class PlaylistTest
    {
        [Test]
        public void GetPath_ValidString_ReturnsPath()
        {
            string path = Playlist.getPath("bob");
            Assert.True(path.EndsWith("\\AppData\\Roaming\\lalamachine\\playlists\\bob.json"));
        }

        [Test]
        public void GetPath_InValidString_RaisesException()
        {
            Assert.Throws<ArgumentException>( () => Playlist.getPath("") );
        }
    }
}
