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
        public const int Version = 2;

        private const double MinBpm = 60;
        private const double MaxBpm = 200;
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

        // The onset envelope peaks at a frame that starts this long before the attack: the frame with the attack 70% of the
        // way in, where its window rises steeply. Measured with synthetic drums.
        private const double OnsetLead = 0.0323;

        // Null when there is no beat to find.
        public static BeatGrid? Analyze(IAudioDecoder decoder, double? taggedBpm) => Analyze(decoder, taggedBpm, cueSeconds: []);

        // Cues, like Mixed In Key's, are on the first beat of bars. They decide which beat is the first of a bar when most
        // of them agree.
        public static BeatGrid? Analyze(IAudioDecoder decoder, double? taggedBpm, IReadOnlyList<double> cueSeconds)
        {
            ArgumentNullException.ThrowIfNull(decoder);
            ArgumentNullException.ThrowIfNull(cueSeconds);
            return Analyze(OnsetEnvelope.Compute(decoder), OnsetEnvelope.HopSeconds, taggedBpm, cueSeconds);
        }

        internal static BeatGrid? Analyze(Onsets envelopes, double hopSeconds, double? taggedBpm, IReadOnlyList<double> cueSeconds)
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

            // Candidates: the tempos at which the onsets repeat most, with double and half of them, the autocorrelation
            // often prefers one of those over the tempo the music is counted in.
            var candidates = TempoCandidates(onsets, hopSeconds)
                .SelectMany(bpm => new[] { bpm, bpm * 2, bpm / 2 })
                .Where(bpm => bpm is >= MinBpm and <= MaxBpm)
                .Select(bpm => Fold(peaks, hopSeconds, bpm, range: 0.008, step: 0.0005))
                .ToList();
            if (candidates.Count == 0)
            {
                return null;
            }

            var best = candidates.MaxBy(c => c.Sharpness * Prior(c.Bpm));
            if (taggedBpm is > 0 and { } tag)
            {
                var tagged = Fold(peaks, hopSeconds, tag, range: 0.015, step: 0.0005);
                if (tagged.Sharpness >= TagSharpnessShare * candidates.Max(c => c.Sharpness))
                {
                    best = tagged;
                }
            }
            best = Fold(peaks, hopSeconds, best.Bpm, range: 0.0006, step: 0.00004);

            // The beat is where the kick is. All onsets together often pile up higher on the off-beat, where hi-hats are, and
            // the bass alone where a bass line plays between the kicks. A kick is in the bass and the click range at once.
            // Measured against Mixed In Key's cues in a DJ's library: all onsets put 28% of steady tracks half a beat off,
            // the kick 6%. From the raw flux, a moving average taken off loses some kicks.
            var phase = best.Phase;
            var bass = Scale(envelopes.Bass);
            var expectedBeats = onsets.Length * hopSeconds * best.Bpm / 60;
            foreach (var beats in new[] { Kicks(bass, Scale(envelopes.Click)), bass })
            {
                var beatPeaks = Peaks(beats);
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

            // Numbered from the first beat at or after the start of the track.
            var period = line.Period;
            var firstBeat = line.First + OnsetLead;
            var shift = (int)Math.Floor(firstBeat / period);
            firstBeat -= shift * period;
            var heard = line.Heard.Select(number => number + shift).ToHashSet();
            var (start, end) = MusicSpan(heard);
            var downbeat = Downbeat(cueSeconds, firstBeat, period) ?? Downbeat(envelopes.Bands, hopSeconds, firstBeat, period);

            return new BeatGrid(60 / period, firstBeat, line.DeviationMs, line.Heard.Count, firstBeat + (downbeat * period), firstBeat + (start * period), firstBeat + (end * period));
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

        // Which of the first four beats starts a bar, by the cues, when most of them agree.
        private static int? Downbeat(IReadOnlyList<double> cueSeconds, double firstBeat, double period)
        {
            var positions = cueSeconds
                .Select(cue => (int)Math.Round((cue - firstBeat) / period))
                .Where(number => number >= 0)
                .Select(number => number % 4)
                .ToList();
            if (positions.Count < 2)
            {
                return null;
            }
            var most = positions.GroupBy(position => position).MaxBy(g => g.Count())!;
            return most.Count() >= MinCueAgreement * positions.Count ? most.Key : null;
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

        // Without a tag, house and techno tempos are likelier than their half or double.
        private static double Prior(double bpm)
        {
            var octaves = Math.Log2(bpm / 125);
            return Math.Exp(-0.5 * octaves * octaves / 0.36);
        }

        // Folds the onsets at tempos around bpm and returns the one at which they pile up most sharply at one phase: the
        // tempo, the highest pile over the average one, and the phase of that pile in seconds.
        private static (double Bpm, double Sharpness, double Phase) Fold((double Index, float Strength)[] peaks, double hopSeconds, double bpm, double range, double step)
        {
            var histogram = new double[PhaseBins];
            var best = (Bpm: bpm, Sharpness: 0.0, Phase: 0.0);
            for (var candidate = bpm * (1 - range); candidate <= bpm * (1 + range); candidate += bpm * step)
            {
                Array.Clear(histogram);
                var beatsPerHop = hopSeconds * candidate / 60;
                foreach (var (index, strength) in peaks)
                {
                    var phase = index * beatsPerHop;
                    phase -= Math.Floor(phase);
                    histogram[(int)(phase * PhaseBins) % PhaseBins] += strength;
                }

                double highest = 0;
                double total = 0;
                var highestBin = 0;
                for (var bin = 0; bin < PhaseBins; bin++)
                {
                    var smoothed = histogram[(bin + PhaseBins - 1) % PhaseBins] + (2 * histogram[bin]) + histogram[(bin + 1) % PhaseBins];
                    total += smoothed;
                    if (smoothed > highest)
                    {
                        highest = smoothed;
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
