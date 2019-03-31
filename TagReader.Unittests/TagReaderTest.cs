using System;
using Xunit;
using TagReader;

namespace TagReader.Unittests
{
    public class TagReaderTest
    {
        [Fact]
        public void EmptyPathShouldThrowTagReaderException()
        {
            var ex = Assert.Throws<TagReaderException>(() => TagReader.Read(""));
            Assert.Equal("You must give a value for path.", ex.Message);
        }

        [Fact]
        public void BadPathShouldThrowTagReaderException()
        {
            var ex = Assert.Throws<TagReaderException>(() => TagReader.Read("lalalala"));
            Assert.Equal("Can't open lalalala.", ex.Message);
        }

        [Fact]
        public void BadMp3ShouldThrowTagReaderException()
        {
            var ex = Assert.Throws<TagReaderException>(() => TagReader.Read("../../../testdata/bad.mp3"));
            Assert.Equal("Can't open ../../../testdata/bad.mp3.", ex.Message);
        }

        [Fact]
        public void Mp3PathShouldReturnTags()
        {
            // ./bin/Debug/netcoreapp2.1 -> ./testdata/2.06_Finale.mp3
            var tags = TagReader.Read("../../../testdata/2-06_Finale.mp3");
            Assert.Equal<uint>(2, tags.discNumber);
        }

        [Fact]
        public void Mp3PathShouldReturnTagsLength()
        {
            var tags = TagReader.Read("../../../testdata/rip.mp3");
            Assert.Equal<int>(59, tags.duration.Seconds);
            Assert.Equal<int>(4, tags.duration.Minutes);
            Assert.Equal("Varieté", tags.album);
        }

        [Fact]
        public void Mp3PathShouldReturnTagsArtist()
        {
            var tags = TagReader.Read("../../../testdata/left.mp3");
            Assert.Equal<uint>(1, tags.discNumber);
        }

        [Fact]
        public void M4aPathShouldReturnTags()
        {
            // ./bin/Debug/netcoreapp2.1 -> ./testdata/down_under.m4a
            var path = "../../../testdata/down_under.m4a";
            var tags = TagReader.Read(path);
            Assert.Equal<uint>(1, tags.discNumber);
            Assert.Equal(System.IO.Path.GetFullPath(path), tags.path);
        }
    }
}
