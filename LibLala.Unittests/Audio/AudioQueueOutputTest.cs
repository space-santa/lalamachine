using System;
using System.Runtime.Versioning;
using System.Threading;
using LibLala.Audio;
using LibLala.Audio.MacOS;
using NUnit.Framework;

namespace LibLala.Unittests.Audio
{
    // Plays silence through the Mac's output, at volume 0.
    [Category("AudioDevice")]
    [SupportedOSPlatform("macos")]
    public sealed class AudioQueueOutputTest
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(2);

        [SetUp]
        public void SetUp()
        {
            if (!OperatingSystem.IsMacOS())
            {
                Assert.Ignore("Plays through macOS's AudioQueue.");
            }
        }

        [Test]
        public void Play_RendersAhead_AndReportsWhatWasPlayed()
        {
            var renderer = new CountingRenderer();
            using var output = new AudioQueueOutput(renderer) { Volume = 0 };

            output.Play();

            Assert.That(renderer.Rendered, Is.EqualTo(3));
            Assert.That(SpinWait.SpinUntil(() => renderer.Played >= 3, Timeout), Is.True);
            Assert.That(renderer.Rendered - renderer.Played, Is.EqualTo(3));
        }

        [Test]
        public void Pause_StopsPlaying_AndPlayContinues()
        {
            var renderer = new CountingRenderer();
            using var output = new AudioQueueOutput(renderer) { Volume = 0 };
            output.Play();
            SpinWait.SpinUntil(() => renderer.Played >= 1, Timeout);

            output.Pause();
            // A callback can be on its way.
            Thread.Sleep(50);
            var played = renderer.Played;
            Thread.Sleep(200);

            Assert.That(renderer.Played, Is.EqualTo(played));
            output.Play();
            Assert.That(SpinWait.SpinUntil(() => renderer.Played > played, Timeout), Is.True);
        }

        [Test]
        public void Reset_DropsWhatWasRendered_AndNothingIsRenderedUntilPlay()
        {
            var renderer = new CountingRenderer();
            using var output = new AudioQueueOutput(renderer) { Volume = 0 };
            output.Play();
            SpinWait.SpinUntil(() => renderer.Played >= 1, Timeout);

            output.Reset();
            var played = renderer.Played;
            var rendered = renderer.Rendered;
            Thread.Sleep(200);

            Assert.That(renderer.Played, Is.EqualTo(played));
            Assert.That(renderer.Rendered, Is.EqualTo(rendered));
            output.Play();
            Assert.That(renderer.Rendered, Is.EqualTo(rendered + 3));
            Assert.That(SpinWait.SpinUntil(() => renderer.Played > played, Timeout), Is.True);
        }

        [Test]
        public void Reset_WhilePaused_StaysPaused_AndPlayContinues()
        {
            var renderer = new CountingRenderer();
            using var output = new AudioQueueOutput(renderer) { Volume = 0 };
            output.Play();
            SpinWait.SpinUntil(() => renderer.Played >= 1, Timeout);
            output.Pause();
            Thread.Sleep(50);

            output.Reset();
            var played = renderer.Played;
            Thread.Sleep(200);

            Assert.That(renderer.Played, Is.EqualTo(played));
            output.Play();
            Assert.That(SpinWait.SpinUntil(() => renderer.Played > played, Timeout), Is.True);
        }

        [Test]
        public void Dispose_WhilePlaying_Returns()
        {
            var renderer = new CountingRenderer();
            var output = new AudioQueueOutput(renderer) { Volume = 0 };
            output.Play();
            SpinWait.SpinUntil(() => renderer.Played >= 1, Timeout);

            output.Dispose();
            var rendered = renderer.Rendered;
            Thread.Sleep(100);

            Assert.That(renderer.Rendered, Is.EqualTo(rendered));
        }

        private sealed class CountingRenderer : IAudioRenderer
        {
            private int _rendered;
            private int _played;

            public int Rendered => Volatile.Read(ref _rendered);
            public int Played => Volatile.Read(ref _played);

            public void Render(Span<float> buffer)
            {
                buffer.Clear();
                Interlocked.Increment(ref _rendered);
            }

            void IAudioRenderer.Played() => Interlocked.Increment(ref _played);
        }
    }
}
