using System;
using Xunit;
using TagReader;

namespace TagReader.Unittests
{
    public class TagsTest
    {
        [Fact]
        public void DurationWithHoursShouldBeCorrectLength()
        {
            TimeSpan ts = new TimeSpan(0, 2, 4, 3, 999);
            Tags tags = new Tags();
            tags.duration = ts;
            Assert.Equal("124:03", tags.lengthString);
            Assert.Equal(124 * 60 + 3, tags.length);
        }

        [Fact]
        public void DurationWithTwoDigitSecondsShouldBeCorrectLength()
        {
            TimeSpan ts = new TimeSpan(0, 2, 4, 34, 15);
            Tags tags = new Tags();
            tags.duration = ts;
            Assert.Equal("124:34", tags.lengthString);
            Assert.Equal(124 * 60 + 34, tags.length);
        }

        [Fact]
        public void ArtistArrayDuplicatesShouldBeRemoved()
        {
            Tags tags = new Tags();
            tags.Artist = new string[2];
            tags.Artist[0] = "Therapy?";
            tags.Artist[1] = "Therapy?";
            Assert.Equal("Therapy?", tags.artistString);
        }

        [Fact]
        public void ArtistStringSetterShouldSplitCommaSeperatedString()
        {
            Tags tags = new Tags();
            tags.artistString = "Therapy?, Machine Head";
            Assert.Equal(2, tags.Artist.Length);
            Assert.Equal("Therapy?", tags.Artist[0]);
            Assert.Equal("Machine Head", tags.Artist[1]);
        }

        [Fact]
        public void ArtistStringSetterShouldRemoveDuplicates()
        {
            Tags tags = new Tags();
            tags.artistString = "Therapy?, Therapy?";
            Assert.Single(tags.Artist);
            Assert.Equal("Therapy?", tags.Artist[0]);
        }
    }
}