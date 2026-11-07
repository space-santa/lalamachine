using System;

namespace LibLala.Audio
{
    // Where the beats of a track are, assuming they come at a steady tempo.
    public sealed record BeatGrid
    {
        // The most that the grid may be off the music in nine of ten stretches of 16 beats, for beats to be matched by it.
        public const double SteadyDeviationMs = 8;
        public const int SteadyBeatCount = 64;

        public BeatGrid(double beatsPerMinute, double firstBeat, double deviationMs, int beatCount)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(beatsPerMinute);
            BeatsPerMinute = beatsPerMinute;
            FirstBeat = firstBeat;
            DeviationMs = deviationMs;
            BeatCount = beatCount;
        }

        public double BeatsPerMinute { get; }

        // In seconds, of the first beat at or after the start. The grid goes on through intros and breaks without beats.
        public double FirstBeat { get; }

        // How far off the music the grid is in nine of ten stretches of 16 beats. Small for music made to a click, large for
        // a band that speeds up and slows down.
        public double DeviationMs { get; }

        // The beats that were heard on the grid.
        public int BeatCount { get; }

        public double BeatSeconds => 60 / BeatsPerMinute;

        // The beats of the track can be matched to another's by this grid.
        public bool IsSteady => BeatCount >= SteadyBeatCount && DeviationMs <= SteadyDeviationMs;
    }
}
