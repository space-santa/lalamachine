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

        [TestCase("o")]
        [TestCase("Unknown")]
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
        [TestCase("o")]
        [TestCase("H")]
        [TestCase("AM")]
        [TestCase("0A")]
        [TestCase("13A")]
        [TestCase("4C")]
        [TestCase("4c")]
        [TestCase("4 C")]
        [TestCase("4")]
        [TestCase("99999999999A")]
        public void KeyThatIsNotCamelotHasNoNaturalNotation(string name)
        {
            var key = new InitialKey(name);

            Assert.That(key.NaturalNotation, Is.Null);
            Assert.That(key.IsCamelot, Is.False);
        }

        // Camelot written another way.
        [TestCase("8A", "8A")]
        [TestCase("8a", "8A")]
        [TestCase("8 b", "8B")]
        [TestCase(" 12B ", "12B")]
        // Open Key, where 1d is C major.
        [TestCase("1d", "8B")]
        [TestCase("1m", "8A")]
        [TestCase("6d", "1B")]
        [TestCase("12m", "7A")]
        [TestCase("5M", "12A")]
        // The key's name, as the ID3 standard and most DJ software write it.
        [TestCase("Am", "8A")]
        [TestCase("C", "8B")]
        [TestCase("F#m", "11A")]
        [TestCase("Gbm", "11A")]
        [TestCase("Dbm", "12A")]
        [TestCase("C#m", "12A")]
        [TestCase("Bb", "6B")]
        [TestCase("Bbm", "3A")]
        [TestCase("Bm", "10A")]
        [TestCase("Ebm", "2A")]
        [TestCase("Abm", "1A")]
        [TestCase("Cb", "1B")]
        [TestCase("E#m", "4A")]
        [TestCase("am", "8A")]
        [TestCase("Amin", "8A")]
        [TestCase("Cmaj", "8B")]
        [TestCase("A minor", "8A")]
        [TestCase("A Minor", "8A")]
        [TestCase("D major", "10B")]
        [TestCase("F♯m", "11A")]
        [TestCase("B♭", "6B")]
        [TestCase("F sharp minor", "11A")]
        [TestCase("E flat", "5B")]
        // As lalamachine names it, both names of a key on a black note.
        [TestCase("F♯/G♭ minor", "11A")]
        [TestCase("C#/Db", "3B")]
        public void KeyIsShownOnTheCamelotWheel(string name, string camelot)
        {
            var key = new InitialKey(name);

            Assert.That(key.ToString(), Is.EqualTo(camelot));
            Assert.That(key.IsCamelot, Is.True);
        }

        [TestCase("")]
        [TestCase("o")]
        [TestCase("AM")]
        [TestCase("H")]
        [TestCase("13A")]
        [TestCase("0m")]
        [TestCase("4C")]
        [TestCase("Am7")]
        [TestCase("not a key")]
        // Digits that aren't ASCII ones, a fullwidth and an Arabic-Indic 8.
        [TestCase("８A")]
        [TestCase("٨A")]
        [TestCase("８m")]
        public void KeyThatCantBeRead_StaysAsItIs(string name)
        {
            Assert.That(new InitialKey(name).ToString(), Is.EqualTo(name));
        }

        // Every name of every key gets back to where it is on the wheel.
        [Test]
        public void NaturalNotation_IsReadBackAsTheSameKey()
        {
            for (var number = 1; number <= 12; number++)
            {
                foreach (var letter in new[] { "A", "B" })
                {
                    var camelot = $"{number}{letter}";
                    var natural = new InitialKey(camelot).NaturalNotation!;

                    Assert.That(new InitialKey(natural).ToString(), Is.EqualTo(camelot), natural);
                }
            }
        }
    }
}
