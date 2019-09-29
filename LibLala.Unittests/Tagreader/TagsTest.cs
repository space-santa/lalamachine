using System;
using System.Collections.Generic;
using LibLala.LibLalaTagReader;
using NUnit.Framework;

namespace LibLala.Unittests.Tagreader
{
    public class TagsTest
    {
        [Test]
        public void DurationWithHoursShouldBeCorrectLength()
        {
            var ts = new TimeSpan(0, 2, 4, 3, 999);
            var tags = new LibLalaTags("bob", "/path/tp/bob")
            {
                Duration = ts
            };
            Assert.AreEqual("124:03", tags.LengthString);
            Assert.AreEqual(124 * 60 + 3, tags.Length);
        }

        [Test]
        public void DurationWithTwoDigitSecondsShouldBeCorrectLength()
        {
            var ts = new TimeSpan(0, 2, 4, 34, 15);
            var tags = new LibLalaTags("bob", "/path/tp/bob")
            {
                Duration = ts
            };
            Assert.AreEqual("124:34", tags.LengthString);
            Assert.AreEqual(124 * 60 + 34, tags.Length);
        }

        [Test]
        public void ArtistArrayDuplicatesShouldBeRemoved()
        {
            var tags = new LibLalaTags(title: "bob", path: "/path/tp/bob", genre: new List<string>(), artist: new List<string> { "Therapy?", "Therapy?" }, "");
            Assert.AreEqual("Therapy?", tags.ArtistString);
        }

        [Test]
        public void ArtistStringSetterShouldSplitCommaSeperatedString()
        {
            var tags = new LibLalaTags("bob", "/path/tp/bob")
            {
                ArtistString = "Therapy?, Machine Head"
            };
            Assert.AreEqual(2, tags.Artist.Count);
            Assert.AreEqual("Therapy?", tags.Artist[0]);
            Assert.AreEqual("Machine Head", tags.Artist[1]);
        }

        [Test]
        public void ArtistStringSetterShouldRemoveDuplicates()
        {
            var tags = new LibLalaTags("bob", "/path/tp/bob")
            {
                ArtistString = "Therapy?, Therapy?"
            };
            Assert.AreEqual(tags.Artist.Count, 1);
            Assert.AreEqual("Therapy?", tags.Artist[0]);
        }
    }
}
