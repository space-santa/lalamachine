using System;
using LibLala.Audio;
using LibLala.Audio.Mixing;
using NUnit.Framework;

namespace LibLala.Unittests.Audio.Mixing
{
    public class FilterTest
    {
        // How loud a sine comes out, in dB of how loud it went in, after the filter settled.
        private static double Through(Filter filter, double hz)
        {
            var frames = AudioFormat.SampleRate / 2;
            var buffer = new float[frames * AudioFormat.Channels];
            for (var i = 0; i < frames; i++)
            {
                buffer[i * AudioFormat.Channels] = buffer[(i * AudioFormat.Channels) + 1] = (float)Math.Sin(2 * Math.PI * hz * i / AudioFormat.SampleRate);
            }
            filter.Process(buffer);
            double peak = 0;
            for (var i = frames / 2 * AudioFormat.Channels; i < buffer.Length; i++)
            {
                peak = Math.Max(peak, Math.Abs(buffer[i]));
            }
            return 20 * Math.Log10(peak);
        }

        [Test]
        public void ALowPass_LetsTheBassThrough_AndTakesTheHighsOut()
        {
            Assert.That(Through(new Filter(FilterKind.LowPass) { Cutoff = 500 }, 100), Is.EqualTo(0).Within(0.5));
            Assert.That(Through(new Filter(FilterKind.LowPass) { Cutoff = 500 }, 4000), Is.LessThan(-60));
        }

        [Test]
        public void AHighPass_LetsTheHighsThrough_AndTakesTheBassOut()
        {
            Assert.That(Through(new Filter(FilterKind.HighPass) { Cutoff = 1000 }, 8000), Is.EqualTo(0).Within(0.5));
            Assert.That(Through(new Filter(FilterKind.HighPass) { Cutoff = 1000 }, 125), Is.LessThan(-60));
        }

        [Test]
        public void AtTheCutoff_ItIsThreeDecibelsDown()
        {
            Assert.That(Through(new Filter(FilterKind.LowPass) { Cutoff = 1000 }, 1000), Is.EqualTo(-3).Within(0.3));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Off_LeavesEverythingAsItIs(bool isLowPass)
        {
            var filter = new Filter(isLowPass ? FilterKind.LowPass : FilterKind.HighPass);
            filter.Cutoff = filter.Off;
            float[] buffer = [0.1f, -0.2f, 0.3f, 0.4f];

            filter.Process(buffer);

            Assert.That(filter.IsOff, Is.True);
            Assert.That(buffer, Is.EqualTo(new[] { 0.1f, -0.2f, 0.3f, 0.4f }));
        }
    }
}
