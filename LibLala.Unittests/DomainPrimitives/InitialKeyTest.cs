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
    }
}
