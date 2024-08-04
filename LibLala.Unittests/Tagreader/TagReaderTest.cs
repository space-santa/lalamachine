using LibLala.LibLalaTagReader;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using NUnit.Framework;

namespace LibLala.Unittests.Tagreader
{
    public class TagReaderTest
    {
        [Test]
        public void EmptyPathShouldThrowTagReaderException()
        {
            var ex = Assert.Throws<TagReaderException>(() => new TagReader().Read(""));
            Assert.That(ex is null || "You must give a value for path." == ex.Message);
        }

        [Test]
        public void BadPathShouldThrowTagReaderException()
        {
            var ex = Assert.Throws<TagReaderException>(() => new TagReader().Read("lalalala"));
            Assert.That(ex is null || "Can't open `lalalala`." == ex.Message);
        }

        [Test]
        public void EncodedPathShouldReturnTags()
        {
            var dirtyPath = "D:/OneDrive/musiclib/Various%20Artists/Ant-Man%20(Original%20Motion%20Picture%20Soundtr/Various%20Artists%20-%2007.%20I'll%20Call%20Him%20Antony.mp3";
            var cleanPath = "D:/OneDrive/musiclib/Various Artists/Ant-Man (Original Motion Picture Soundtr/Various Artists - 07. I'll Call Him Antony.mp3";
            var tagReader = new TagReader();
            var tagCreatorMock = Substitute.For<TagCreator>();
            tagReader.TagCreator = tagCreatorMock;
            tagReader.Read(dirtyPath);
            tagCreatorMock.Received().Create(cleanPath);
        }

        [Test]
        public void BadMp3ShouldThrowTagReaderException()
        {
            var testPath = "bad.mp3";
            var tagReader = new TagReader();
            var tagCreatorMock = Substitute.For<TagCreator>();
            tagCreatorMock.Create(testPath).Returns(x => { throw new TestException("dummy"); });
            tagReader.TagCreator = tagCreatorMock;
            var ex = Assert.Throws<TagReaderException>(() => { tagReader.Read(testPath); });
            Assert.That(ex is null || "Can't open `bad.mp3`." == ex.Message);
        }
    }
}
