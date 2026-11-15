using System;
using System.Collections.Generic;
using System.Threading;

namespace LibLala.Audio.Analysis
{
    // How loud a track is along its length, to draw it: the RMS of both channels in each slice of SliceFrames. Not the
    // peaks, they are near full scale all through a mastered track.
    public sealed class Waveform
    {
        // 50 ms. A track of 6 minutes has 7200 slices, a few for each bar even on a wide screen.
        public const int SliceFrames = AudioFormat.SampleRate / 20;
        private const int SlicesPerRead = 8;

        private readonly float[] _levels;

        private Waveform(float[] levels)
        {
            _levels = levels;
        }

        public static double SliceSeconds => (double)SliceFrames / AudioFormat.SampleRate;

        public int SliceCount => _levels.Length;

        // The whole track, till the decoder has no more: its length may be an estimate.
        public static Waveform Read(IAudioDecoder decoder, CancellationToken token)
        {
            ArgumentNullException.ThrowIfNull(decoder);

            var capacity = (int)Math.Min(int.MaxValue, (decoder.Length / SliceFrames) + 1);
            var levels = new List<float>(capacity);
            var block = new float[SliceFrames * SlicesPerRead * AudioFormat.Channels];
            double squares = 0;
            var frames = 0;
            int read;
            while ((read = decoder.Read(block)) > 0)
            {
                token.ThrowIfCancellationRequested();
                for (var i = 0; i < read; i++)
                {
                    var left = block[i * AudioFormat.Channels];
                    var right = block[(i * AudioFormat.Channels) + 1];
                    squares += ((double)left * left) + ((double)right * right);
                    if (++frames == SliceFrames)
                    {
                        levels.Add(Rms(squares, frames));
                        squares = 0;
                        frames = 0;
                    }
                }
            }
            if (frames > 0)
            {
                levels.Add(Rms(squares, frames));
            }
            return new Waveform([.. levels]);
        }

        // The first seconds of the track drawn as count bars side by side, each as long as the others: the RMS of each, as
        // a share of the loudest bar's. Bars past its end are 0, and all are 0 in a silent track.
        public float[] Bars(int count, double seconds)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(count);

            var bars = new float[count];
            if (seconds <= 0)
            {
                return bars;
            }
            var slicesPerBar = seconds / SliceSeconds / count;
            float loudest = 0;
            for (var b = 0; b < count; b++)
            {
                var from = (int)Math.Min(SliceCount, Math.Floor(b * slicesPerBar));
                // At least one slice, when bars are shorter than slices.
                var to = (int)Math.Min(SliceCount, Math.Max(from + 1, Math.Floor((b + 1) * slicesPerBar)));
                double squares = 0;
                for (var s = from; s < to; s++)
                {
                    squares += (double)_levels[s] * _levels[s];
                }
                bars[b] = to > from ? (float)Math.Sqrt(squares / (to - from)) : 0;
                loudest = Math.Max(loudest, bars[b]);
            }
            if (loudest > 0)
            {
                for (var b = 0; b < count; b++)
                {
                    bars[b] /= loudest;
                }
            }
            return bars;
        }

        private static float Rms(double squares, int frames) => (float)Math.Sqrt(squares / (frames * AudioFormat.Channels));
    }
}
