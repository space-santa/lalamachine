using System.Collections.Generic;
using LibLala.Audio;
using NUnit.Framework;

namespace LibLala.Unittests.Audio
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "The teardown disposes them.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "The engine takes over the decoders.")]
    public sealed class AudioEngineTest
    {
        private FakeOutput _output = null!;
        private AudioEngine _engine = null!;
        private List<int> _ended = null!;
        private List<int> _failed = null!;

        [SetUp]
        public void SetUp()
        {
            _engine = new AudioEngine(renderer => _output = new FakeOutput(renderer));
            _ended = [];
            _failed = [];
            _engine.Ended += (sender, e) => _ended.Add(e.Track);
            _engine.Failed += (sender, e) => _failed.Add(e.Track);
        }

        [TearDown]
        public void TearDown()
        {
            _engine.Dispose();
        }

        [Test]
        public void Position_IsWhatWasPlayed_NotWhatWasRendered()
        {
            _engine.Open(new CountingDecoder(100));
            _engine.Play();

            Assert.That(_engine.Position, Is.Zero);
            Assert.That(_output.PlayBuffer(), Is.EqualTo(new float[] { 0, 1, 2, 3 }));
            Assert.That(_engine.Position, Is.EqualTo(4));
        }

        [Test]
        public void Ended_IsRaisedOnce_WhenTheLastFrameWasPlayed()
        {
            var track = _engine.Open(new CountingDecoder(10));
            _engine.Play();

            // All of the track is rendered, three buffers ahead.
            _output.PlayBuffer();
            _output.PlayBuffer();
            Assert.That(_ended, Is.Empty);

            Assert.That(_output.PlayBuffer(), Is.EqualTo(new float[] { 8, 9, 0, 0 }));
            Assert.That(_ended, Is.EqualTo(new[] { track }));
            Assert.That(_engine.Position, Is.EqualTo(10));

            _output.PlayBuffer();
            Assert.That(_ended, Has.Count.EqualTo(1));
        }

        [Test]
        public void Seek_DropsWhatWasRendered_AndPlaysFromThere()
        {
            _engine.Open(new CountingDecoder(100));
            _engine.Play();
            _output.PlayBuffer();

            _engine.Seek(50);

            Assert.That(_engine.Position, Is.EqualTo(50));
            Assert.That(_output.PlayBuffer(), Is.EqualTo(new float[] { 50, 51, 52, 53 }));
        }

        [Test]
        public void Seek_WhilePlaying_DoesntStartTheOutputAgain()
        {
            _engine.Open(new CountingDecoder(100));
            _engine.Play();

            _engine.Seek(50);
            _engine.Seek(60);

            Assert.That(_output.Starts, Is.EqualTo(1));
            Assert.That(_output.PlayBuffer(), Is.EqualTo(new float[] { 60, 61, 62, 63 }));
        }

        [Test]
        public void Seek_WhilePaused_StaysPaused()
        {
            _engine.Open(new CountingDecoder(100));
            _engine.Play();
            _engine.Pause();

            _engine.Seek(20);

            Assert.That(_output.IsRunning, Is.False);
            Assert.That(_engine.Position, Is.EqualTo(20));
            _engine.Play();
            Assert.That(_output.PlayBuffer(), Is.EqualTo(new float[] { 20, 21, 22, 23 }));
        }

        [Test]
        public void Seek_PastTheEnd_EndsTheTrack()
        {
            var track = _engine.Open(new CountingDecoder(100));
            _engine.Play();

            _engine.Seek(1000);
            _output.PlayBuffer();

            Assert.That(_ended, Is.EqualTo(new[] { track }));
        }

        [Test]
        public void Open_StopsTheTrackThatPlays_AndGivesTheNewOneANewNumber()
        {
            var first = new CountingDecoder(100);
            var firstTrack = _engine.Open(first);
            _engine.Play();
            _output.PlayBuffer();

            var secondTrack = _engine.Open(new CountingDecoder(100));

            Assert.That(secondTrack, Is.Not.EqualTo(firstTrack));
            Assert.That(first.IsDisposed, Is.True);
            Assert.That(_output.IsRunning, Is.False);
            Assert.That(_engine.Position, Is.Zero);
            _engine.Play();
            Assert.That(_output.PlayBuffer(), Is.EqualTo(new float[] { 0, 1, 2, 3 }));
        }

        [Test]
        public void Play_AfterTheEnd_StartsFromTheBeginning()
        {
            _engine.Open(new CountingDecoder(10));
            _engine.Play();
            for (var i = 0; i < 3; i++)
            {
                _output.PlayBuffer();
            }
            _engine.Stop();

            _engine.Play();

            Assert.That(_output.PlayBuffer(), Is.EqualTo(new float[] { 0, 1, 2, 3 }));
        }

        [Test]
        public void Stop_ContinuesWhereItWasPlayed_NotWhereItWasRendered()
        {
            _engine.Open(new CountingDecoder(100));
            _engine.Play();
            _output.PlayBuffer();

            _engine.Stop();

            Assert.That(_output.IsRunning, Is.False);
            Assert.That(_engine.Position, Is.EqualTo(4));
            _engine.Play();
            Assert.That(_output.PlayBuffer(), Is.EqualTo(new float[] { 4, 5, 6, 7 }));
        }

        [Test]
        public void ADecoderThatFails_RaisesFailedOnce_AndPlaysSilence()
        {
            var track = _engine.Open(new CountingDecoder(100) { FailsFrom = 12 });
            _engine.Play();

            _output.PlayBuffer();
            Assert.That(_failed, Is.EqualTo(new[] { track }));
            Assert.That(_output.PlayBuffer(), Is.EqualTo(new float[] { 4, 5, 6, 7 }));
            _output.PlayBuffer();
            Assert.That(_output.PlayBuffer(), Is.EqualTo(new float[] { 0, 0, 0, 0 }));
            Assert.That(_failed, Has.Count.EqualTo(1));
        }

        [Test]
        public void ASeekThatFails_RaisesFailed_AndDoesntPlay()
        {
            var track = _engine.Open(new CountingDecoder(100) { FailsFrom = 50 });
            _engine.Play();

            _engine.Seek(60);

            Assert.That(_failed, Is.EqualTo(new[] { track }));
            Assert.That(_output.IsRunning, Is.False);
            _engine.Play();
            Assert.That(_output.IsRunning, Is.False);
        }

        [Test]
        public void AnOutputThatCantPlay_RaisesFailed()
        {
            var track = _engine.Open(new CountingDecoder(100));
            _output.FailsToPlay = true;

            _engine.Play();

            Assert.That(_failed, Is.EqualTo(new[] { track }));
        }

        [Test]
        public void Close_StopsAndPlaysNothing()
        {
            var decoder = new CountingDecoder(100);
            _engine.Open(decoder);
            _engine.Play();

            _engine.Close();
            _engine.Play();

            Assert.That(decoder.IsDisposed, Is.True);
            Assert.That(_output.IsRunning, Is.False);
            Assert.That(_engine.Length, Is.Zero);
        }
    }
}
