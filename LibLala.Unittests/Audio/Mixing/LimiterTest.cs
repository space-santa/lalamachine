using System;
using System.Linq;
using LibLala.Audio;
using LibLala.Audio.Mixing;
using NUnit.Framework;

namespace LibLala.Unittests.Audio.Mixing
{
    public class LimiterTest
    {
        private const int Ahead = Limiter.LookAheadFrames;

        // Frames of the same level in both channels, with a peak at one of them.
        private static float[] Frames(int count, float level, int peakAt = -1, float peak = 0)
        {
            var frames = new float[count * AudioFormat.Channels];
            for (var i = 0; i < count; i++)
            {
                frames[i * 2] = frames[(i * 2) + 1] = i == peakAt ? peak : level;
            }
            return frames;
        }

        private static float[] Run(Limiter limiter, float[] input, bool limit = true)
        {
            limiter.Prime(input.AsSpan(0, Ahead * AudioFormat.Channels));
            var output = new float[input.Length - (Ahead * AudioFormat.Channels)];
            limiter.Process(input.AsSpan(Ahead * AudioFormat.Channels), output, limit);
            return output;
        }

        [Test]
        public void UnderTheCeiling_ItOnlyDelays()
        {
            var input = Enumerable.Range(0, 1000 * AudioFormat.Channels).Select(i => (float)Math.Sin(i * 0.01) * 0.9f).ToArray();

            var output = Run(new Limiter(), input);

            Assert.That(output, Is.EqualTo(input.Take(output.Length)));
        }

        [Test]
        public void APeak_ComesOutAtTheCeiling_TheGainGoingDownSmoothlyBefore()
        {
            var output = Run(new Limiter(), Frames(2000, 0.5f, peakAt: 1000, peak: 2));

            Assert.That(output.Max(Math.Abs), Is.LessThanOrEqualTo(Limiter.Ceiling + 1e-6));
            Assert.That(output[1000 * AudioFormat.Channels], Is.EqualTo(Limiter.Ceiling).Within(1e-6));
            var gains = Enumerable.Range(0, 2000 - Ahead).Where(i => i != 1000).Select(i => output[i * 2] / 0.5).ToArray();
            var steps = gains.Zip(gains.Skip(1), (a, b) => Math.Abs(b - a));
            Assert.That(steps.Max(), Is.LessThan(0.01));
        }

        [Test]
        public void NotLimiting_APeakGoesThrough()
        {
            var output = Run(new Limiter(), Frames(2000, 0.5f, peakAt: 1000, peak: 2), limit: false);

            Assert.That(output[1000 * AudioFormat.Channels], Is.EqualTo(2));
        }

        [Test]
        public void AfterAPeak_ItLetsGo_AndWhatItHoldsComesOutAsItIs()
        {
            var limiter = new Limiter();
            Run(limiter, Frames(1000, 0.5f, peakAt: 500, peak: 2));
            Assert.That(limiter.IsIdle, Is.False);

            limiter.Process(Frames(AudioFormat.SampleRate, 0.25f), new float[AudioFormat.SampleRate * AudioFormat.Channels], limit: false);
            Assert.That(limiter.IsIdle, Is.True);
            var held = new float[(Ahead + 10) * AudioFormat.Channels];

            Assert.That(limiter.Drain(held), Is.EqualTo(Ahead));
            Assert.That(held.Take(Ahead * AudioFormat.Channels), Is.All.EqualTo(0.25f));
            Assert.That(limiter.Held, Is.Zero);
        }
    }
}
