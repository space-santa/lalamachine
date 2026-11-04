using LibLala.DomainPrimitives;
using NUnit.Framework;

namespace LibLala.Unittests.DomainPrimitives
{
    public class InitialKeyTest
    {
        [TestCase("4A", 4, "A")]
        [TestCase("10B", 10, "B")]
        [TestCase(" 11B ", 11, "B")]
        public void CamelotKeyIsSplitIntoNumberAndLetter(string name, int number, string letter)
        {
            var key = new InitialKey(name);

            Assert.That(key.Number, Is.EqualTo(number));
            Assert.That(key.Letter, Is.EqualTo(letter));
        }

        [TestCase("Am")]
        [TestCase("F#m")]
        public void KeyWithoutNumberSortsAfterTheNumberedOnes(string name)
        {
            var key = new InitialKey(name);

            Assert.That(key.Number, Is.EqualTo(int.MaxValue));
            Assert.That(key.Letter, Is.EqualTo(name));
        }

        [Test]
        public void NumberTooLongForAnIntDoesNotThrow()
        {
            Assert.That(new InitialKey("99999999999A").Number, Is.EqualTo(int.MaxValue));
        }

        [Test]
        public void EmptyKeyIsEmpty()
        {
            var key = new InitialKey("");

            Assert.That(key.ToString(), Is.Empty);
            Assert.That(key.Letter, Is.Empty);
        }

        [TestCase("1A", "G♯/A♭ minor")]
        [TestCase("2A", "D♯/E♭ minor")]
        [TestCase("3A", "A♯/B♭ minor")]
        [TestCase("4A", "F minor")]
        [TestCase("5A", "C minor")]
        [TestCase("6A", "G minor")]
        [TestCase("7A", "D minor")]
        [TestCase("8A", "A minor")]
        [TestCase("9A", "E minor")]
        [TestCase("10A", "B minor")]
        [TestCase("11A", "F♯/G♭ minor")]
        [TestCase("12A", "C♯/D♭ minor")]
        [TestCase("1B", "B major")]
        [TestCase("2B", "F♯/G♭ major")]
        [TestCase("3B", "C♯/D♭ major")]
        [TestCase("4B", "G♯/A♭ major")]
        [TestCase("5B", "D♯/E♭ major")]
        [TestCase("6B", "A♯/B♭ major")]
        [TestCase("7B", "F major")]
        [TestCase("8B", "C major")]
        [TestCase("9B", "G major")]
        [TestCase("10B", "D major")]
        [TestCase("11B", "A major")]
        [TestCase("12B", "E major")]
        [TestCase(" 11B ", "A major")]
        [TestCase("8a", "A minor")]
        [TestCase("8b", "C major")]
        [TestCase("8 A", "A minor")]
        public void CamelotKeyIsTranslatedToNaturalNotation(string name, string natural)
        {
            Assert.That(new InitialKey(name).NaturalNotation, Is.EqualTo(natural));
        }

        [TestCase("")]
        [TestCase("Am")]
        [TestCase("F#m")]
        [TestCase("0A")]
        [TestCase("13A")]
        [TestCase("4C")]
        [TestCase("4c")]
        [TestCase("4 C")]
        [TestCase("4")]
        [TestCase("99999999999A")]
        public void KeyThatIsNotCamelotHasNoNaturalNotation(string name)
        {
            Assert.That(new InitialKey(name).NaturalNotation, Is.Null);
        }
    }
}
