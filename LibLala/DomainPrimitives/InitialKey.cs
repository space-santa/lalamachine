using System;
using System.Globalization;
using System.Linq;

namespace LibLala.DomainPrimitives
{
    public class InitialKey : BaseNameString
    {
        // The Camelot wheel from 1 to 12. The same number is a minor key (A) and its relative major (B).
        // A key on a black note has both of its names, Beatport for one writes D♭ minor where others write C♯ minor.
        private static readonly string[] CamelotMinorKeys = ["G♯/A♭ minor", "D♯/E♭ minor", "A♯/B♭ minor", "F minor", "C minor", "G minor", "D minor", "A minor", "E minor", "B minor", "F♯/G♭ minor", "C♯/D♭ minor"];
        private static readonly string[] CamelotMajorKeys = ["B major", "F♯/G♭ major", "C♯/D♭ major", "G♯/A♭ major", "D♯/E♭ major", "A♯/B♭ major", "F major", "C major", "G major", "D major", "A major", "E major"];

        public InitialKey(string name) : base(name)
        {
            var key = name.Trim();
            var digits = key.TakeWhile(char.IsAsciiDigit).Count();
            // Keys that don't start with a number (e.g. Am) sort after the ones that do.
            Number = int.TryParse(key.AsSpan(0, digits), NumberStyles.None, Constants.CULTURE, out var number) ? number : int.MaxValue;
            Letter = key[digits..];
        }

        // Camelot keys are split so that they can sort by the number first, otherwise 10A comes before 2A.
        public int Number { get; }
        public string Letter { get; }

        // Null if the key isn't in Camelot notation, there's nothing to translate then.
        public string? NaturalNotation => (Number, Letter.Trim().ToUpperInvariant()) switch
        {
            (>= 1 and <= 12, "A") => CamelotMinorKeys[Number - 1],
            (>= 1 and <= 12, "B") => CamelotMajorKeys[Number - 1],
            _ => null,
        };
    }
}
