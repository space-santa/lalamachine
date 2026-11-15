using System;
using System.Linq;
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

        [TestCase(5, true, true)]
        [TestCase(4, false, true)]
        [TestCase(3, false, true)]
        [TestCase(2, false, true)]
        [TestCase(1, false, false)]
        [TestCase(6, false, false)]
        public void WhatAVersionBeforeFound_IsFoundAgain_AndPlayedByUntilThen(int version, bool isCurrent, bool isPlayable)
        {
            Assert.That(BeatGridAnalyzer.Version, Is.EqualTo(5));
            Assert.That(BeatGridAnalyzer.IsCurrent(version), Is.EqualTo(isCurrent));
            Assert.That(BeatGridAnalyzer.IsPlayable(version), Is.EqualTo(isPlayable));
        }

        // Version 4 only moves the beats where the cues say, version 5 only adds intros: otherwise it finds what version 4
        // and 3 found, and what version 2 found with a tagged tempo. The tempo too, which shows for a track without a tagged
        // one.
        [TestCase(4, false, true, true)]
        [TestCase(3, false, true, true)]
        [TestCase(3, true, true, true)]
        [TestCase(2, true, true, false)]
        [TestCase(2, false, false, false)]
        [TestCase(1, true, false, false)]
        [TestCase(5, false, false, true)]
        [TestCase(6, true, false, false)]
        public void WhatAVersionBeforeFound_IsWhatThisOneFinds_UnlessItMayChange(int version, bool hasTaggedTempo, bool isCurrentUnlessChanged, bool hasCurrentTempo)
        {
            Assert.That(BeatGridAnalyzer.IsCurrentUnlessChanged(version, hasTaggedTempo), Is.EqualTo(isCurrentUnlessChanged));
            Assert.That(BeatGridAnalyzer.HasCurrentTempo(version), Is.EqualTo(hasCurrentTempo));
        }

        // 128 BPM, a bar is 1.875 s: the music 16 bars in starts at 30.25 s.
        [TestCase(30.25, 2, true)]
        [TestCase(30.25, 30, false)]
        [TestCase(28.3, 2, false)]
        public void AGrid_MayChange_WhenItsSteadyMusicStarts16BarsInOrLater(double musicStart, double deviationMs, bool mayChange)
        {
            var grid = new BeatGrid(128, 0.25, deviationMs, 256, 0.25, musicStart, 119);

            Assert.That(BeatGridAnalyzer.MayChange(grid, []), Is.EqualTo(mayChange));
        }

        [Test]
        public void CuesMayMoveTheBeats_OnlyWhenMostOfThemAreOffTheBeats()
        {
            var beat = 60 / 128.0;
            var grid = new BeatGrid(128, 0.25, 1, 256, 0.25, 0.25, 119);
            double[] onTheBeats = [0.25 + (4 * beat) + 0.045, 0.25 + (36 * beat) + 0.045, 0.25 + (68 * beat) + 0.045];
            double[] onTheOffBeats = [.. onTheBeats.Select(cue => cue + (beat / 2))];

            Assert.That(BeatGridAnalyzer.CuesMayMove(grid, onTheBeats), Is.False);
            Assert.That(BeatGridAnalyzer.CuesMayMove(grid, onTheOffBeats), Is.True);
            Assert.That(BeatGridAnalyzer.CuesMayMove(grid, [onTheOffBeats[0], onTheBeats[1], onTheBeats[2]]), Is.False);
            Assert.That(BeatGridAnalyzer.CuesMayMove(grid, [onTheOffBeats[0]]), Is.False, "one cue alone");
            Assert.That(BeatGridAnalyzer.CuesMayMove(grid, []), Is.False);
        }

        // On the first beat of bars, a little late, like Mixed In Key's: on the kick, or that many beats after it.
        private static double[] Cues(double after = 0)
        {
            var beat = 60 / 128.0;
            return [0.25 + ((4 + after) * beat) + 0.045, 0.25 + ((36 + after) * beat) + 0.045, 0.25 + ((68 + after) * beat) + 0.045];
        }

        // A bass note with a hi-hat is in the bass and the click range like a kick: on each off-beat, or on a 16th. Without
        // its click, the kick is less like one than a bass note right after it.
        [TestCase(0.5, 0.0, true)]
        [TestCase(0.75, 0.1, true)]
        [TestCase(0.25, 0.1, false)]
        public void Cues_PutTheBeatOnTheKick_WhenABassNoteSoundsLikeOne(double bassNoteAt, double sixteenthHiHat, bool hasClick)
        {
            var beat = 60 / 128.0;
            DrumDecoder Drums() => new(128, 0.25) { BassNoteAt = bassNoteAt, SixteenthHiHat = sixteenthHiHat, HasClick = hasClick };
            using var withoutCues = Drums();
            using var decoder = Drums();

            var guessed = BeatGridAnalyzer.Analyze(withoutCues, taggedBpm: null)!;
            var grid = BeatGridAnalyzer.Analyze(decoder, taggedBpm: null, Cues())!;

            Assert.That(OffBeat(guessed.FirstBeat - 0.25, beat), Is.EqualTo(bassNoteAt).Within(0.02), "on the bass note, without the cues");
            Assert.That(grid.FirstBeat, Is.EqualTo(0.25).Within(0.003));
            Assert.That(grid.BeatsPerMinute, Is.EqualTo(128).Within(0.005));
            Assert.That(grid.FirstDownbeat, Is.EqualTo(0.25).Within(0.003));
            Assert.That(grid.IsSteady, Is.True);
        }

        // Like a kick with other sounds around it: by the kicks the beats would be less steady than by the bass notes. The
        // drums end with the bass note after the kick at 59.78 s.
        [TestCase(0.5, 0.0, true)]
        [TestCase(0.25, 0.1, false)]
        public void Cues_MoveTheBeatsAsTheyWere_WhenTheKicksAreLessSteady(double bassNoteAt, double sixteenthHiHat, bool hasClick)
        {
            var beat = 60 / 128.0;
            DrumDecoder Drums() => new(128, 0.25, seconds: 70) { BassNoteAt = bassNoteAt, SixteenthHiHat = sixteenthHiHat, HasClick = hasClick, KickDriftMs = 20, BeatsUntil = 60.1 };
            using var withoutCues = Drums();
            using var decoder = Drums();

            var guessed = BeatGridAnalyzer.Analyze(withoutCues, taggedBpm: null)!;
            var grid = BeatGridAnalyzer.Analyze(decoder, taggedBpm: null, Cues())!;

            Assert.That(OffBeat(guessed.FirstBeat - 0.25, beat), Is.EqualTo(bassNoteAt).Within(0.02), "on the bass note, without the cues");
            Assert.That(OffBeat(grid.FirstBeat - 0.25, guessed.BeatSeconds), Is.EqualTo(0).Within(0.02).Or.EqualTo(1).Within(0.02));
            Assert.That(OffBeat(grid.FirstBeat - guessed.FirstBeat, guessed.BeatSeconds), Is.EqualTo(1 - bassNoteAt).Within(0.0001));
            Assert.That((grid.DeviationMs, grid.BeatCount), Is.EqualTo((guessed.DeviationMs, guessed.BeatCount)));
            Assert.That(grid.IsSteady, Is.True);
            // The last kick, not one after the music.
            Assert.That(grid.MusicEnd, Is.EqualTo(0.25 + (127 * beat)).Within(0.03));
        }

        // A band's: by the kicks the beats would be even less steady than by the off-beats, and heard less often. Where the
        // music starts and ends stays where it was heard.
        [Test]
        public void Cues_MoveUnsteadyBeatsAsTheyWere_WhenTheKicksAreLessSteadyStill()
        {
            var beat = 60 / 128.0;
            DrumDecoder Drums() => new(128, 0.25, seconds: 110, driftMs: 15) { BassNoteAt = 0.5, KickDriftMs = 20, BeatsFrom = 10, BeatsUntil = 100 };
            using var withoutCues = Drums();
            using var decoder = Drums();

            var guessed = BeatGridAnalyzer.Analyze(withoutCues, taggedBpm: null)!;
            var grid = BeatGridAnalyzer.Analyze(decoder, taggedBpm: null, Cues())!;

            Assert.That(guessed.IsSteady, Is.False);
            Assert.That(OffBeat(guessed.FirstBeat - 0.25, beat), Is.EqualTo(0.5).Within(0.02), "the off-beat, without the cues");
            Assert.That(OffBeat(grid.FirstBeat - 0.25, beat), Is.EqualTo(0).Within(0.02).Or.EqualTo(1).Within(0.02));
            Assert.That((grid.DeviationMs, grid.BeatCount), Is.EqualTo((guessed.DeviationMs, guessed.BeatCount)));
            Assert.That(grid.MusicStart, Is.EqualTo(guessed.MusicStart - (guessed.BeatSeconds / 2)).Within(0.0001));
            Assert.That(grid.MusicEnd, Is.EqualTo(guessed.MusicEnd - (guessed.BeatSeconds / 2)).Within(0.0001));
        }

        // A quarter of a beat after the kicks or three: between them and the hi-hats, or on hi-hats on 16ths there. Those
        // are heard, but no kick.
        [TestCase(0.25, 0.0, true)]
        [TestCase(0.25, 0.1, true)]
        [TestCase(0.75, 0.1, true)]
        [TestCase(0.25, 0.1, false)]
        [TestCase(0.75, 0.1, false)]
        public void CuesWhereNoKickIsHeard_LeaveTheBeat(double cuesAfter, double sixteenthHiHat, bool hasClick)
        {
            using var decoder = new DrumDecoder(128, 0.25) { SixteenthHiHat = sixteenthHiHat, HasClick = hasClick };

            var grid = BeatGridAnalyzer.Analyze(decoder, taggedBpm: null, Cues(cuesAfter))!;

            Assert.That(grid.FirstBeat, Is.EqualTo(0.25).Within(0.003));
            Assert.That(grid.IsSteady, Is.True);
        }

        [Test]
        public void CuesThatDontAgreeOnTheBeat_LeaveIt()
        {
            // One on the beat, one on the off-beat, one in between.
            double[] cues = [Cues()[0], Cues(0.5)[1], Cues(0.3)[2]];
            using var withoutCues = new DrumDecoder(128, 0.25) { BassNoteAt = 0.5 };
            using var decoder = new DrumDecoder(128, 0.25) { BassNoteAt = 0.5 };

            var guessed = BeatGridAnalyzer.Analyze(withoutCues, taggedBpm: null)!;
            var grid = BeatGridAnalyzer.Analyze(decoder, taggedBpm: null, cues)!;

            Assert.That(grid.FirstBeat, Is.EqualTo(guessed.FirstBeat));
        }

        // How far into a beat the seconds are, from 0 to 1.
        private static double OffBeat(double seconds, double beat)
        {
            var beats = seconds / beat;
            return beats - Math.Floor(beats);
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
        public void MusicFromTheStart_HasNoIntro()
        {
            using var decoder = new DrumDecoder(128, 0.25, seconds: 120) { BeatsFrom = 10, BeatsUntil = 100 };

            Assert.That(BeatGridAnalyzer.Analyze(decoder, taggedBpm: null)!.Intro, Is.Null);
        }

        // 128 BPM from 0.25 s, the intro's drums until 40 s and the rest's from 70 s, longer: the grid is the rest's.
        private static DrumDecoder IntroAndBreak(double beatsLater, double introDrums = 1, double introUntil = 40) =>
            new(128, 0.25, seconds: 150) { BreakFrom = introUntil, BreakUntil = 70, BeatsLaterAfterBreak = beatsLater, DrumsBeforeBreak = introDrums };

        // On the first beat of bars like Mixed In Key's: two in the intro, two in the rest.
        private static double[] IntroAndBreakCues(double beatsLater)
        {
            var beat = 60 / 128.0;
            return [0.25 + 0.045, 0.25 + (32 * beat) + 0.045, 0.25 + ((160 + beatsLater) * beat) + 0.045, 0.25 + ((192 + beatsLater) * beat) + 0.045];
        }

        [Test]
        public void AnIntroOnABeatOfItsOwn_HasAGridOfItsOwn_WhereTheCuesSaySo()
        {
            var beat = 60 / 128.0;
            using var decoder = IntroAndBreak(beatsLater: 0.3);

            var grid = BeatGridAnalyzer.Analyze(decoder, taggedBpm: 128, IntroAndBreakCues(0.3))!;

            Assert.That(OffBeat(grid.FirstBeat - 0.25, beat), Is.EqualTo(0.3).Within(0.01));
            Assert.That(grid.MusicStart, Is.EqualTo(70).Within(beat));
            Assert.That(grid.Intro, Is.Not.Null);
            var intro = grid.Intro!;
            Assert.That(intro.IsSteady, Is.True);
            Assert.That(intro.BeatsPerMinute, Is.EqualTo(128).Within(0.01));
            Assert.That(intro.FirstBeat, Is.EqualTo(0.25).Within(0.003));
            Assert.That(intro.FirstDownbeat, Is.EqualTo(0.25).Within(0.003));
            Assert.That(intro.MusicStart, Is.EqualTo(0.25).Within(beat));
            Assert.That(intro.MusicEnd, Is.EqualTo(40).Within(beat));
            Assert.That(intro.Intro, Is.Null);
        }

        // Found a part of a beat off the rest, an intro is more often off its kick than on a beat of its own.
        [Test]
        public void AnIntroOnABeatOfItsOwn_WithoutCuesThatSaySo_HasNone()
        {
            using var decoder = IntroAndBreak(beatsLater: 0.3);

            var grid = BeatGridAnalyzer.Analyze(decoder, taggedBpm: 128)!;

            Assert.That(grid.MusicStart, Is.EqualTo(70).Within(60 / 128.0));
            Assert.That(grid.Intro, Is.Null);
        }

        [Test]
        public void AnIntroOfFewerThan16Bars_HasNoGridOfItsOwn()
        {
            using var decoder = IntroAndBreak(beatsLater: 0.3, introUntil: 28);

            var grid = BeatGridAnalyzer.Analyze(decoder, taggedBpm: 128, IntroAndBreakCues(0.3))!;

            Assert.That(grid.Intro, Is.Null);
        }

        // Its drums hardly louder than the noise: by how they stand out of the louder rest, they aren't heard.
        [Test]
        public void AQuietIntro_OnTheBeatOfTheRest_HasAGridOfItsOwn()
        {
            var beat = 60 / 128.0;
            using var decoder = IntroAndBreak(beatsLater: 0, introDrums: 0.02);

            var grid = BeatGridAnalyzer.Analyze(decoder, taggedBpm: 128)!;

            Assert.That(grid.MusicStart, Is.EqualTo(70).Within(4 * beat), "not heard on the grid of the rest");
            Assert.That(grid.Intro, Is.Not.Null);
            Assert.That(OffBeat(grid.Intro!.FirstBeat - 0.25, beat), Is.EqualTo(0).Within(0.02).Or.EqualTo(1).Within(0.02));
            Assert.That(grid.Intro.MusicStart, Is.EqualTo(0.25).Within(beat));
            Assert.That(grid.Intro.MusicEnd, Is.EqualTo(40).Within(4 * beat));
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
