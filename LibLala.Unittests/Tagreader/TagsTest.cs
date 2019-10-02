using System;
using System.Collections.Generic;
using LibLala.DomainPrimitives;
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
            var tags = new LibLalaTagsBuilder("bob", new FakeTrackPath("/path/to/bob"), (int)ts.TotalSeconds).Build();
            Assert.AreEqual("124:03", tags.LengthString);
            Assert.AreEqual(124 * 60 + 3, tags.Length);
        }

        [Test]
        public void DurationWithTwoDigitSecondsShouldBeCorrectLength()
        {
            var ts = new TimeSpan(0, 2, 4, 34, 15);
            var tags = new LibLalaTagsBuilder("bob", new FakeTrackPath("/path/to/bob"), (int)ts.TotalSeconds).Build();
            Assert.AreEqual("124:34", tags.LengthString);
            Assert.AreEqual(124 * 60 + 34, tags.Length);
        }

        [Test]
        public void ArtistArrayDuplicatesShouldBeRemoved()
        {
            var tags = new LibLalaTagsBuilder(title: "bob", fakePath: new FakeTrackPath("/path/to/bob"), totalSeconds: 5).WithArtist(new List<string> { "Therapy?", "Therapy?" }).Build();
            Assert.AreEqual("Therapy?", tags.ArtistString);
        }
    }
}
