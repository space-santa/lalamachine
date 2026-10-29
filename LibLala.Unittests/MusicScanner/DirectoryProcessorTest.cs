using System;
using System.IO;
using LibLala.LibLalaTagReader;
using LibLala.MusicScanner;
using NSubstitute;
using NUnit.Framework;
using TestHelpers;

namespace LibLala.Unittests.MusicScanner
{
    public class DirectoryProcessorTest
    {
        private string _musicDir = "";

        [SetUp]
        public void SetUp()
        {
            _musicDir = Path.Combine(Path.GetTempPath(), $"lalatest-{Guid.NewGuid()}");
            Directory.CreateDirectory(_musicDir);
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(_musicDir, recursive: true);
        }

        [Test]
        public void UpperCaseExtensionIsScanned()
        {
            TestMp3.Write(Path.Combine(_musicDir, "LOUD.MP3"), "Loud");
            var db = Substitute.For<IMusicDatabase>();

            var processor = new DirectoryProcessor(db, _musicDir);
            processor.ProcessDirectory();

            Assert.That(processor.FileCount, Is.EqualTo(1));
            db.Received(1).AddTagsToDatabase(Arg.Is<LibLalaTags>(x => x.Title == "Loud"));
        }

        [Test]
        public void UnreadableFileIsSkippedAndTheRestIsSaved()
        {
            var brokenPath = Path.Combine(_musicDir, "broken.mp3");
            TestMp3.WriteBroken(brokenPath);
            TestMp3.Write(Path.Combine(_musicDir, "good.mp3"), "Good");
            var db = Substitute.For<IMusicDatabase>();

            var processor = new DirectoryProcessor(db, _musicDir);
            processor.ProcessDirectory();

            db.Received(1).AddTagsToDatabase(Arg.Is<LibLalaTags>(x => x.Title == "Good"));
            db.Received(1).SaveChanges();
            Assert.That(processor.SkippedFiles, Is.EqualTo(new[] { brokenPath }));
        }

        [Test]
        public void UntaggedFileIsTitledByItsFileName()
        {
            var path = Path.Combine(_musicDir, "No Tags Here.mp3");
            TestMp3.Write(path);

            var tags = new TagReader().Read(path);

            Assert.That(tags.Title, Is.EqualTo("No Tags Here"));
        }
    }
}
