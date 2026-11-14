using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibLala.Audio;
using LibLala.Audio.Mixing;
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

        // 120 BPM, bars of 2 s from 0, the music from 4 s to the last hit at 56 s: the transition from 56 - 32 s, the tempo
        // going to it from 56 - 48 s.
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
        public void TwoLoudTracks_StayUnderTheCeiling()
        {
            // A's highs and B's bass go through the crossover at full level most of the way, together nearly twice full scale.
            MixAt23Seconds(new SineDecoder(5000, 60 * Rate) { Amplitude = 0.9 }, new SineDecoder(60, 60 * Rate) { Amplitude = 0.9 });
            PlayFor(1);

            var peaks = Enumerable.Range(0, 320).Select(i => PlayFor(0.1)).ToList();

            Assert.That(peaks.Max(), Is.LessThanOrEqualTo(Limiter.Ceiling + 1e-6));
            // Half way, both are there.
            Assert.That(peaks[160], Is.EqualTo(Limiter.Ceiling).Within(0.01));
        }

        [Test]
        public void AfterTheTransition_TheNextTrackComesOutAsItIs()
        {
            // Every sample of B is far above full scale, the limiter turns it all the way down while A is there.
            var b = new CountingDecoder(60 * Rate);
            MixAt23Seconds(_a, b);
            PlayFor(33 + 2);
            var position = _engine.Position;

            var buffer = new float[100 * AudioFormat.Channels];
            _engine.Render(buffer);

            Assert.That(Enumerable.Range(0, 100).Select(i => buffer[i * AudioFormat.Channels]), Is.EqualTo(Enumerable.Range(0, 100).Select(i => (float)(position + i))));
        }

        [Test]
        public void TransitionDue_ComesTwoSecondsBeforeTheEarliestTempoChange()
        {
            var track = _engine.Open(_a, Grid());
            _engine.Play();

            PlayFor(5.5);
            Assert.That(_due, Is.Empty);
            PlayFor(1);

            Assert.That(_due, Has.Count.EqualTo(1));
            Assert.That(_due[0].Track, Is.EqualTo(track));
            Assert.That(_due[0].Seconds, Is.EqualTo(6).Within(0.11));
        }

        // Plays A until it's at the second as heard.
        private void PlayTo(double secondsOfA)
        {
            while (_engine.Position < secondsOfA * Rate)
            {
                PlayFor(0.1);
            }
        }

        // When it's due at 6 s of A: A's tempo goes halfway to B's from 8 s to 24 s, where B comes in.
        private void MixWhenDue(IAudioDecoder b, double bpmB)
        {
            _engine.Open(_a, Grid());
            _engine.Play();
            PlayFor(6.1);
            Assert.That(_due, Has.Count.EqualTo(1));
            _engine.MixTo(b, Grid(bpmB));
        }

        [Test]
        public void TheCurrentTrack_GoesHalfwayToTheNextOnesTempo_InThe8BarsBeforeTheTransition()
        {
            MixWhenDue(new CountingDecoder(60 * Rate) { Silent = true }, 125);
            var halfway = Math.Sqrt(125 / 120.0);

            PlayTo(7.9);
            Assert.That((_engine.Tempo, _engine.BeatsPerMinute), Is.EqualTo((1.0, 120.0)));
            PlayTo(16);
            Assert.That(_engine.Tempo, Is.EqualTo((1 + halfway) / 2).Within(0.001));
            PlayTo(24.2);

            Assert.That(_engine.Tempo, Is.EqualTo(halfway).Within(1e-9));
            Assert.That(_engine.BeatsPerMinute, Is.EqualTo(Math.Sqrt(120 * 125.0)).Within(1e-9));
        }

        [Test]
        public void AfterGoingHalfway_TheNextTrackComesInOnTheBeat_AndGoesTheOtherHalf()
        {
            MixWhenDue(new CountingDecoder(60 * Rate) { Silent = true }, 125);
            PlayTo(23.9);
            Assert.That(_engine.IncomingPosition, Is.Null);

            // A's beats from 24 s every 0.5 s, B's from its bar at 3.84 s every 0.48 s: as many of each.
            while (_engine.IncomingPosition is null)
            {
                PlayFor(0.1);
            }
            Assert.That(_engine.Position / (double)Rate, Is.InRange(24, 24.11));
            Assert.That((_engine.IncomingPosition.Value / (double)Rate - 3.84) / 0.48, Is.EqualTo((_engine.Position / (double)Rate - 24) / 0.5).Within(0.02));
            PlayTo(40);
            Assert.That((_engine.IncomingPosition!.Value / (double)Rate - 3.84) / 0.48, Is.EqualTo((_engine.Position / (double)Rate - 24) / 0.5).Within(0.02));

            while (_tookOver.Count == 0)
            {
                PlayFor(0.1);
            }
            Assert.That(_engine.BeatsPerMinute, Is.EqualTo(Math.Sqrt(120 * 125.0)).Within(0.05));
            PlayFor(17);
            Assert.That((_engine.Tempo, _engine.BeatsPerMinute), Is.EqualTo((1.0, 125.0)));
        }

        [Test]
        public void ASeekIntoTheBarsBeforeTheTransition_PutsTheCurrentTrackAtItsTempoThere()
        {
            var b = new CountingDecoder(60 * Rate) { Silent = true };
            MixWhenDue(b, 125);
            var halfway = Math.Sqrt(125 / 120.0);

            _engine.Seek(16 * Rate);
            PlayFor(0.1);
            Assert.That(_engine.Tempo, Is.EqualTo((1 + halfway) / 2).Within(0.001));
            _engine.Seek(30 * Rate);
            PlayFor(0.1);
            Assert.That(_engine.Tempo, Is.EqualTo(halfway).Within(1e-9));
            // Due, the tempo not changing yet.
            _engine.Seek(7 * Rate);
            PlayFor(0.1);
            Assert.That(_engine.Tempo, Is.EqualTo(1));
            Assert.That(b.IsDisposed, Is.False);
            _engine.Seek(16 * Rate);
            _engine.Seek(2 * Rate);
            PlayFor(0.1);

            Assert.That(_engine.Tempo, Is.EqualTo(1));
            Assert.That(b.IsDisposed, Is.True);
        }

        [Test]
        public void MixTo_LateForSomeOfTheBarsBefore_GoesHalfwayInTheWholeBarsLeft()
        {
            _engine.Open(_a, Grid());
            _engine.Play();
            PlayFor(19);
            _engine.MixTo(new CountingDecoder(60 * Rate) { Silent = true }, Grid(125));
            var halfway = Math.Sqrt(125 / 120.0);

            // From the bar at 20 s.
            PlayTo(19.9);
            Assert.That(_engine.Tempo, Is.EqualTo(1));
            PlayTo(22);
            Assert.That(_engine.Tempo, Is.EqualTo((1 + halfway) / 2).Within(0.001));
            PlayTo(24.2);

            Assert.That(_engine.Tempo, Is.EqualTo(halfway).Within(1e-9));
        }

        [Test]
        public void MixTo_InTheTransition_TheCurrentTrackKeepsItsTempo_AndTheNextOneGoesAllTheWay()
        {
            _engine.Open(_a, Grid());
            _engine.Play();
            _engine.Seek(40 * Rate);
            PlayFor(0.1);

            _engine.MixTo(new CountingDecoder(60 * Rate) { Silent = true }, Grid(125));
            PlayFor(1);

            Assert.That(_engine.Tempo, Is.EqualTo(1));
            // A at 41.1 s, B came in with its bar at 3.84 s when A was at 24 s, at 0.96 since.
            Assert.That(_engine.IncomingPosition / (double)Rate, Is.EqualTo(3.84 + (17.1 * 0.96)).Within(0.01));
        }

        [Test]
        public void TurnedOff_InTheBarsBeforeTheTransition_TheCurrentTrackGoesBackToItsOwnTempo()
        {
            var b = new CountingDecoder(60 * Rate) { Silent = true };
            MixWhenDue(b, 125);
            PlayTo(20);
            Assert.That(_engine.Tempo, Is.GreaterThan(1.01));

            _engine.Transitions = false;
            PlayFor(8);
            Assert.That(_engine.Tempo, Is.GreaterThan(1.005).And.LessThan(1.01));
            PlayFor(8.5);

            Assert.That(_engine.Tempo, Is.EqualTo(1));
            Assert.That(b.IsDisposed, Is.True);
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
        public void TemposTooFarApart_TheNextTrackComesInOnTheDownbeatWhereTheMusicEnds()
        {
            _engine.Open(_a, Grid());
            _engine.Play();
            PlayFor(23);
            long? aWhenBCameIn = null;
            long? bWhenItCameIn = null;
            var b = new CountingDecoder(60 * Rate) { Silent = true };
            b.BeforeFirstRead = () => (aWhenBCameIn, bWhenItCameIn) = (_a.Position, b.Position);

            var trackB = _engine.MixTo(b, Grid(bpm: 140));
            PlayFor(33.1);

            // The music ends with the bar at 56 s, the file 4 s later. B's first bar at 3.43 s is heard then, faded in
            // before.
            var lead = TransitionPlanner.CutSeconds * Rate;
            Assert.That(aWhenBCameIn, Is.EqualTo((56 * Rate) - lead).Within(1));
            Assert.That(bWhenItCameIn, Is.EqualTo((480.0 / 140 * Rate) - lead).Within(1));
            Assert.That(_tookOver, Is.EqualTo(new[] { trackB }));
        }

        [Test]
        public void ACut_OnlyFades()
        {
            // A only has bass. Through the crossover, it would be gone half way through.
            _engine.Open(new SineDecoder(60, 60 * Rate), Grid());
            _engine.Play();
            PlayFor(23);
            _engine.MixTo(new CountingDecoder(60 * Rate) { Silent = true }, Grid(bpm: 140));
            PlayFor(32.9);

            // To half way through the cut at 55.98 s: A at 55.99 s, the bass peaks at 55.9875 s.
            var buffer = new float[(int)(0.09 * Rate) * AudioFormat.Channels];
            _engine.Render(buffer);
            var peak = 0f;
            for (var i = buffer.Length - (Rate / 200 * AudioFormat.Channels); i < buffer.Length; i++)
            {
                peak = Math.Max(peak, Math.Abs(buffer[i]));
            }

            Assert.That(peak, Is.GreaterThan(0.3));
        }

        [Test]
        public void WithoutAGrid_TheNextTrackCrossfades_AtTheEndOfTheMusic()
        {
            _engine.Open(_a, Grid());
            _engine.Play();
            PlayFor(23);
            long? aWhenBCameIn = null;
            var b = new CountingDecoder(60 * Rate) { Silent = true };
            b.BeforeFirstRead = () => aWhenBCameIn = _a.Position;

            _engine.MixTo(b, null);
            PlayFor(35);

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

            _engine.Seek(2 * Rate);

            Assert.That(b.IsDisposed, Is.True);
            Assert.That(_due, Has.Count.EqualTo(1));
            PlayFor(4.5);
            Assert.That(_due, Has.Count.EqualTo(2));
            Assert.That(_due[1].Seconds, Is.EqualTo(6).Within(0.11));
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
        public void MixTo_SeeksTheIncomingTrack_WithoutKeepingTheOutputWaiting()
        {
            _engine.Open(_a, Grid());
            _engine.Play();
            PlayFor(23);
            var rendered = false;
            var b = new CountingDecoder(60 * Rate) { Silent = true };
            // The output's thread wants audio while the file is read.
            b.OnSeek = () => rendered = Task.Run(() => _engine.Render(new float[256 * AudioFormat.Channels])).Wait(TimeSpan.FromSeconds(5));

            _engine.MixTo(b, Grid());

            Assert.That(rendered, Is.True);
        }

        [Test]
        public void MixTo_InTheTransition_ComesInWhereTheCurrentTrackGotTo_WhileTheFileWasRead()
        {
            _engine.Open(_a, Grid());
            _engine.Play();
            _engine.Seek(40 * Rate);
            PlayFor(0.1);
            var b = new CountingDecoder(50 * Rate) { Silent = true };
            var seeks = 0;
            b.OnSeek = () =>
            {
                if (seeks++ == 0)
                {
                    Task.Run(() => PlayFor(0.5)).Wait(TimeSpan.FromSeconds(5));
                }
            };

            _engine.MixTo(b, Grid());
            PlayFor(1);

            // A rendered to 40.6 s when B came in, at 4 + 16.6 s.
            Assert.That(b.Position / (double)Rate, Is.EqualTo(21.6).Within(0.02));
        }

        [Test]
        public void MixTo_WhileTheTempoComesBackAfterATransition_HoldsItForThePlan()
        {
            // B at 125 BPM plays at 0.96 to come in, mixed in too late for A to go halfway, and goes back to 1 over 8
            // bars after it took over at 30.72 s. Short, its own transition is due right then, its tempo going to the next
            // one's from the bar at 32.64 s.
            var gridB = new BeatGrid(125, 0, 2, 500, 0, 0, 70);
            _engine.Open(_a, Grid());
            _engine.Play();
            PlayFor(23);
            _engine.MixTo(new CountingDecoder(72 * Rate) { Silent = true }, gridB);
            while (_due.Count < 2)
            {
                PlayFor(0.1);
            }
            Assert.That(_tookOver, Has.Count.EqualTo(1));

            _engine.MixTo(new CountingDecoder(60 * Rate) { Silent = true }, gridB);
            // The limiter still lets out the end of the transition before, B doesn't move on meanwhile.
            PlayFor(0.1);
            var before = _engine.Position;
            PlayFor(0.8);
            var between = _engine.Position;
            PlayFor(0.8);

            // As fast in both, still before its tempo changes.
            Assert.That(_engine.Position / (double)Rate, Is.LessThan(32.64));
            Assert.That(_engine.Position - between, Is.EqualTo(between - before).Within(10));
            Assert.That(_engine.Tempo, Is.EqualTo(0.96).Within(0.001));
            // At its own for the transition at 40.32 s, the next track's is the same.
            PlayTo(40.4);
            Assert.That(_engine.Tempo, Is.EqualTo(1));
        }

        [Test]
        public void AfterATransition_TheTempoAsPlayed_IsTheOneBeforeUntilTheTrackIsBackAtItsOwn()
        {
            _engine.Open(_a, Grid());
            _engine.Play();
            PlayFor(23);
            Assert.That((_engine.Tempo, _engine.BeatsPerMinute), Is.EqualTo((1.0, 120.0)));

            // B at 125 BPM comes in at 0.96, and goes back to 1 over 8 bars of 2 s after it took over at 56 s of A.
            _engine.MixTo(new CountingDecoder(60 * Rate) { Silent = true }, Grid(bpm: 125));
            PlayFor(33.1);
            Assert.That(_tookOver, Has.Count.EqualTo(1));
            Assert.That(_engine.Tempo, Is.EqualTo(0.96).Within(0.001));
            Assert.That(_engine.BeatsPerMinute, Is.EqualTo(120).Within(0.1));
            PlayFor(8);
            Assert.That(_engine.BeatsPerMinute, Is.EqualTo(122.5).Within(0.1));
            PlayFor(8.5);

            Assert.That((_engine.Tempo, _engine.BeatsPerMinute), Is.EqualTo((1.0, 125.0)));
        }

        [Test]
        public void WithoutAGrid_ThereAreNoBeatsPerMinute()
        {
            _engine.Open(_a, null);

            Assert.That((_engine.Tempo, _engine.BeatsPerMinute), Is.EqualTo((1.0, (double?)null)));
        }

        [Test]
        public void ASeekAfterTheTransitionWasRendered_ButBeforeItWasHeard_DoesntSeekTheNextTrack()
        {
            _engine.Open(_a, Grid());
            _engine.Play();
            PlayFor(23);
            var trackB = _engine.MixTo(new CountingDecoder(50 * Rate) { Silent = true }, Grid());
            PlayFor(32.9);
            // Past the end of the transition at 56 s of A, not played yet.
            _engine.Render(new float[Rate / 5 * AudioFormat.Channels]);
            Assert.That(_tookOver, Is.Empty);

            // From A's position slider.
            _engine.Seek(58 * Rate);

            Assert.That(_tookOver, Is.EqualTo(new[] { trackB }));
            // B came in at 4 s when A was at 24 s, and was heard until A was at 55.9 s.
            Assert.That(_engine.Position / (double)Rate, Is.EqualTo(35.9).Within(0.01));
            PlayFor(1);
            Assert.That(_ended, Is.Empty);
        }

        [Test]
        public void TurnedOff_ATransitionThatDidntStart_IsCalledOff()
        {
            var trackA = _engine.Open(_a, Grid());
            _engine.Play();
            PlayFor(23);
            var b = new CountingDecoder(50 * Rate) { Silent = true };
            _engine.MixTo(b, Grid());

            _engine.Transitions = false;
            PlayFor(37.5);

            Assert.That(b.IsDisposed, Is.True);
            Assert.That(_tookOver, Is.Empty);
            Assert.That(_ended, Is.EqualTo(new[] { trackA }));
        }

        [Test]
        public void TurnedOff_ATransitionThatStarted_GoesOn()
        {
            _engine.Open(_a, Grid());
            _engine.Play();
            PlayFor(23);
            var trackB = _engine.MixTo(new CountingDecoder(50 * Rate) { Silent = true }, Grid());
            PlayFor(2);

            _engine.Transitions = false;
            PlayFor(32);

            Assert.That(_tookOver, Is.EqualTo(new[] { trackB }));
        }

        [Test]
        public void TurnedOnAgain_TheTransitionIsDueAgain()
        {
            _engine.Open(_a, Grid());
            _engine.Play();
            PlayFor(23);
            _engine.MixTo(new CountingDecoder(50 * Rate) { Silent = true }, Grid());
            _engine.Transitions = false;

            _engine.Transitions = true;
            PlayFor(0.1);

            Assert.That(_due, Has.Count.EqualTo(2));
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
