using System;
using System.Globalization;

namespace LibLala.Audio
{
    // The key of a track: its tonic as a pitch class, from C = 0 to B = 11, in major or minor.
    public readonly record struct MusicalKey
    {
        public MusicalKey(int tonic, bool isMinor)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(tonic);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(tonic, 11);
            Tonic = tonic;
            IsMinor = isMinor;
        }

        public int Tonic { get; }
        public bool IsMinor { get; }

        // On the Camelot wheel, as Mixed In Key writes it: 8B is C major and 8A its relative minor, A minor. A fifth up is
        // one number up, and 1 comes after 12.
        public string Camelot
        {
            get
            {
                var relativeMajor = IsMinor ? (Tonic + 3) % 12 : Tonic;
                var number = (((relativeMajor * 7) + 7) % 12) + 1;
                return number.ToString(CultureInfo.InvariantCulture) + (IsMinor ? "A" : "B");
            }
        }

        public override string ToString() => Camelot;
    }
}
