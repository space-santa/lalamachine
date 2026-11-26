using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using LibLala.Audio;

namespace LibLala.DomainPrimitives
{
    // A key as lalamachine shows it: on the Camelot wheel, whatever notation the tags have it in. A key that can't be read
    // as one stays as it is.
    public partial class InitialKey : BaseNameString
    {
        // The Camelot wheel from 1 to 12. The same number is a minor key (A) and its relative major (B).
        // A key on a black note has both of its names, Beatport for one writes D♭ minor where others write C♯ minor.
        private static readonly string[] CamelotMinorKeys = ["G♯/A♭ minor", "D♯/E♭ minor", "A♯/B♭ minor", "F minor", "C minor", "G minor", "D minor", "A minor", "E minor", "B minor", "F♯/G♭ minor", "C♯/D♭ minor"];
        private static readonly string[] CamelotMajorKeys = ["B major", "F♯/G♭ major", "C♯/D♭ major", "G♯/A♭ major", "D♯/E♭ major", "A♯/B♭ major", "F major", "C major", "G major", "D major", "A major", "E major"];

        // C, D, E, F, G, A and B as pitch classes.
        private static readonly int[] NaturalTonics = [9, 11, 0, 2, 4, 5, 7];

        public InitialKey(string name) : base(ToCamelot(name))
        {
            var key = ToString();
            var digits = key.TakeWhile(char.IsAsciiDigit).Count();
            // Keys that don't start with a number sort after the ones that do.
            Number = int.TryParse(key.AsSpan(0, digits), NumberStyles.None, Constants.CULTURE, out var number) ? number : int.MaxValue;
            Letter = key[digits..];
        }

        // Camelot keys are split so that they can sort by the number first, otherwise 10A comes before 2A.
        public int Number { get; }
        public string Letter { get; }

        // Null if the key isn't in Camelot notation, there's nothing to translate then.
        public string? NaturalNotation => (Number, Letter) switch
        {
            (>= 1 and <= 12, "A") => CamelotMinorKeys[Number - 1],
            (>= 1 and <= 12, "B") => CamelotMajorKeys[Number - 1],
            _ => null,
        };

        // False for none, and for one that couldn't be read as a key.
        public bool IsCamelot => NaturalNotation is not null;

        // As Mixed In Key writes it, e.g. 8A. From Camelot written another way (8a, 8 A), Open Key (1m), and the key's name
        // (Am, F#m, Dbmaj, A minor, and F♯/G♭ minor as lalamachine shows it). Anything else as it is.
        public static string ToCamelot(string name)
        {
            ArgumentNullException.ThrowIfNull(name);

            var key = name.Trim();
            if (WheelPattern().Match(key) is { Success: true } wheel)
            {
                var number = int.Parse(wheel.Groups["number"].Value, NumberStyles.None, CultureInfo.InvariantCulture);
                var letter = char.ToUpperInvariant(wheel.Groups["letter"].Value[0]);
                return letter switch
                {
                    _ when number is < 1 or > 12 => key,
                    'A' => Camelot(number, isMinor: true),
                    'B' => Camelot(number, isMinor: false),
                    // Open Key: 1d is C major, 8B on the Camelot wheel. Its numbers are 7 behind.
                    'D' => Camelot(((number + 6) % 12) + 1, isMinor: false),
                    _ => Camelot(((number + 6) % 12) + 1, isMinor: true),
                };
            }

            if (NamePattern().Match(key) is { Success: true } named)
            {
                var tonic = NaturalTonics[char.ToUpperInvariant(named.Groups["note"].Value[0]) - 'A'] + Shift(named.Groups["accidental"].Value);
                var mode = named.Groups["mode"].Value;
                var isMinor = mode == "m" || mode.StartsWith("min", StringComparison.OrdinalIgnoreCase);
                return new MusicalKey((tonic + 12) % 12, isMinor).Camelot;
            }

            return key;
        }

        private static string Camelot(int number, bool isMinor) => number.ToString(CultureInfo.InvariantCulture) + (isMinor ? "A" : "B");

        private static int Shift(string accidental) => accidental.ToUpperInvariant() switch
        {
            "#" or "♯" or "SHARP" => 1,
            "B" or "♭" or "FLAT" => -1,
            _ => 0,
        };

        // A number and a letter, either of Camelot (A, B) or of Open Key (m, d).
        [GeneratedRegex(@"^(?<number>\d{1,2})\s*(?<letter>[ABab]|[mMdD])$")]
        private static partial Regex WheelPattern();

        // The note, maybe sharp or flat, maybe followed by its other name (as in F♯/G♭), and maybe the mode. A lone m is
        // minor, the words in any case. Major without a mode, as the ID3 standard has it.
        [GeneratedRegex(@"^(?<note>[A-Ga-g])\s*(?<accidental>#|♯|b|♭|(?i:sharp|flat))?(?:\s*/\s*[A-Ga-g]\s*(?:#|♯|b|♭))?\s*(?<mode>m|(?i:minor|min|major|maj))?$")]
        private static partial Regex NamePattern();
    }
}
