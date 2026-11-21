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

        // A % in a file name is a letter of it.
        [Test]
        public void APath_IsOpenedAsItIs()
        {
            var path = System.IO.Path.GetFullPath("Rock 50%25 Off.mp3");
            var tagReader = new TagReader();
            var tagCreatorMock = Substitute.For<TagCreator>();
            tagReader.TagCreator = tagCreatorMock;
            tagReader.Read(path);
            tagCreatorMock.Received().Create(path);
        }

        [Test]
        public void AFileUri_IsOpenedAtItsPath()
        {
            var path = System.IO.Path.GetFullPath("Various Artists - 07. I'll Call Him 50%25.mp3");
            var tagReader = new TagReader();
            var tagCreatorMock = Substitute.For<TagCreator>();
            tagReader.TagCreator = tagCreatorMock;
            tagReader.Read(new System.Uri(path).AbsoluteUri);
            tagCreatorMock.Received().Create(path);
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
