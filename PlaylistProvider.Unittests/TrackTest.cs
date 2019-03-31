using NUnit.Framework;
using PlaylistProvider;

namespace Tests
{
    public class TrackTest
    {
        private const string V = @"{
            'album': 'bob',
            'artist': 'bob artist',
            'genre': 'bob genre',
            'comment': 'bob 12 b',
            'track': '3',
            'title': 'bob title',
            'mrl': 'mrl://bob',
            'path': 'b/o/b',
            'length': 456,
            'lengthString': '3:12',
            'year': 1999,
            'discNumber': 2,
            'id': 3,
        }";
        string validJson = V;

        [Test]
        public void FromJson_ValidString_CreatesObject()
        {
            Track track = Track.FromJson(validJson);
            Assert.AreEqual(3, track.id);
            Assert.AreEqual("mrl://bob", track.mrl);
        }

        [Test]
        public void FromJson_InValidString_CreatesEmptyObject()
        {
            Track track = Track.FromJson("{'notKey': 999}");
            Assert.AreEqual(0, track.id);
            Assert.AreEqual("", track.mrl);
        }
    }
}
