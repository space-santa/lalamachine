using System;
using LibLala.Audio;
using LibLala.Audio.Analysis;
using NUnit.Framework;

namespace LibLala.Unittests.Audio
{
    public sealed class BeatGridAnalyzerTest
    {
        [TestCase(128, 0.25)]
        [TestCase(127.3, 0.31)]
        [TestCase(150, 0.1)]
        [TestCase(93.5, 0.6)]
        public void ASteadyBeat_IsFoundToTheMillisecond(double bpm, double firstBeat)
        {
            using var decoder = new DrumDecoder(bpm, firstBeat);

            var grid = BeatGridAnalyzer.Analyze(decoder, taggedBpm: null);

            Assert.That(grid, Is.Not.Null);
            Assert.That(grid!.BeatsPerMinute, Is.EqualTo(bpm).Within(0.005));
            Assert.That(grid.FirstBeat, Is.EqualTo(firstBeat).Within(0.003));
            Assert.That(grid.IsSteady, Is.True);
        }

        [Test]
        public void TheBeat_IsOnTheKick_NotOnTheLouderHiHat()
        {
            // The hi-hats on the off-beats make more onset than the kicks. Placed by all onsets, the grid would be half a beat off.
            using var decoder = new DrumDecoder(128, 0.25);

            var grid = BeatGridAnalyzer.Analyze(decoder, taggedBpm: null)!;

            var beat = 60 / 128.0;
            var offBy = (grid.FirstBeat - 0.25) / beat;
            Assert.That(offBy - Math.Round(offBy), Is.EqualTo(0).Within(0.02));
        }

        [Test]
        public void AKickWithoutClick_IsFoundByTheBass()
        {
            using var decoder = new DrumDecoder(128, 0.25) { HasClick = false };

            var grid = BeatGridAnalyzer.Analyze(decoder, taggedBpm: null)!;

            var offBy = (grid.FirstBeat - 0.25) / (60 / 128.0);
            Assert.That(offBy - Math.Round(offBy), Is.EqualTo(0).Within(0.02));
        }

        // Where Mixed In Key has the tempo of nearly every track: drum and bass in half time, a slow track in double time.
        [TestCase(174, 87)]
        [TestCase(165, 82.5)]
        [TestCase(70, 140)]
        public void WithoutATag_TheTempoIsTheOneBetween80And160(double played, double counted)
        {
            using var decoder = new DrumDecoder(played, 0.1);

            var grid = BeatGridAnalyzer.Analyze(decoder, taggedBpm: null)!;

            Assert.That(grid.BeatsPerMinute, Is.EqualTo(counted).Within(0.005));
            Assert.That(grid.IsSteady, Is.True);
        }

        [TestCase(2, true, true)]
        [TestCase(2, false, false)]
        [TestCase(3, false, true)]
        [TestCase(1, true, false)]
        public void WhatTheVersionBeforeFound_IsStillRight_ForATrackWithATaggedTempo(int version, bool hasTaggedTempo, bool isCurrent)
        {
            Assert.That(BeatGridAnalyzer.Version, Is.EqualTo(3));
            Assert.That(BeatGridAnalyzer.IsCurrent(version, hasTaggedTempo), Is.EqualTo(isCurrent));
        }

        [Test]
        public void ATag_DecidesBetweenHalfAndDoubleTempo()
        {
            // Kicks on every beat and hi-hats between them repeat at 87 and at 174 BPM.
            using var tagged87 = new DrumDecoder(87, 0.1);
            using var tagged174 = new DrumDecoder(87, 0.1);

            var at87 = BeatGridAnalyzer.Analyze(tagged87, taggedBpm: 87)!;
            var at174 = BeatGridAnalyzer.Analyze(tagged174, taggedBpm: 174)!;

            Assert.That(at87.BeatsPerMinute, Is.EqualTo(87).Within(0.005));
            Assert.That(at174.BeatsPerMinute, Is.EqualTo(174).Within(0.005));
        }

        [Test]
        public void AWrongTag_IsOverruledByAClearBeat()
        {
            using var decoder = new DrumDecoder(128, 0.25);

            var grid = BeatGridAnalyzer.Analyze(decoder, taggedBpm: 96)!;

            Assert.That(grid.BeatsPerMinute, Is.EqualTo(128).Within(0.005));
        }

        [Test]
        public void ADriftingBeat_IsntSteady()
        {
            // A band 25 ms ahead and behind.
            using var decoder = new DrumDecoder(120, 0.2, driftMs: 25);

            var grid = BeatGridAnalyzer.Analyze(decoder, taggedBpm: null);

            Assert.That(grid, Is.Not.Null);
            Assert.That(grid!.BeatsPerMinute, Is.EqualTo(120).Within(0.5));
            Assert.That(grid.IsSteady, Is.False);
        }

        [Test]
        public void ASlightlyDriftingBeat_IsSteady()
        {
            using var decoder = new DrumDecoder(120, 0.2, driftMs: 2);

            var grid = BeatGridAnalyzer.Analyze(decoder, taggedBpm: null)!;

            Assert.That(grid.IsSteady, Is.True);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(3)]
        public void TheFirstBeatOfABar_IsWhereTheSoundChanges(int downbeat)
        {
            using var decoder = new DrumDecoder(128, 0.25) { Downbeat = downbeat };

            var grid = BeatGridAnalyzer.Analyze(decoder, taggedBpm: null)!;

            Assert.That(grid.FirstDownbeat, Is.EqualTo(0.25 + (downbeat * 60 / 128.0)).Within(0.003));
        }

        [Test]
        public void Cues_DecideTheFirstBeatOfABar()
        {
            using var decoder = new DrumDecoder(128, 0.25) { Downbeat = 0 };
            var beat = 60 / 128.0;
            // On the third beat of bars, a few ms late, like Mixed In Key's.
            double[] cues = [0.25 + (2 * beat) + 0.045, 0.25 + (34 * beat) + 0.045, 0.25 + (66 * beat) + 0.045];

            var grid = BeatGridAnalyzer.Analyze(decoder, taggedBpm: null, cues)!;

            Assert.That(grid.FirstDownbeat, Is.EqualTo(0.25 + (2 * beat)).Within(0.003));
        }

        [Test]
        public void CuesThatDisagree_AreLeftOut()
        {
            using var decoder = new DrumDecoder(128, 0.25) { Downbeat = 0 };
            var beat = 60 / 128.0;
            double[] cues = [0.25 + (2 * beat), 0.25 + (5 * beat), 0.25 + (11 * beat)];

            var grid = BeatGridAnalyzer.Analyze(decoder, taggedBpm: null, cues)!;

            Assert.That(grid.FirstDownbeat, Is.EqualTo(0.25).Within(0.003));
        }

        [Test]
        public void TheMusic_IsWhereTheBeatsAre()
        {
            using var decoder = new DrumDecoder(128, 0.25, seconds: 120) { BeatsFrom = 10, BeatsUntil = 100 };

            var grid = BeatGridAnalyzer.Analyze(decoder, taggedBpm: null)!;

            Assert.That(grid.MusicStart, Is.EqualTo(10).Within(60 / 128.0));
            Assert.That(grid.MusicEnd, Is.EqualTo(100).Within(60 / 128.0));
        }

        [Test]
        public void Silence_HasNoGrid()
        {
            using var decoder = new CountingDecoder(30 * AudioFormat.SampleRate) { Silent = true };

            Assert.That(BeatGridAnalyzer.Analyze(decoder, taggedBpm: 128), Is.Null);
        }

        [Test]
        public void ATooShortTrack_HasNoGrid()
        {
            using var decoder = new DrumDecoder(128, 0.25, seconds: 2);

            Assert.That(BeatGridAnalyzer.Analyze(decoder, taggedBpm: null), Is.Null);
        }

        [Test]
        public void NoiseWithoutABeat_IsntSteady()
        {
            using var decoder = new DrumDecoder(128, 0.25) { HasKick = false, HasHiHat = false, Noise = 0.3 };

            var grid = BeatGridAnalyzer.Analyze(decoder, taggedBpm: null);

            Assert.That(grid?.IsSteady ?? false, Is.False);
        }
    }
}
