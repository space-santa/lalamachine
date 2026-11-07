using System.Collections.Generic;
using LibLala.Audio;
using NUnit.Framework;

namespace LibLala.Unittests.Audio
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "The teardown disposes them.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "The engine takes over the decoders.")]
    public sealed class AudioEngineTransitionTest
    {
        private const int Rate = AudioFormat.SampleRate;
        private AudioEngine _engine = null!;
        private List<int> _ended = null!;
        private List<(int Track, double Seconds)> _due = null!;
        private List<int> _tookOver = null!;
        private CountingDecoder _a = null!;

        // 120 BPM, bars of 2 s from 0, the music from 4 s to the last hit at 56 s: the earliest transition at 56 - 32 s.
        private static BeatGrid Grid(double bpm = 120) => new(bpm, 0, 2, 500, 0, 4, 56);

        [SetUp]
        public void SetUp()
        {
            // Render and Played go together here, what is played is what was rendered.
            _engine = new AudioEngine(renderer => new SilentOutput()) { Transitions = true };
            _ended = [];
            _due = [];
            _tookOver = [];
            _engine.Ended += (sender, e) => _ended.Add(e.Track);
            _engine.TookOver += (sender, e) => _tookOver.Add(e.Track);
            _engine.TransitionDue += (sender, e) => _due.Add((e.Track, _a.Position / (double)Rate));
            _a = new CountingDecoder(60 * Rate) { Silent = true };
        }

        [TearDown]
        public void TearDown()
        {
            _engine.Dispose();
        }

        // Renders and plays seconds of audio, in tenths. Returns the peak of the last tenth.
        private float PlayFor(double seconds)
        {
            var buffer = new float[Rate / 10 * AudioFormat.Channels];
            for (var i = 0; i < seconds * 10; i++)
            {
                _engine.Render(buffer);
                _engine.Played();
            }
            var peak = 0f;
            foreach (var sample in buffer)
            {
                peak = System.Math.Max(peak, System.Math.Abs(sample));
            }
            return peak;
        }

        private void MixAt23Seconds(IAudioDecoder a, IAudioDecoder b)
        {
            _engine.Open(a, Grid());
            _engine.Play();
            PlayFor(23);
            _engine.MixTo(b, Grid());
        }

        // The transition goes from 24 s to 56 s of A: a quarter of it is at 32 s, half at 40 s.
        [Test]
        public void TheOutgoingTrack_LosesItsBassFirst()
        {
            MixAt23Seconds(new SineDecoder(60, 60 * Rate), new CountingDecoder(60 * Rate) { Silent = true });

            Assert.That(PlayFor(0.5), Is.EqualTo(0.5).Within(0.01));
            // The crossover at 632 Hz.
            Assert.That(PlayFor(16.5), Is.LessThan(0.005));
        }

        [Test]
        public void TheOutgoingTracksHighs_StayLonger()
        {
            MixAt23Seconds(new SineDecoder(5000, 60 * Rate), new CountingDecoder(60 * Rate) { Silent = true });

            Assert.That(PlayFor(17.5), Is.EqualTo(0.5).Within(0.02));
        }

        [Test]
        public void TheIncomingTrack_ComesInFromItsBass()
        {
            MixAt23Seconds(new CountingDecoder(60 * Rate) { Silent = true }, new SineDecoder(5000, 60 * Rate));

            // The crossover at 112 Hz.
            Assert.That(PlayFor(9.5), Is.LessThan(0.005));
            Assert.That(PlayFor(25), Is.EqualTo(0.5).Within(0.01));
        }

        [Test]
        public void TheIncomingTracksBass_IsThereHalfWay()
        {
            MixAt23Seconds(new CountingDecoder(60 * Rate) { Silent = true }, new SineDecoder(60, 60 * Rate));

            Assert.That(PlayFor(17.5), Is.EqualTo(0.5).Within(0.02));
        }

        [Test]
        public void TransitionDue_ComesTwoSecondsBeforeTheEarliestTransition()
        {
            var track = _engine.Open(_a, Grid());
            _engine.Play();

            PlayFor(21.5);
            Assert.That(_due, Is.Empty);
            PlayFor(1);

            Assert.That(_due, Has.Count.EqualTo(1));
            Assert.That(_due[0].Track, Is.EqualTo(track));
            Assert.That(_due[0].Seconds, Is.EqualTo(22).Within(0.11));
        }

        [Test]
        public void WithoutTransitions_ThereIsNoTransitionDue()
        {
            _engine.Transitions = false;
            _engine.Open(_a, Grid());
            _engine.Play();

            PlayFor(30);

            Assert.That(_due, Is.Empty);
        }

        [Test]
        public void TheNextTrack_ComesInOnTheFirstBeatOfTheLast16Bars()
        {
            _engine.Open(_a, Grid());
            _engine.Play();
            PlayFor(23);
            long? aWhenBCameIn = null;
            var b = new CountingDecoder(60 * Rate) { Silent = true };
            b.BeforeFirstRead = () => aWhenBCameIn = _a.Position;

            _engine.MixTo(b, Grid());
            PlayFor(2);

            Assert.That(aWhenBCameIn, Is.EqualTo(24 * Rate));
        }

        [Test]
        public void TheOutgoingTrack_IsTheCurrentOne_UntilTheTransitionIsOver()
        {
            _engine.Open(_a, Grid());
            _engine.Play();
            PlayFor(23);

            var trackB = _engine.MixTo(new CountingDecoder(50 * Rate) { Silent = true }, Grid());
            PlayFor(32.5);

            Assert.That(_engine.Position / (double)Rate, Is.EqualTo(55.5).Within(0.01));
            Assert.That(_engine.Length, Is.EqualTo(60 * Rate));
            Assert.That(_tookOver, Is.Empty);
            PlayFor(1);
            Assert.That(_tookOver, Is.EqualTo(new[] { trackB }));
            Assert.That(_engine.Length, Is.EqualTo(50 * Rate));
        }

        [Test]
        public void TheOutgoingTrack_DoesntEnd_TheIncomingOneDoes()
        {
            _engine.Open(_a, Grid());
            _engine.Play();
            PlayFor(23);
            var b = new CountingDecoder(60 * Rate) { Silent = true };
            var trackB = _engine.MixTo(b, Grid());

            // A is over at 56 s, B started at 4 s when A was at 24 s.
            PlayFor(34);
            Assert.That(_a.IsDisposed, Is.True);
            Assert.That(_ended, Is.Empty);
            Assert.That(_tookOver, Is.EqualTo(new[] { trackB }));
            PlayFor(30);

            Assert.That(_ended, Is.EqualTo(new[] { trackB }));
        }

        [Test]
        public void TemposTooFarApart_Crossfade_AtTheEndOfTheMusic()
        {
            _engine.Open(_a, Grid());
            _engine.Play();
            PlayFor(23);
            long? aWhenBCameIn = null;
            var b = new CountingDecoder(60 * Rate) { Silent = true };
            b.BeforeFirstRead = () => aWhenBCameIn = _a.Position;

            _engine.MixTo(b, Grid(bpm: 140));
            PlayFor(35);

            // The music ends with the bar at 56 s, the file 4 s later.
            Assert.That(aWhenBCameIn, Is.EqualTo(52 * Rate));
        }

        [Test]
        public void TheIncomingTrack_IsThereAsHeard_FromWhenItComesInUntilItTakesOver()
        {
            _engine.Open(_a, Grid());
            _engine.Play();
            PlayFor(23);
            _engine.MixTo(new CountingDecoder(50 * Rate) { Silent = true }, Grid());

            PlayFor(0.5);
            Assert.That(_engine.IncomingPosition, Is.Null);
            Assert.That(_engine.IncomingLength, Is.Null);
            PlayFor(1);
            // In at 24 s with its bar at 4 s, A is at 24.5 s.
            Assert.That(_engine.IncomingPosition / (double)Rate, Is.EqualTo(4.5).Within(0.01));
            Assert.That(_engine.IncomingLength, Is.EqualTo(50 * Rate));
            PlayFor(32);
            Assert.That(_tookOver, Has.Count.EqualTo(1));
            Assert.That(_engine.IncomingPosition, Is.Null);
        }

        [Test]
        public void ASeekIntoTheTransition_PutsTheIncomingTrackWhereItWouldBe()
        {
            _engine.Open(_a, Grid());
            _engine.Play();
            PlayFor(23);
            var b = new CountingDecoder(50 * Rate) { Silent = true };
            _engine.MixTo(b, Grid());

            // Half way: B came in at 4 s, and is 16 s on.
            _engine.Seek(40 * Rate);
            Assert.That(_engine.Position, Is.EqualTo(40 * Rate));
            PlayFor(1);

            Assert.That(b.Position / (double)Rate, Is.EqualTo(21).Within(0.01));
            Assert.That(_engine.Position / (double)Rate, Is.EqualTo(41).Within(0.01));
            Assert.That(_tookOver, Is.Empty);
        }

        [Test]
        public void DraggingToTheEnd_StaysInTheTrack_AndTheNextOneTakesOverAfter()
        {
            _engine.Open(_a, Grid());
            _engine.Play();
            PlayFor(23);
            var trackB = _engine.MixTo(new CountingDecoder(50 * Rate) { Silent = true }, Grid());

            // Like the position slider held at the end: a seek every tenth.
            for (var i = 0; i < 30; i++)
            {
                _engine.Seek(59 * Rate);
                Assert.That(_engine.Position / (double)Rate, Is.EqualTo(55.5).Within(0.01));
                PlayFor(0.1);
            }
            Assert.That(_tookOver, Is.Empty);
            Assert.That(_due, Has.Count.EqualTo(1));

            PlayFor(1);
            Assert.That(_tookOver, Is.EqualTo(new[] { trackB }));
        }

        [Test]
        public void ASeekBackToBeforeTheTransitionIsDue_CallsItOff_UntilItsDueAgain()
        {
            _engine.Open(_a, Grid());
            _engine.Play();
            PlayFor(23);
            var b = new CountingDecoder(50 * Rate) { Silent = true };
            _engine.MixTo(b, Grid());
            PlayFor(2);

            _engine.Seek(10 * Rate);

            Assert.That(b.IsDisposed, Is.True);
            Assert.That(_due, Has.Count.EqualTo(1));
            PlayFor(12.5);
            Assert.That(_due, Has.Count.EqualTo(2));
            Assert.That(_due[1].Seconds, Is.EqualTo(22).Within(0.11));
        }

        [Test]
        public void ASeekToBeforeTheTransitionStarts_LetsTheIncomingTrackWait()
        {
            _engine.Open(_a, Grid());
            _engine.Play();
            PlayFor(23);
            var b = new CountingDecoder(50 * Rate) { Silent = true };
            _engine.MixTo(b, Grid());
            PlayFor(3);

            _engine.Seek((long)(22.5 * Rate));
            PlayFor(1);
            // A at 23.5 s, B waits at the start of its first bar.
            Assert.That(b.Position, Is.EqualTo(4 * Rate));
            PlayFor(1);

            Assert.That(b.Position / (double)Rate, Is.EqualTo(4.5).Within(0.01));
        }

        [Test]
        public void MixTo_InTheTransition_ComesInWhereItWouldBe()
        {
            _engine.Open(_a, Grid());
            _engine.Play();
            _engine.Seek(40 * Rate);
            PlayFor(0.1);
            Assert.That(_due, Has.Count.EqualTo(1));
            var b = new CountingDecoder(50 * Rate) { Silent = true };

            _engine.MixTo(b, Grid());
            PlayFor(1);

            // A rendered to 40.1 s when B came in, at 4 + 16.1 s.
            Assert.That(b.Position / (double)Rate, Is.EqualTo(21.1).Within(0.02));
        }

        [Test]
        public void MixTo_AfterTheTransitionWouldBeOver_GoesOverShortly()
        {
            _engine.Open(_a, Grid());
            _engine.Play();
            _engine.Seek((long)(57.5 * Rate));
            PlayFor(0.1);
            var trackB = _engine.MixTo(new CountingDecoder(50 * Rate) { Silent = true }, Grid());

            PlayFor(3);

            Assert.That(_tookOver, Is.EqualTo(new[] { trackB }));
        }

        [Test]
        public void MixTo_WithoutATrack_LeavesItToOpen()
        {
            var b = new CountingDecoder(60 * Rate) { Silent = true };

            Assert.That(_engine.MixTo(b, Grid()), Is.Zero);
            Assert.That(b.IsDisposed, Is.False);
            Assert.That(_engine.Length, Is.Zero);
        }
    }
}
