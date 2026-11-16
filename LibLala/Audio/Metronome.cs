using System;

namespace LibLala.Audio
{
    // A click on every beat of a grid, to hear whether the grid is on the music's beat.
    internal static class Metronome
    {
        private const double ClickSeconds = 0.03;
        private const double DecaySeconds = 0.006;
        private const double Hz = 1500;
        private const double Loudness = 0.5;

        // Adds the clicks to the frames of the buffer, which start at the track's frame firstFrame and move on by tempo. On
        // the intro's grid before its end, like BeatGrid.At.
        public static void Add(Span<float> buffer, double firstFrame, double tempo, BeatGrid grid)
        {
            var music = Beats.Of(grid);
            var (intro, introEndFrame) = grid.Intro is { } introGrid ? (Beats.Of(introGrid), grid.IntroEnd * AudioFormat.SampleRate) : (music, double.NegativeInfinity);
            var frames = buffer.Length / AudioFormat.Channels;
            for (var i = 0; i < frames; i++)
            {
                var frame = firstFrame + (i * tempo);
                var (beatFrames, firstBeatFrame) = frame < introEndFrame ? intro : music;
                var sinceFirstBeat = frame - firstBeatFrame;
                var sinceBeat = (sinceFirstBeat - (Math.Floor(sinceFirstBeat / beatFrames) * beatFrames)) / AudioFormat.SampleRate;
                if (sinceBeat >= ClickSeconds)
                {
                    continue;
                }
                var click = (float)(Loudness * Math.Exp(-sinceBeat / DecaySeconds) * Math.Sin(2 * Math.PI * Hz * sinceBeat));
                buffer[i * AudioFormat.Channels] += click;
                buffer[(i * AudioFormat.Channels) + 1] += click;
            }
        }

        // A grid's beats in frames: how long each is, and where the first is.
        private readonly record struct Beats(double Frames, double FirstFrame)
        {
            public static Beats Of(BeatGrid grid) => new(grid.BeatSeconds * AudioFormat.SampleRate, grid.FirstBeat * AudioFormat.SampleRate);
        }
    }
}
