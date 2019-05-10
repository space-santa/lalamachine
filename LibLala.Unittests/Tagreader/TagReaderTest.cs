using System;
using LibLala.TagReader;
using NUnit.Framework;

namespace LibLala.Unittests.Tagreader
{
    public class TagReaderTest
    {
        [Test]
        public void EmptyPathShouldThrowTagReaderException()
        {
            var ex = Assert.Throws<TagReaderException>(() => TagReader.TagReader.Read(""));
            Assert.AreEqual("You must give a value for path.", ex.Message);
        }

        [Test]
        public void BadPathShouldThrowTagReaderException()
        {
            var ex = Assert.Throws<TagReaderException>(() => TagReader.TagReader.Read("lalalala"));
            Assert.AreEqual("Can't open lalalala.", ex.Message);
        }

        [Test]
        [Category("Integration")]
        public void BadMp3ShouldThrowTagReaderException()
        {
            var ex = Assert.Throws<TagReaderException>(() => TagReader.TagReader.Read("../../../TagReader/testdata/bad.mp3"));
            Assert.AreEqual("Can't open ../../../TagReader/testdata/bad.mp3.", ex.Message);
        }

        [Test]
        [Category("Integration")]
        public void Mp3PathShouldReturnTags()
        {
            var tags = TagReader.TagReader.Read("../../../TagReader/testdata/2-06_Finale.mp3");
            Assert.AreEqual(2, tags.discNumber);
        }

        [Test]
        [Category("Integration")]
        public void Mp3PathShouldReturnTagsLength()
        {
            var tags = TagReader.TagReader.Read("../../../TagReader/testdata/rip.mp3");
            Assert.AreEqual(59, tags.duration.Seconds);
            Assert.AreEqual(4, tags.duration.Minutes);
            Assert.AreEqual("Varieté", tags.album);
        }

        [Test]
        [Category("Integration")]
        public void Mp3PathShouldReturnTagsArtist()
        {
            var tags = TagReader.TagReader.Read("../../../TagReader/testdata/left.mp3");
            Assert.AreEqual(1, tags.discNumber);
        }

        [Test]
        [Category("Integration")]
        public void M4aPathShouldReturnTags()
        {
            var path = "../../../TagReader/testdata/down_under.m4a";
            var tags = TagReader.TagReader.Read(path);
            Assert.AreEqual(1, tags.discNumber);
            Assert.AreEqual(System.IO.Path.GetFullPath(path), tags.path);
        }
    }
}
