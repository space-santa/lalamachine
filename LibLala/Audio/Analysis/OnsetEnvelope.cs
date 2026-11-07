using System;
using System.Collections.Generic;

namespace LibLala.Audio.Analysis
{
    // How much new sound starts, every 11.6 ms of a track: the spectral flux of all of it, of the bass, and of the click
    // range. A kick drum is in both of these at once, a bass line only in the bass and a hi-hat only above.
    internal sealed record Onsets(float[] All, float[] Bass, float[] Click);

    internal static class OnsetEnvelope
    {
        // Half of AudioFormat's, plenty for drums.
        public const int SampleRate = AudioFormat.SampleRate / 2;
        public const int Hop = 256;
        private const int Window = 1024;
        private const double BassHz = 150;
        private const double ClickFromHz = 1000;
        private const double ClickToHz = 5000;
        private const double TopHz = 11000;
        private const int BlockSamples = 8192;

        public static double HopSeconds => (double)Hop / SampleRate;

        public static Onsets Compute(IAudioDecoder decoder)
        {
            var capacity = (int)(decoder.Length / 2 / Hop) + 1;
            var allEnvelope = new List<float>(capacity);
            var bassEnvelope = new List<float>(capacity);
            var clickEnvelope = new List<float>(capacity);
            var fft = new RealFft(Window);
            var hann = new float[Window];
            for (var i = 0; i < Window; i++)
            {
                hann[i] = (float)(0.5 - (0.5 * Math.Cos(2 * Math.PI * i / Window)));
            }
            var bassBins = (int)(BassHz * Window / SampleRate) + 1;
            var topBins = (int)(TopHz * Window / SampleRate);
            var clickFrom = (int)(ClickFromHz * Window / SampleRate);
            var clickTo = (int)(ClickToHz * Window / SampleRate);

            var frame = new float[Window];
            var magnitudes = new float[Window / 2];
            var previous = new float[Window / 2];
            var current = new float[Window / 2];
            // Mono samples not yet in a frame. What's left after the frames is less than a window.
            var samples = new float[Window + BlockSamples];
            var count = 0;
            var block = new float[BlockSamples * 2 * AudioFormat.Channels];
            int read;
            while ((read = decoder.Read(block)) > 0)
            {
                // Two stereo frames to one mono sample.
                for (var i = 0; i + 1 < read; i += 2)
                {
                    var s = 2 * i;
                    samples[count++] = 0.25f * (block[s] + block[s + 1] + block[s + 2] + block[s + 3]);
                }

                var start = 0;
                for (; start + Window <= count; start += Hop)
                {
                    for (var i = 0; i < Window; i++)
                    {
                        frame[i] = samples[start + i] * hann[i];
                    }
                    fft.Magnitudes(frame, magnitudes);

                    float all = 0;
                    float bass = 0;
                    float click = 0;
                    for (var k = 1; k < topBins; k++)
                    {
                        var level = MathF.Log(1 + (1000 * magnitudes[k] / (Window / 4f)));
                        current[k] = level;
                        var rise = level - previous[k];
                        if (rise > 0)
                        {
                            all += rise;
                            if (k < bassBins)
                            {
                                bass += rise;
                            }
                            else if (k >= clickFrom && k < clickTo)
                            {
                                click += rise;
                            }
                        }
                    }
                    allEnvelope.Add(all);
                    bassEnvelope.Add(bass);
                    clickEnvelope.Add(click);
                    (previous, current) = (current, previous);
                }

                Array.Copy(samples, start, samples, 0, count - start);
                count -= start;
            }

            return new Onsets([.. allEnvelope], [.. bassEnvelope], [.. clickEnvelope]);
        }
    }
}
