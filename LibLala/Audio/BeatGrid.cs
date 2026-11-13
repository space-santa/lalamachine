using System;

namespace LibLala.Audio
{
    // Where the beats of a track are, assuming they come at a steady tempo.
    public sealed record BeatGrid
    {
        // The most that the grid may be off the music in nine of ten stretches of 16 beats, for beats to be matched by it.
        public const double SteadyDeviationMs = 8;
        public const int SteadyBeatCount = 64;

        public BeatGrid(double beatsPerMinute, double firstBeat, double deviationMs, int beatCount, double firstDownbeat, double musicStart, double musicEnd)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(beatsPerMinute);
            BeatsPerMinute = beatsPerMinute;
            FirstBeat = firstBeat;
            DeviationMs = deviationMs;
            BeatCount = beatCount;
            FirstDownbeat = firstDownbeat;
            MusicStart = musicStart;
            MusicEnd = musicEnd;
        }

        public double BeatsPerMinute { get; }

        // In seconds, of the first beat at or after the start. The grid goes on through intros and breaks without beats.
        public double FirstBeat { get; }

        // How far off the music the grid is in nine of ten stretches of 16 beats. Small for music made to a click, large for
        // a band that speeds up and slows down.
        public double DeviationMs { get; }

        // The beats that were heard on the grid.
        public int BeatCount { get; }

        // In seconds, the first beat of the first bar: one of the first four beats.
        public double FirstDownbeat { get; }

        // In seconds, the first and the last beat of the music, where the beats are heard most of the time. An intro or
        // outro without beats is before or after.
        public double MusicStart { get; }
        public double MusicEnd { get; }

        public double BeatSeconds => 60 / BeatsPerMinute;
        public double BarSeconds => 4 * BeatSeconds;

        // The beats of the track can be matched to another's by this grid.
        public bool IsSteady => BeatCount >= SteadyBeatCount && DeviationMs <= SteadyDeviationMs;

        // The tempo to show for a track whose tags have none, as a whole number like the tags have it. 0 for none: only a
        // steady grid has a tempo to rely on. In a DJ's library a steady grid found without the tag has the tempo that Mixed
        // In Key tagged for 95% of the tracks, any grid for 72%, and a third of the classical ones.
        public uint ShownBeatsPerMinute => ShownBeatsPerMinuteOf(BeatsPerMinute, DeviationMs, BeatCount);

        // The same for a grid as it is stored.
        public static uint ShownBeatsPerMinuteOf(double beatsPerMinute, double deviationMs, int beatCount)
        {
            return beatsPerMinute > 0 && beatCount >= SteadyBeatCount && deviationMs <= SteadyDeviationMs ? (uint)Math.Round(beatsPerMinute) : 0;
        }
    }
}
