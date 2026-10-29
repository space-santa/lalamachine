using LibLala.DomainPrimitives;
using NUnit.Framework;

namespace LibLala.Unittests.DomainPrimitives
{
    public class BaseNameStringTest
    {
        private static AlbumName? NoAlbum() => null;

        [Test]
        public void NameAndNullAreNotEqual()
        {
            var album = new AlbumName("x");

            Assert.That(album == NoAlbum(), Is.False);
            Assert.That(album != NoAlbum(), Is.True);
            Assert.That(NoAlbum() != album, Is.True);
        }

        [Test]
        public void NullEqualsNull()
        {
            Assert.That(NoAlbum() == NoAlbum(), Is.True);
            Assert.That(NoAlbum() != NoAlbum(), Is.False);
        }

        [Test]
        public void NamesCompareByTrimmedText()
        {
            Assert.That(new AlbumName(" x ") == new AlbumName("x"), Is.True);
            Assert.That(new AlbumName("x") != new AlbumName("y"), Is.True);
        }
    }
}
