using System;
using NUnit.Framework;
using LibLala.TagReader;

namespace LibLala.Unittests.Tagreader
{
    public class TagsTest
    {
        [Test]
        public void DurationWithHoursShouldBeCorrectLength()
        {
            TimeSpan ts = new TimeSpan(0, 2, 4, 3, 999);
            Tags tags = new Tags();
            tags.duration = ts;
            Assert.AreEqual("124:03", tags.lengthString);
            Assert.AreEqual(124 * 60 + 3, tags.length);
        }

        [Test]
        public void DurationWithTwoDigitSecondsShouldBeCorrectLength()
        {
            TimeSpan ts = new TimeSpan(0, 2, 4, 34, 15);
            Tags tags = new Tags();
            tags.duration = ts;
            Assert.AreEqual("124:34", tags.lengthString);
            Assert.AreEqual(124 * 60 + 34, tags.length);
        }

        [Test]
        public void ArtistArrayDuplicatesShouldBeRemoved()
        {
            Tags tags = new Tags();
            tags.Artist = new string[2] { "Therapy?", "Therapy?" };
            Assert.AreEqual("Therapy?", tags.artistString);
        }

        [Test]
        public void ArtistStringSetterShouldSplitCommaSeperatedString()
        {
            Tags tags = new Tags();
            tags.artistString = "Therapy?, Machine Head";
            Assert.AreEqual(2, tags.Artist.Length);
            Assert.AreEqual("Therapy?", tags.Artist[0]);
            Assert.AreEqual("Machine Head", tags.Artist[1]);
        }

        [Test]
        public void ArtistStringSetterShouldRemoveDuplicates()
        {
            Tags tags = new Tags();
            tags.artistString = "Therapy?, Therapy?";
            Assert.AreEqual(tags.Artist.Length, 1);
            Assert.AreEqual("Therapy?", tags.Artist[0]);
        }
    }
}
