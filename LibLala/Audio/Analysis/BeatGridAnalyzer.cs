using System;
using System.Collections.Generic;
using System.Linq;

namespace LibLala.Audio.Analysis
{
    // Finds the beat grid of a track: the tempo from how the onsets repeat, then the beats themselves, and a line through
    // them for a tempo to a few thousandths of a BPM.
    public static class BeatGridAnalyzer
    {
        // Goes up when the analysis changes, so that the library is analyzed again.
        public const int Version = 5;
        // The grids from this version on are good to play by until the track is analyzed again. Version 3 found the tempo
        // of a track without a tagged one in another way, version 4 moves the beats where Mixed In Key's cues say, version
        // 5 finds a grid of its own for an intro.
        private const int PlayableSinceVersion = 2;
        // What version 3 found, and version 2 in a track with a tagged tempo, is what this version finds unless it may
        // change: see MayChange. Its tempo too.
        private const int SameUnlessChangedSinceVersion = 3;
        private const int SameWithTagUnlessChangedSinceVersion = 2;

        private const double MinBpm = 60;
        private const double MaxBpm = 200;
        // Where the tempo of a track without a tag is looked for, and after how many beats the onsets repeat at it.
        private const double MinCountedBpm = 80;
        private const double MaxCountedBpm = 160;
        private static readonly double[] CountedBeats = [0.5, 1, 2, 4];
        // How far the sharpest fold may be from the counted tempo: lags are 12 ms apart, 3% of a beat at 160 BPM.
        private const double CountedTempoRange = 0.02;
        // A tagged tempo is used unless the onsets repeat a lot more sharply at another.
        private const double TagSharpnessShare = 0.75;
        private const int PhaseBins = 96;
        private const int BeatsPerSection = 16;
        // How sharply kicks have to pile up at one phase to place the beat by them: twice the average.
        private const double MinBeatSharpness = 2;
        // Too few beats in every stretch of 16 to tell: far from steady.
        private const double NoSectionsDeviationMs = 1000;
        // Beats heard in a stretch of 16 for it to be the music, not an intro or outro.
        private const int MusicBeatsPerSection = 12;
        private const double MinCueAgreement = 0.6;
        // Mixed In Key's cues are this long after the kick's attack, in our decoders' time.
        private const double CueLateSeconds = 0.045;
        // How far from a quarter of a beat a cue may be to be on it, in beats.
        private const double CueQuarterTolerance = 0.1;
        // A kick is in the bass: at least this much of the bass onsets at the beats found have to be at the cues for them
        // to move the beats. In a DJ's library it was 0.19 and more for every track whose cues moved its beats, 0.06 and
        // less for hi-hats on 16ths in synthetic drums.
        private const double MinCueBassShare = 0.1;
        // With the music this many bars in or later, the part before it is looked at on its own, for an intro whose beats
        // weren't heard on the grid of the rest. As long as that at least for a grid of its own, to mix in with.
        private const int IntroBars = 16;
        // An intro's beats are the rest's when they're this close to where the rest's grid has them, in beats. When they're
        // further, they're on a beat of their own only if the cues say so: an intro found a quarter of a beat off the rest
        // is more often off its kick than the beat jumps. In a DJ's library, 24 of the 106 steady tracks whose music starts
        // 16 bars in or later have an intro of their own by this, 2 on a beat of their own that the cues confirm. Of the
        // intros found a quarter of a beat off the rest, the cues had 5 on the rest's beats and none on their own.
        private const double IntroBeatTolerance = 0.1;
        // An intro is at the tempo of the rest, as far as the intro's tempo is looked for around it. Not at one the onsets
        // repeat at more sharply, like double or three quarters of it, whose beats are on the rest's now and then. In a DJ's
        // library every intro but one was within 0.12% of the rest's tempo, that one at three quarters of it.
        private const double IntroTempoTolerance = 0.02;

        // The onset envelope peaks at a frame that starts this long before the attack: the frame with the attack 70% of the
        // way in, where its window rises steeply. Measured with synthetic drums.
        private const double OnsetLead = 0.0323;

        // Whether what a version of the analysis found in a track is what this one finds.
        public static bool IsCurrent(int version) => version == Version;

        // Whether a grid that a version of the analysis found is good to play by, until the track is analyzed again.
        public static bool IsPlayable(int version) => version is >= PlayableSinceVersion and <= Version;

        // Whether what a version found is what this one finds, unless it may change: see MayChange.
        public static bool IsCurrentUnlessChanged(int version, bool hasTaggedTempo)
        {
            return version is >= SameUnlessChangedSinceVersion and < Version
                || (hasTaggedTempo && version is >= SameWithTagUnlessChangedSinceVersion and < Version);
        }

        // Whether the tempo that a version found in a track without a tagged one is the tempo this one finds.
        public static bool HasCurrentTempo(int version) => version is >= SameUnlessChangedSinceVersion and <= Version;

        // Whether this version may find another grid than one an earlier version found, see IsCurrentUnlessChanged: when
        // the cues may move its beats, or there may be an intro with a grid of its own. When not, the grid is what this
        // version finds in the track.
        public static bool MayChange(BeatGrid grid, IReadOnlyList<double> cueSeconds)
        {
            ArgumentNullException.ThrowIfNull(grid);
            ArgumentNullException.ThrowIfNull(cueSeconds);
            return CuesMayMove(grid, cueSeconds) || MayHaveIntro(grid);
        }

        internal static bool CuesMayMove(BeatGrid grid, IReadOnlyList<double> cueSeconds) => CueQuarter(cueSeconds, grid.FirstBeat, grid.BeatSeconds) is not (null or 0);

        // Whether the part before the music is looked at for an intro: only before steady beats, to mix in with.
        internal static bool MayHaveIntro(BeatGrid grid) => grid.IsSteady && grid.MusicStartBar >= IntroBars;

        // Null when there is no beat to find.
        public static BeatGrid? Analyze(IAudioDecoder decoder, double? taggedBpm) => Analyze(decoder, taggedBpm, cueSeconds: []);

        // Cues, like Mixed In Key's, are on the first beat of bars. They decide which beat is the first of a bar when most
        // of them agree.
        public static BeatGrid? Analyze(IAudioDecoder decoder, double? taggedBpm, IReadOnlyList<double> cueSeconds) => Analyze(decoder, taggedBpm, cueSeconds, keyAnalyzer: null);

        // The key analyzer hears the track as it is read here, so that a track that needs both is decoded once.
        public static BeatGrid? Analyze(IAudioDecoder decoder, double? taggedBpm, IReadOnlyList<double> cueSeconds, KeyAnalyzer? keyAnalyzer)
        {
            ArgumentNullException.ThrowIfNull(decoder);
            ArgumentNullException.ThrowIfNull(cueSeconds);
            return Analyze(OnsetEnvelope.Compute(decoder, keyAnalyzer), OnsetEnvelope.HopSeconds, taggedBpm, cueSeconds);
        }

        internal static BeatGrid? Analyze(Onsets envelopes, double hopSeconds, double? taggedBpm, IReadOnlyList<double> cueSeconds)
        {
            var grid = Find(envelopes, hopSeconds, taggedBpm, cueSeconds);
            if (grid is null || !MayHaveIntro(grid))
            {
                return grid;
            }

            // The part before the music on its own: the onsets there count by how they stand out of it, not of the louder
            // music after. Up to the music's first beat, at the tempo of the music unless they repeat a lot more sharply at
            // another.
            var end = grid.IntroEnd;
            var introCues = cueSeconds.Where(cue => cue < end).ToList();
            var intro = Find(envelopes.Before((int)(end / hopSeconds)), hopSeconds, grid.BeatsPerMinute, introCues);
            return intro is not null && IsIntroOf(intro, grid, introCues) ? grid with { Intro = intro } : grid;
        }

        // Whether a grid found before the music is an intro's, to mix in with: steady for 16 bars or more, at the tempo of
        // the rest, and on its beats unless the cues say it's on a beat of its own.
        internal static bool IsIntroOf(BeatGrid intro, BeatGrid grid, IReadOnlyList<double> introCues)
        {
            return intro.IsSteady
                && Math.Abs((intro.BeatsPerMinute / grid.BeatsPerMinute) - 1) <= IntroTempoTolerance
                && intro.MusicEndBar - intro.MusicStartBar >= IntroBars
                && (IsOnTheBeatsOf(intro, grid) || CueQuarter(introCues, intro.FirstBeat, intro.BeatSeconds) == 0);
        }

        // Whether the intro's beats are where the grid has beats, at the start of its music and at the end.
        private static bool IsOnTheBeatsOf(BeatGrid intro, BeatGrid grid)
        {
            return new[] { intro.MusicStart, intro.MusicEnd }.All(beat =>
            {
                var beats = (beat - grid.FirstBeat) / grid.BeatSeconds;
                return Math.Abs(beats - Math.Round(beats)) <= IntroBeatTolerance;
            });
        }

        private static BeatGrid? Find(Onsets envelopes, double hopSeconds, double? taggedBpm, IReadOnlyList<double> cueSeconds)
        {
            // The bass counts three times for the tempo.
            var combined = new float[envelopes.All.Length];
            for (var i = 0; i < combined.Length; i++)
            {
                combined[i] = envelopes.All[i] + (2 * envelopes.Bass[i]);
            }
            var onsets = Normalize(combined, hopSeconds);
            var peaks = Peaks(onsets);
            if (onsets.Length == 0 || peaks.Length == 0)
            {
                return null;
            }

            if (Tempo(onsets, peaks, hopSeconds, taggedBpm) is not { } tempo)
            {
                return null;
            }
            var best = Fold(peaks, hopSeconds, tempo, range: 0.0006, step: 0.00004);

            // The beat is where the kick is. All onsets together often pile up higher on the off-beat, where hi-hats are, and
            // the bass alone where a bass line plays between the kicks. A kick is in the bass and the click range at once.
            // Measured against Mixed In Key's cues in a DJ's library: all onsets put 28% of steady tracks half a beat off,
            // the kick 16%. From the raw flux, a moving average taken off loses some kicks.
            var phase = best.Phase;
            var bass = Scale(envelopes.Bass);
            var bassPeaks = Peaks(bass);
            var expectedBeats = onsets.Length * hopSeconds * best.Bpm / 60;
            foreach (var beatPeaks in new[] { Peaks(Kicks(bass, Scale(envelopes.Click))), bassPeaks })
            {
                if (beatPeaks.Length < expectedBeats / 4)
                {
                    continue;
                }
                var beat = Fold(beatPeaks, hopSeconds, best.Bpm, range: 0, step: 1);
                if (beat.Sharpness >= MinBeatSharpness)
                {
                    phase = beat.Phase;
                    break;
                }
            }

            if (Fit(onsets, hopSeconds, best.Bpm, phase) is not { } line)
            {
                return null;
            }

            // Still, a bass line on the off-beat under the hi-hat is in the bass and the click range too, and can be louder
            // than the kick. Mixed In Key's cues are on the kick: when most of them are a quarter or half a beat off the
            // beats, and the bass is heard there too, the beats are where they are. The onsets there place them to the
            // millisecond. Unless those aren't steady and less so than the ones before, or heard less often: other sounds
            // around the kicks. Then the beats before, moved. Without the bass, the cues aren't on a kick: hi-hats on 16ths,
            // say. Nor when the beats before are steady and the onsets at the cues are steady only before their music: an
            // intro on a beat of its own, most of the cues in it, which gets a grid of its own. The bars of the rest are then
            // where the cues in it say, when enough of them agree. Measured against the cues in a DJ's library, this puts 252
            // of 268 steady tracks that were a half or a quarter beat off on the beat, and leaves the others as they were.
            IReadOnlyList<double> musicCues = [];
            if (CueQuarter(cueSeconds, line.First + OnsetLead, line.Period) is { } quarters and not 0)
            {
                var atCues = line.First + (quarters * line.Period / 4);
                var piles = Piles(bassPeaks, hopSeconds, 60 / line.Period);
                if (PileNear(piles, atCues, line.Period) >= MinCueBassShare * PileNear(piles, line.First, line.Period)
                    && Fit(onsets, hopSeconds, 60 / line.Period, atCues) is { } moved)
                {
                    var isSteadier = BeatGrid.IsSteadyOf(moved.DeviationMs, moved.Heard.Count)
                        || (moved.DeviationMs <= line.DeviationMs && moved.Heard.Count >= line.Heard.Count);
                    // Each beat heard moved back to the kick that starts it, keeping its number: the music starts and ends
                    // with the kicks it did.
                    if (!isSteadier)
                    {
                        line = line with { First = atCues - line.Period };
                    }
                    else if (!BeatGrid.IsSteadyOf(line.DeviationMs, line.Heard.Count) || !IsBefore(moved, line))
                    {
                        line = moved;
                    }
                    else
                    {
                        // A beat that jumps by more than half a beat leaves the intro's bars counted apart from the rest's.
                        var musicStart = MusicSeconds(line).Start - (line.Period / 2);
                        musicCues = [.. cueSeconds.Where(cue => cue >= musicStart)];
                    }
                }
            }

            // Numbered from the first beat at or after the start of the track.
            var period = line.Period;
            var firstBeat = line.First + OnsetLead;
            var shift = (int)Math.Floor(firstBeat / period);
            firstBeat -= shift * period;
            var heard = line.Heard.Select(number => number + shift).ToHashSet();
            var (start, end) = MusicSpan(heard);
            var downbeat = Downbeat(musicCues, firstBeat, period) ?? Downbeat(cueSeconds, firstBeat, period) ?? Downbeat(envelopes.Bands, hopSeconds, firstBeat, period);

            return new BeatGrid(60 / period, firstBeat, line.DeviationMs, line.Heard.Count, firstBeat + (downbeat * period), firstBeat + (start * period), firstBeat + (end * period));
        }

        // The tempo that the beats are at, to about a thousandth. Null when the onsets don't repeat at any tempo from 60 to
        // 200 BPM, with a tag or without.
        private static double? Tempo(float[] onsets, (double Index, float Strength)[] peaks, double hopSeconds, double? taggedBpm)
        {
            var repeats = TempoCandidates(onsets, hopSeconds).ToList();
            if (repeats.Count == 0)
            {
                return null;
            }

            if (taggedBpm is not > 0)
            {
                // Without a tag, the tempo that the music is counted in, and the sharpest fold near it.
                return Fold(peaks, hopSeconds, CountedTempo(onsets, hopSeconds), range: CountedTempoRange, step: 0.0005).Bpm;
            }

            // With one, the tagged tempo unless the onsets repeat a lot more sharply at another. Candidates for that: the
            // tempos at which the onsets repeat most, with double and half of them, the autocorrelation often prefers one of
            // those over the tempo the music is counted in. Never none: the tempos it repeats at are from 60 to 200 BPM.
            var candidates = repeats
                .SelectMany(bpm => new[] { bpm, bpm * 2, bpm / 2 })
                .Where(bpm => bpm is >= MinBpm and <= MaxBpm)
                .Select(bpm => Fold(peaks, hopSeconds, bpm, range: 0.008, step: 0.0005))
                .ToList();

            var tagged = Fold(peaks, hopSeconds, taggedBpm.Value, range: 0.015, step: 0.0005);
            if (tagged.Sharpness >= TagSharpnessShare * candidates.Max(c => c.Sharpness))
            {
                return tagged.Bpm;
            }
            return candidates.MaxBy(c => c.Sharpness * Prior(c.Bpm)).Bpm;
        }

        // Whether the music of one line is over where the other's starts. Give or take the beats that MusicSpan may place
        // each of them past the last beat heard, or before the first.
        private static bool IsBefore(Line line, Line other)
        {
            var leeway = 2 * (BeatsPerSection - MusicBeatsPerSection) * other.Period;
            return MusicSeconds(line).End <= MusicSeconds(other).Start + leeway;
        }

        private static (double Start, double End) MusicSeconds(Line line)
        {
            var (start, end) = MusicSpan([.. line.Heard]);
            return (line.First + (start * line.Period), line.First + (end * line.Period));
        }

        // The first and the last beat with at least 12 of 16 beats heard from there, or the first and last heard if there's no
        // such stretch.
        private static (int Start, int End) MusicSpan(HashSet<int> heard)
        {
            int? start = null;
            int? end = null;
            var first = heard.Min();
            var last = heard.Max();
            for (var number = first; number <= last && start is null; number++)
            {
                if (Enumerable.Range(number, BeatsPerSection).Count(heard.Contains) >= MusicBeatsPerSection)
                {
                    start = number;
                }
            }
            for (var number = last; number >= first && end is null; number--)
            {
                if (Enumerable.Range(number - BeatsPerSection + 1, BeatsPerSection).Count(heard.Contains) >= MusicBeatsPerSection)
                {
                    end = number;
                }
            }
            return start is { } s && end is { } e && s <= e ? (s, e) : (first, last);
        }

        // How many quarters of a beat after the beats most cues are, less how late they are: 0 to 3. Null with fewer than
        // two cues, or when they don't agree. A cue that is on no quarter counts against that.
        private static int? CueQuarter(IReadOnlyList<double> cueSeconds, double firstBeat, double period)
        {
            if (cueSeconds.Count < 2)
            {
                return null;
            }
            var quarters = cueSeconds
                .Select(cue => (cue - CueLateSeconds - firstBeat) / period * 4)
                .Where(quarter => Math.Abs(quarter - Math.Round(quarter)) <= CueQuarterTolerance * 4)
                .Select(quarter => (((int)Math.Round(quarter) % 4) + 4) % 4);
            return MostAgreedOn(quarters, cueSeconds.Count);
        }

        // Which of the first four beats starts a bar, by the cues, when most of them agree.
        private static int? Downbeat(IReadOnlyList<double> cueSeconds, double firstBeat, double period)
        {
            var positions = cueSeconds
                .Select(cue => (int)Math.Round((cue - firstBeat) / period))
                .Where(number => number >= 0)
                .Select(number => number % 4)
                .ToList();
            return positions.Count < 2 ? null : MostAgreedOn(positions, positions.Count);
        }

        // What most of the cues are on, when that is 60% of all of them or more.
        private static int? MostAgreedOn(IEnumerable<int> positions, int all)
        {
            var most = positions.GroupBy(position => position).MaxBy(g => g.Count());
            return most is not null && most.Count() >= MinCueAgreement * all ? most.Key : null;
        }

        // Without cues: where the sound changes most from one beat to the next. New parts of a track come in on the first beat
        // of a bar. Squared, so that a few big changes count more than many small ones. Right for 77% of steady tracks, by
        // Mixed In Key's cues.
        private static int Downbeat(float[] bands, double hopSeconds, double firstBeat, double period)
        {
            var hops = bands.Length / Onsets.BandCount;
            var changes = new double[4];
            double[]? previous = null;
            for (var number = 0; ; number++)
            {
                var from = (int)((firstBeat + (number * period) - OnsetLead) / hopSeconds);
                var to = (int)((firstBeat + ((number + 1) * period) - OnsetLead) / hopSeconds);
                if (to > hops)
                {
                    break;
                }
                from = Math.Max(0, from);
                var current = new double[Onsets.BandCount];
                for (var hop = from; hop < to; hop++)
                {
                    for (var b = 0; b < Onsets.BandCount; b++)
                    {
                        current[b] += bands[(hop * Onsets.BandCount) + b] / Math.Max(1, to - from);
                    }
                }
                if (previous is not null)
                {
                    double change = 0;
                    for (var b = 0; b < Onsets.BandCount; b++)
                    {
                        change += (current[b] - previous[b]) * (current[b] - previous[b]);
                    }
                    changes[number % 4] += change;
                }
                previous = current;
            }
            return Array.IndexOf(changes, changes.Max());
        }

        // Keeps what stands out of the 0.4 s around it, from 0 to 1.
        private static float[] Normalize(float[] envelope, double hopSeconds)
        {
            var length = envelope.Length;
            var width = Math.Max(1, (int)(0.4 / hopSeconds));
            var result = new float[length];
            double sum = 0;
            var from = 0;
            var to = 0;
            for (var i = 0; i < length; i++)
            {
                // The average of a window centered on i.
                while (to < Math.Min(length, i + (width / 2) + 1))
                {
                    sum += envelope[to++];
                }
                while (from < i - (width / 2))
                {
                    sum -= envelope[from++];
                }
                result[i] = (float)Math.Max(0, envelope[i] - (sum / (to - from)));
            }

            var max = result.Length > 0 ? result.Max() : 0;
            if (max <= 0)
            {
                return [];
            }
            for (var i = 0; i < length; i++)
            {
                result[i] /= max;
            }
            return result;
        }

        // From 0 to 1, as it is.
        private static float[] Scale(float[] envelope)
        {
            var max = envelope.Length > 0 ? envelope.Max() : 0;
            return max > 0 ? envelope.Select(value => value / max).ToArray() : [];
        }

        // The geometric mean of bass and click onsets, a hop either way, the click can come a little before the bass rises.
        private static float[] Kicks(float[] bass, float[] click)
        {
            if (bass.Length == 0 || click.Length == 0)
            {
                return [];
            }
            var kicks = new float[bass.Length];
            for (var i = 1; i + 1 < bass.Length; i++)
            {
                var nearClick = Math.Max(click[i], Math.Max(click[i - 1], click[i + 1]));
                kicks[i] = MathF.Sqrt(bass[i] * nearClick);
            }
            return kicks;
        }

        // One per onset, where it is highest. Every hop counted, a sound that swells for a few hops would count a few times,
        // like a bass coming back after the kick ducked it.
        private static (double Index, float Strength)[] Peaks(float[] onsets)
        {
            var peaks = new List<(double, float)>();
            for (var i = 1; i + 1 < onsets.Length; i++)
            {
                if (onsets[i] > 0.01f && onsets[i] >= onsets[i - 1] && onsets[i] > onsets[i + 1])
                {
                    peaks.Add((i, onsets[i]));
                }
            }
            return [.. peaks];
        }

        private static IEnumerable<double> TempoCandidates(float[] onsets, double hopSeconds)
        {
            var minLag = (int)(60 / MaxBpm / hopSeconds);
            var maxLag = (int)(60 / MinBpm / hopSeconds) + 2;
            var correlation = new double[maxLag + 2];
            for (var lag = minLag; lag <= maxLag + 1; lag++)
            {
                double sum = 0;
                for (var i = 0; i + lag < onsets.Length; i++)
                {
                    sum += onsets[i] * onsets[i + lag];
                }
                correlation[lag] = sum;
            }

            var peaks = new List<(double Bpm, double Score)>();
            for (var lag = minLag + 1; lag <= maxLag; lag++)
            {
                if (correlation[lag] > correlation[lag - 1] && correlation[lag] >= correlation[lag + 1])
                {
                    var bend = correlation[lag - 1] - (2 * correlation[lag]) + correlation[lag + 1];
                    var offset = bend == 0 ? 0 : 0.5 * (correlation[lag - 1] - correlation[lag + 1]) / bend;
                    var bpm = 60 / ((lag + offset) * hopSeconds);
                    if (bpm is >= MinBpm and <= MaxBpm)
                    {
                        peaks.Add((bpm, correlation[lag]));
                    }
                }
            }
            return peaks.OrderByDescending(p => p.Score).Take(3).Select(p => p.Bpm);
        }

        // The tempo at which the onsets repeat most: after one beat, after two and four of them, and after half a beat. A
        // tempo that only a part of the rhythm repeats at, like three sixteenths, has less of that. Between 80 and 160 BPM,
        // where Mixed In Key has the tempo of nearly every track: of a tempo and its double, the one in there. Measured
        // against its tags in a DJ's library, this gets the tagged tempo for 95% of the tracks with a steady beat; the
        // sharpest fold of the tempos at which the onsets repeat most got it for 76%.
        private static double CountedTempo(float[] onsets, double hopSeconds)
        {
            var correlation = new double[(int)(CountedBeats[^1] * 60 / MinCountedBpm / hopSeconds) + 3];
            for (var lag = 1; lag < correlation.Length; lag++)
            {
                double sum = 0;
                for (var i = 0; i + lag < onsets.Length; i++)
                {
                    sum += onsets[i] * onsets[i + lag];
                }
                correlation[lag] = sum;
            }

            var best = (Bpm: MinCountedBpm, Score: double.NegativeInfinity);
            for (var bpm = MinCountedBpm; bpm < MaxCountedBpm; bpm += 0.25)
            {
                double score = 0;
                foreach (var beats in CountedBeats)
                {
                    score += Highest(correlation, beats * 60 / bpm / hopSeconds);
                }
                if (score > best.Score)
                {
                    best = (bpm, score);
                }
            }
            return best.Bpm;
        }

        // The highest correlation within half a lag either side, between the lags as a line.
        private static double Highest(double[] correlation, double lag)
        {
            double highest = 0;
            for (var offset = -0.5; offset <= 0.5; offset += 0.25)
            {
                var index = (int)(lag + offset);
                var between = lag + offset - index;
                highest = Math.Max(highest, (correlation[index] * (1 - between)) + (correlation[index + 1] * between));
            }
            return highest;
        }

        // When the tag is overruled, house and techno tempos are likelier than their half or double.
        private static double Prior(double bpm)
        {
            var octaves = Math.Log2(bpm / 125);
            return Math.Exp(-0.5 * octaves * octaves / 0.36);
        }

        // Folds the onsets at tempos around bpm and returns the one at which they pile up most sharply at one phase: the
        // tempo, the highest pile over the average one, and the phase of that pile in seconds.
        private static (double Bpm, double Sharpness, double Phase) Fold((double Index, float Strength)[] peaks, double hopSeconds, double bpm, double range, double step)
        {
            var best = (Bpm: bpm, Sharpness: 0.0, Phase: 0.0);
            for (var candidate = bpm * (1 - range); candidate <= bpm * (1 + range); candidate += bpm * step)
            {
                var piles = Piles(peaks, hopSeconds, candidate);
                double highest = 0;
                double total = 0;
                var highestBin = 0;
                for (var bin = 0; bin < PhaseBins; bin++)
                {
                    total += piles[bin];
                    if (piles[bin] > highest)
                    {
                        highest = piles[bin];
                        highestBin = bin;
                    }
                }
                var sharpness = total > 0 ? highest / (total / PhaseBins) : 0;
                if (sharpness > best.Sharpness)
                {
                    best = (candidate, sharpness, (highestBin + 0.5) / PhaseBins * 60 / candidate);
                }
            }
            return best;
        }

        // The onsets folded at a tempo: how much of them is at each of the phases of a beat, the phases either side counting
        // half.
        private static double[] Piles((double Index, float Strength)[] peaks, double hopSeconds, double bpm)
        {
            var histogram = new double[PhaseBins];
            var beatsPerHop = hopSeconds * bpm / 60;
            foreach (var (index, strength) in peaks)
            {
                var phase = index * beatsPerHop;
                phase -= Math.Floor(phase);
                histogram[(int)(phase * PhaseBins) % PhaseBins] += strength;
            }
            var piles = new double[PhaseBins];
            for (var bin = 0; bin < PhaseBins; bin++)
            {
                piles[bin] = histogram[(bin + PhaseBins - 1) % PhaseBins] + (2 * histogram[bin]) + histogram[(bin + 1) % PhaseBins];
            }
            return piles;
        }

        // The highest pile within a 16th of a beat of a time, in seconds.
        private static double PileNear(double[] piles, double seconds, double period)
        {
            var phase = seconds / period;
            var bin = (int)((phase - Math.Floor(phase)) * PhaseBins);
            var near = PhaseBins / 16;
            return Enumerable.Range(bin - near, (2 * near) + 1).Max(b => piles[((b % PhaseBins) + PhaseBins) % PhaseBins]);
        }

        // A line through the beats heard: the time of beat 0 in the onset envelope, the seconds per beat, how far the music is
        // off it, and the numbers of the beats heard on it.
        private sealed record Line(double First, double Period, double DeviationMs, List<int> Heard);

        // Looks for the onset at each beat of the grid and fits a line through them, three times, leaving out the ones that
        // are far off the line before.
        private static Line? Fit(float[] onsets, double hopSeconds, double bpm, double phase)
        {
            var period = 60 / bpm;
            var first = phase;
            var duration = onsets.Length * hopSeconds;
            var beats = new List<(int Number, double Seconds, double Weight)>();
            for (var round = 0; round < 3; round++)
            {
                var search = period * (round == 0 ? 0.12 : 0.08);
                var found = new List<(int Number, double Seconds, double Weight)>();
                for (var number = 0; first + (number * period) < duration; number++)
                {
                    if (FindOnset(onsets, hopSeconds, first + (number * period), search) is { } onset)
                    {
                        found.Add((number, onset.Seconds, onset.Strength));
                    }
                }
                if (found.Count < 8)
                {
                    return null;
                }

                double sw = 0, sn = 0, st = 0, snn = 0, snt = 0;
                foreach (var (number, seconds, weight) in found)
                {
                    sw += weight;
                    sn += weight * number;
                    st += weight * seconds;
                    snn += weight * number * number;
                    snt += weight * number * seconds;
                }
                var determinant = (sw * snn) - (sn * sn);
                if (determinant <= 0)
                {
                    return null;
                }
                period = ((sw * snt) - (sn * st)) / determinant;
                first = ((st * snn) - (sn * snt)) / determinant;

                var errors = found.Select(b => b.Seconds - (first + (b.Number * period))).ToList();
                var rms = Math.Sqrt(found.Select((b, i) => errors[i] * errors[i] * b.Weight).Sum() / sw);
                var limit = Math.Max(3 * rms, 0.006);
                beats = found.Where((b, i) => Math.Abs(errors[i]) <= limit).ToList();
            }
            if (period <= 0 || beats.Count < 8)
            {
                return null;
            }

            // In each stretch of 16 beats, how far the onsets are off the grid on average.
            var sections = beats
                .GroupBy(b => b.Number / BeatsPerSection)
                .Where(g => g.Count() >= 6)
                .Select(g => Math.Abs(g.Sum(b => (b.Seconds - (first + (b.Number * period))) * b.Weight) / g.Sum(b => b.Weight)) * 1000)
                .OrderBy(ms => ms)
                .ToList();
            var deviation = sections.Count > 0 ? sections[(int)(sections.Count * 0.9)] : NoSectionsDeviationMs;

            return new Line(first, period, deviation, beats.Select(b => b.Number).ToList());
        }

        // The strongest onset within search seconds of the time, placed between hops by a parabola through its neighbours.
        private static (double Seconds, float Strength)? FindOnset(float[] onsets, double hopSeconds, double seconds, double search)
        {
            var from = Math.Max(1, (int)((seconds - search) / hopSeconds));
            var to = Math.Min(onsets.Length - 2, (int)((seconds + search) / hopSeconds));
            var strongest = -1;
            float strength = 0;
            for (var i = from; i <= to; i++)
            {
                if (onsets[i] > strength && onsets[i] >= onsets[i - 1] && onsets[i] >= onsets[i + 1])
                {
                    strength = onsets[i];
                    strongest = i;
                }
            }
            if (strongest < 0 || strength < 0.05f)
            {
                return null;
            }

            var bend = onsets[strongest - 1] - (2 * onsets[strongest]) + onsets[strongest + 1];
            var offset = bend == 0 ? 0 : 0.5 * (onsets[strongest - 1] - onsets[strongest + 1]) / bend;
            return ((strongest + Math.Clamp(offset, -1, 1)) * hopSeconds, strength);
        }
    }
}
