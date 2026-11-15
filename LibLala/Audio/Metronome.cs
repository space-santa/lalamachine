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
        // the intro's grid up to the bar after its music.
        public static void Add(Span<float> buffer, double firstFrame, double tempo, BeatGrid grid)
        {
            var introEndFrame = grid.Intro is { } intro ? (intro.FirstDownbeat + (intro.MusicEndBar * intro.BarSeconds)) * AudioFormat.SampleRate : double.NegativeInfinity;
            var frames = buffer.Length / AudioFormat.Channels;
            for (var i = 0; i < frames; i++)
            {
                var frame = firstFrame + (i * tempo);
                var clicked = frame < introEndFrame ? grid.Intro! : grid;
                var beatFrames = clicked.BeatSeconds * AudioFormat.SampleRate;
                var sinceFirstBeat = frame - (clicked.FirstBeat * AudioFormat.SampleRate);
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
    }
}
