using System;
using System.Globalization;
using System.Linq;

namespace LibLala.DomainPrimitives
{
    public class InitialKey : BaseNameString
    {
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
    }
}
