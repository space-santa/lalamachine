using System;
using System.Threading;
using LibLala.Audio;
using LibLala.Audio.Analysis;
using NUnit.Framework;

namespace LibLala.Unittests.Audio
{
    public class WaveformTest
    {
        [Test]
        public void TheBars_AreTheRmsOfBothChannels_AsSharesOfTheLoudest()
        {
            // √((0.6² + 0.8²) / 2) = √0.5, then 0.5.
            using var decoder = new LevelsDecoder((0.6f, 0.8f, Seconds(1)), (0.5f, -0.5f, Seconds(1)));

            var bars = Waveform.Read(decoder, CancellationToken.None).Bars(2, 2);

            Assert.That(bars, Is.EqualTo(new[] { 1, Math.Sqrt(0.5) }).Within(1e-6));
        }

        [Test]
        public void EachBar_IsTheRmsOfTheSlicesInIt()
        {
            // √((0.8² + 0.4²) / 2) for the first bar.
            using var decoder = new LevelsDecoder((0.8f, 0.8f, Seconds(1)), (0.4f, 0.4f, Seconds(1)), (0.2f, 0.2f, Seconds(2)));

            var bars = Waveform.Read(decoder, CancellationToken.None).Bars(2, 4);

            Assert.That(bars, Is.EqualTo(new[] { 1, 0.2 / Math.Sqrt(0.4) }).Within(1e-6));
        }

        [Test]
        public void BarsPastTheEndOfTheTrack_Are0()
        {
            using var decoder = new LevelsDecoder((0.5f, 0.5f, Seconds(2)));

            var bars = Waveform.Read(decoder, CancellationToken.None).Bars(4, 4);

            Assert.That(bars, Is.EqualTo(new[] { 1f, 1f, 0f, 0f }));
        }

        [Test]
        public void BarsShorterThanASlice_ShowTheSliceTheyStartIn()
        {
            // A slice is 50 ms, each bar 25.
            using var decoder = new LevelsDecoder((0.5f, 0.5f, Waveform.SliceFrames), (0.25f, 0.25f, Waveform.SliceFrames));

            var bars = Waveform.Read(decoder, CancellationToken.None).Bars(4, 2 * Waveform.SliceSeconds);

            Assert.That(bars, Is.EqualTo(new[] { 1f, 1f, 0.5f, 0.5f }));
        }

        [Test]
        public void TheLastSlice_IsWhatsLeftOfTheTrack()
        {
            using var decoder = new LevelsDecoder((0.5f, 0.5f, Waveform.SliceFrames), (0.25f, 0.25f, Waveform.SliceFrames / 10));

            var waveform = Waveform.Read(decoder, CancellationToken.None);

            Assert.That(waveform.SliceCount, Is.EqualTo(2));
            Assert.That(waveform.Bars(2, 2 * Waveform.SliceSeconds), Is.EqualTo(new[] { 1f, 0.5f }));
        }

        [Test]
        public void ASilentTrack_HasBarsOf0()
        {
            using var decoder = new LevelsDecoder((0, 0, Seconds(1)));

            var bars = Waveform.Read(decoder, CancellationToken.None).Bars(3, 1);

            Assert.That(bars, Is.EqualTo(new[] { 0f, 0f, 0f }));
        }

        [Test]
        public void NoBars_AndNoSeconds()
        {
            using var decoder = new LevelsDecoder((0.5f, 0.5f, Seconds(1)));
            var waveform = Waveform.Read(decoder, CancellationToken.None);

            Assert.That(waveform.Bars(0, 1), Is.Empty);
            Assert.That(waveform.Bars(2, 0), Is.EqualTo(new[] { 0f, 0f }));
        }

        [Test]
        public void ItsLength_IsWhatTheDecoderReads_NotWhatItSaysItIs()
        {
            // Like a VBR mp3 without a header that says how long it is.
            using var decoder = new LevelsDecoder((0.5f, 0.5f, Seconds(2))) { SaysLength = Seconds(1) };

            var waveform = Waveform.Read(decoder, CancellationToken.None);

            Assert.That(waveform.SliceCount * Waveform.SliceSeconds, Is.EqualTo(2).Within(1e-9));
        }

        [Test]
        public void Stopped_ItThrows()
        {
            using var decoder = new LevelsDecoder((0.5f, 0.5f, Seconds(1)));
            using var stop = new CancellationTokenSource();
            stop.Cancel();

            Assert.That(() => Waveform.Read(decoder, stop.Token), Throws.InstanceOf<OperationCanceledException>());
        }

        private static long Seconds(double seconds) => (long)(seconds * AudioFormat.SampleRate);

        // Stretches of the same sample in each channel, one after the other.
        private sealed class LevelsDecoder : IAudioDecoder
        {
            private readonly (float Left, float Right, long Frames)[] _levels;
            private long _position;

            public LevelsDecoder(params (float Left, float Right, long Frames)[] levels)
            {
                _levels = levels;
                foreach (var level in levels)
                {
                    Length += level.Frames;
                }
            }

            public long Length { get; }
            // What it says its length is, when that's not what it reads.
            public long? SaysLength { get; init; }
            long IAudioDecoder.Length => SaysLength ?? Length;

            public int Read(Span<float> buffer)
            {
                var frames = (int)Math.Min(buffer.Length / AudioFormat.Channels, Length - _position);
                for (var i = 0; i < frames; i++)
                {
                    var (left, right) = LevelAt(_position + i);
                    buffer[i * AudioFormat.Channels] = left;
                    buffer[(i * AudioFormat.Channels) + 1] = right;
                }
                _position += frames;
                return frames;
            }

            public void Seek(long frame) => _position = frame;

            public void Dispose() { }

            private (float Left, float Right) LevelAt(long frame)
            {
                foreach (var level in _levels)
                {
                    if (frame < level.Frames)
                    {
                        return (level.Left, level.Right);
                    }
                    frame -= level.Frames;
                }
                return (0, 0);
            }
        }
    }
}
