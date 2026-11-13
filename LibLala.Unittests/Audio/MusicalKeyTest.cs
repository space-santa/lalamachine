using System;
using System.Linq;
using LibLala.Audio;
using NUnit.Framework;

namespace LibLala.Unittests.Audio
{
    public sealed class MusicalKeyTest
    {
        // The whole wheel: the tonic counted in semitones from C.
        [TestCase(11, false, "1B")]
        [TestCase(6, false, "2B")]
        [TestCase(1, false, "3B")]
        [TestCase(8, false, "4B")]
        [TestCase(3, false, "5B")]
        [TestCase(10, false, "6B")]
        [TestCase(5, false, "7B")]
        [TestCase(0, false, "8B")]
        [TestCase(7, false, "9B")]
        [TestCase(2, false, "10B")]
        [TestCase(9, false, "11B")]
        [TestCase(4, false, "12B")]
        [TestCase(8, true, "1A")]
        [TestCase(3, true, "2A")]
        [TestCase(10, true, "3A")]
        [TestCase(5, true, "4A")]
        [TestCase(0, true, "5A")]
        [TestCase(7, true, "6A")]
        [TestCase(2, true, "7A")]
        [TestCase(9, true, "8A")]
        [TestCase(4, true, "9A")]
        [TestCase(11, true, "10A")]
        [TestCase(6, true, "11A")]
        [TestCase(1, true, "12A")]
        public void AKey_IsWhereMixedInKeyHasIt_OnTheCamelotWheel(int tonic, bool isMinor, string camelot)
        {
            var key = new MusicalKey(tonic, isMinor);

            Assert.That(key.Camelot, Is.EqualTo(camelot));
            Assert.That(key.ToString(), Is.EqualTo(camelot));
        }

        [Test]
        public void AFifthUp_IsOneNumberUp_AndTheRelativeMinorHasTheSameNumber()
        {
            var cMajor = new MusicalKey(0, isMinor: false);
            var gMajor = new MusicalKey(7, isMinor: false);
            var aMinor = new MusicalKey(9, isMinor: true);

            Assert.That((cMajor.Camelot, gMajor.Camelot, aMinor.Camelot), Is.EqualTo(("8B", "9B", "8A")));
        }

        [Test]
        public void AllKeys_AreDifferentOnTheWheel()
        {
            var keys = Enumerable.Range(0, 12).SelectMany(tonic => new[] { new MusicalKey(tonic, false), new MusicalKey(tonic, true) });

            Assert.That(keys.Select(key => key.Camelot).Distinct().Count(), Is.EqualTo(24));
        }

        [TestCase(-1)]
        [TestCase(12)]
        public void ATonicThatIsNoPitchClass_IsRefused(int tonic)
        {
            Assert.That(() => new MusicalKey(tonic, isMinor: false), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }
    }
}
