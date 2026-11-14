using System;
using LibLala.Audio;
using LibLala.Audio.Mixing;
using NUnit.Framework;

namespace LibLala.Unittests.Audio.Mixing
{
    public class TransitionPlannerTest
    {
        // 120 BPM: a bar is 2 s. The music from 4 s to the last beat at 299.5 s, bars from 0 s.
        private static BeatGrid Grid(double bpm = 120, double deviationMs = 2, double firstDownbeat = 0, double musicStart = 4, double musicEnd = 299.5) =>
            new(bpm, firstDownbeat % (60 / bpm), deviationMs, 500, firstDownbeat, musicStart, musicEnd);

        [Test]
        public void SteadyBeats_GoOverIn16Bars_FromTheLast16OfTheMusic()
        {
            var plan = TransitionPlanner.Plan(nowA: 100, lengthA: 310, Grid(), tempoA: 1, Grid(bpm: 124, firstDownbeat: 0.5, musicStart: 8.6));

            Assert.That(plan.Kind, Is.EqualTo(TransitionKind.BeatMatched));
            // The bar after the last beat starts at 300 s.
            Assert.That(plan.StartA, Is.EqualTo(300 - 32).Within(1e-9));
            Assert.That(plan.Seconds, Is.EqualTo(32 / plan.TempoA).Within(1e-9));
            // B's bars start at 0.5 s, every 60/124*4 s; its music in the bar from 8.24 s.
            Assert.That(plan.StartB, Is.EqualTo(0.5 + (4 * 4 * 60 / 124.0)).Within(1e-9));
        }

        [Test]
        public void BothTracks_GoHalfwayToTheOthersTempo()
        {
            var plan = TransitionPlanner.Plan(nowA: 100, lengthA: 310, Grid(), tempoA: 1, Grid(bpm: 124));

            // Both at 121.98 BPM, A 1.65% faster and B 1.63% slower.
            Assert.That(plan.TempoA, Is.EqualTo(Math.Sqrt(124 / 120.0)).Within(1e-9));
            Assert.That(plan.TempoB, Is.EqualTo(Math.Sqrt(120 / 124.0)).Within(1e-9));
            Assert.That(120 * plan.TempoA, Is.EqualTo(124 * plan.TempoB).Within(1e-9));
        }

        [Test]
        public void TheCurrentTrack_GoesToItsTempo_InThe8BarsBeforeTheTransition()
        {
            var plan = TransitionPlanner.Plan(nowA: 100, lengthA: 310, Grid(), tempoA: 1, Grid(bpm: 124));

            Assert.That(plan.RampA, Is.EqualTo(new TempoRamp(268 - 16, 268, 1, plan.TempoA)));
            Assert.That(plan.RampA.At(260), Is.EqualTo((1 + plan.TempoA) / 2).Within(1e-9));
        }

        // 10% apart, each 4.9% off its own tempo. 11%, each would be 5.4%.
        [TestCase(132, true)]
        [TestCase(120 * 1.11, false)]
        [TestCase(120 / 1.1, true)]
        [TestCase(120 / 1.11, false)]
        public void TemposUpTo10PercentApart_AreBeatMatched(double bpmB, bool isBeatMatched)
        {
            var plan = TransitionPlanner.Plan(nowA: 100, lengthA: 310, Grid(), tempoA: 1, Grid(bpm: bpmB));

            Assert.That(plan.Kind, Is.EqualTo(isBeatMatched ? TransitionKind.BeatMatched : TransitionKind.Cut));
        }

        [Test]
        public void LateForSomeOfTheBarsBefore_TheCurrentTrackGoesToItsTempoInTheWholeBarsLeft()
        {
            var plan = TransitionPlanner.Plan(nowA: 0, lengthA: 310, Grid(), tempoA: 1, Grid(bpm: 124), rampFromA: 261);

            Assert.That(plan.StartA, Is.EqualTo(268).Within(1e-9));
            Assert.That(plan.RampA, Is.EqualTo(new TempoRamp(262, 268, 1, Math.Sqrt(124 / 120.0))));
        }

        [Test]
        public void WithoutABarBefore_TheIncomingTrackGoesAllTheWayToTheCurrentOnesTempo()
        {
            var plan = TransitionPlanner.Plan(nowA: 0, lengthA: 310, Grid(), tempoA: 1, Grid(bpm: 124), rampFromA: 267);

            Assert.That(plan.Kind, Is.EqualTo(TransitionKind.BeatMatched));
            Assert.That(plan.StartA, Is.EqualTo(268).Within(1e-9));
            Assert.That(plan.RampA, Is.EqualTo(TempoRamp.None(268, 1)));
            Assert.That(plan.TempoB, Is.EqualTo(120 / 124.0).Within(1e-9));
            Assert.That(plan.Seconds, Is.EqualTo(32).Within(1e-9));
        }

        [Test]
        public void WithoutABarBefore_TemposMoreThan5PercentApart_Cut()
        {
            var plan = TransitionPlanner.Plan(nowA: 0, lengthA: 310, Grid(), tempoA: 1, Grid(bpm: 128), rampFromA: 267);

            AssertCut(plan, 300, 480.0 / 128);
        }

        [Test]
        public void ALastHitOnTheFirstBeatOfABar_EndsTheMusicThere()
        {
            var plan = TransitionPlanner.Plan(nowA: 100, lengthA: 310, Grid(musicEnd: 300), tempoA: 1, Grid());

            Assert.That(plan.StartA, Is.EqualTo(300 - 32).Within(1e-9));
        }

        [Test]
        public void WhenTheBarAfterTheLastBeatIsntInTheFile_TheTransitionStillStartsOnTheFirstBeatOfABar()
        {
            // Bars from 0.005 s at 125 BPM, the last beat at 367.7 s, the file over at 368 s: the bar after the last beat
            // would be at 369.605 s.
            var grid = new BeatGrid(125, 0.005, 2, 500, 0.005, 0.5, 367.7);

            var plan = TransitionPlanner.Plan(nowA: 100, lengthA: 368, grid, tempoA: 1, grid);

            var bars = (plan.StartA - 0.005) / grid.BarSeconds;
            Assert.That(bars, Is.EqualTo(Math.Round(bars)).Within(1e-9));
            Assert.That(plan.StartA + plan.Seconds, Is.LessThanOrEqualTo(368));
        }

        [Test]
        public void ACrossfade_IsOverWhereTheMusicEnds_NotAfterItsTail()
        {
            // The last beat at 299.5 s, the file goes on to 310 s.
            var plan = TransitionPlanner.Plan(nowA: 100, lengthA: 310, Grid(), tempoA: 1, gridB: null);

            Assert.That(plan.StartA + plan.Seconds, Is.EqualTo(300).Within(1e-9));
        }

        [Test]
        public void AStretchedTrack_GoesHalfwayFromItsTempo()
        {
            var plan = TransitionPlanner.Plan(nowA: 100, lengthA: 310, Grid(), tempoA: 1.02, Grid(bpm: 124));

            Assert.That(plan.RampA.From, Is.EqualTo(1.02));
            Assert.That(plan.TempoA, Is.EqualTo(Math.Sqrt(124 / 120.0)).Within(1e-9));
            Assert.That(plan.TempoB, Is.EqualTo(Math.Sqrt(120 / 124.0)).Within(1e-9));
        }

        [Test]
        public void AStretchedTrack_WithoutABarBefore_GoesOverAtItsTempo()
        {
            var plan = TransitionPlanner.Plan(nowA: 0, lengthA: 310, Grid(), tempoA: 1.02, Grid(bpm: 124), rampFromA: 267);

            Assert.That(plan.TempoA, Is.EqualTo(1.02));
            Assert.That(plan.TempoB, Is.EqualTo(120 * 1.02 / 124).Within(1e-9));
            Assert.That(plan.Seconds, Is.EqualTo(32 / 1.02).Within(1e-9));
        }

        [Test]
        public void TooLate_ItGoesOverInTheBarsLeft()
        {
            var plan = TransitionPlanner.Plan(nowA: 285.3, lengthA: 310, Grid(), tempoA: 1, Grid());

            Assert.That(plan.StartA, Is.EqualTo(286));
            Assert.That(plan.Seconds, Is.EqualTo(14));
            Assert.That(plan.RampA, Is.EqualTo(TempoRamp.None(286, 1)));
        }

        // B's first bar on the downbeat at time A's music is over, at its own tempo.
        private static void AssertCut(TransitionPlan plan, double downbeatA, double firstBarB)
        {
            Assert.That(plan.Kind, Is.EqualTo(TransitionKind.Cut));
            Assert.That(plan.TempoB, Is.EqualTo(1));
            Assert.That(plan.Seconds, Is.EqualTo(TransitionPlanner.CutSeconds));
            var lead = downbeatA - plan.StartA;
            Assert.That(lead, Is.GreaterThanOrEqualTo(0).And.LessThanOrEqualTo(TransitionPlanner.CutSeconds + 1e-9));
            Assert.That(plan.StartB + lead, Is.EqualTo(firstBarB).Within(1e-9));
        }

        // B's bars from 0 s, its music from 4 s: at 140 BPM in the bar from 3.43 s, at 100 BPM in the one from 2.4 s.
        [TestCase(140, 480.0 / 140)]
        [TestCase(100, 2.4)]
        // 11% faster, like 128 BPM into 142: each would be 5.4% off its own.
        [TestCase(120 * 142 / 128.0, 480 / (120 * 142 / 128.0))]
        public void TemposTooFarApart_CutOnTheDownbeatWhereTheMusicEnds(double bpmB, double firstBarB)
        {
            var plan = TransitionPlanner.Plan(nowA: 100, lengthA: 310, Grid(), tempoA: 1, Grid(bpm: bpmB));

            AssertCut(plan, 300, firstBarB);
            Assert.That(plan.StartA, Is.EqualTo(300 - TransitionPlanner.CutSeconds).Within(1e-9));
            Assert.That(plan.RampA, Is.EqualTo(TempoRamp.None(plan.StartA, 1)));
        }

        [Test]
        public void TooLateForFourBars_ItCutsOnTheDownbeatWhereTheMusicEnds()
        {
            var plan = TransitionPlanner.Plan(nowA: 295, lengthA: 310, Grid(), tempoA: 1, Grid());

            AssertCut(plan, 300, 4);
        }

        [Test]
        public void PastTheEndOfTheMusic_ItCutsOnTheNextDownbeat()
        {
            var plan = TransitionPlanner.Plan(nowA: 301, lengthA: 310, Grid(), tempoA: 1, Grid(bpm: 140));

            AssertCut(plan, 302, 480.0 / 140);
        }

        [Test]
        public void WithoutADownbeatLeftInTheFile_ItCutsAtItsEnd()
        {
            var plan = TransitionPlanner.Plan(nowA: 308.5, lengthA: 309.5, Grid(), tempoA: 1, Grid(bpm: 140));

            AssertCut(plan, 309.5, 480.0 / 140);
        }

        [Test]
        public void ACutToATrackThatStartsOnItsFirstBeat_StartsItThere()
        {
            var plan = TransitionPlanner.Plan(nowA: 100, lengthA: 310, Grid(), tempoA: 1, Grid(bpm: 140, musicStart: 0));

            AssertCut(plan, 300, 0);
            Assert.That(plan.StartA, Is.EqualTo(300));
        }

        [Test]
        public void ADriftingBeat_Crossfades()
        {
            var plan = TransitionPlanner.Plan(nowA: 100, lengthA: 310, Grid(deviationMs: 30), tempoA: 1, Grid());

            Assert.That(plan.Kind, Is.EqualTo(TransitionKind.Crossfade));
            Assert.That(plan.StartA, Is.EqualTo(300 - TransitionPlanner.CrossfadeSeconds));
            Assert.That(plan.Seconds, Is.EqualTo(TransitionPlanner.CrossfadeSeconds));
            Assert.That(plan.StartB, Is.Zero);
            Assert.That(plan.TempoB, Is.EqualTo(1));
        }

        [Test]
        public void WithoutGrid_ItCrossfades()
        {
            var plan = TransitionPlanner.Plan(nowA: 100, lengthA: 310, Grid(), tempoA: 1, gridB: null);

            Assert.That(plan.Kind, Is.EqualTo(TransitionKind.Crossfade));
        }

        [Test]
        public void TheEarliestStart_IsWhereTheCurrentTrackGoesToItsTempoForABeatMatchedTransition()
        {
            Assert.That(TransitionPlanner.EarliestStart(310, Grid()), Is.EqualTo(268 - 16).Within(1e-9));
            Assert.That(TransitionPlanner.EarliestStart(310, null), Is.EqualTo(306));
            Assert.That(TransitionPlanner.EarliestStart(310, Grid(deviationMs: 30)), Is.EqualTo(296));
        }
    }
}
