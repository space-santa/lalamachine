using System;

namespace LibLala.Audio.Mixing
{
    internal enum TransitionKind
    {
        // B at A's tempo, their beats on top of each other, through the crossover.
        BeatMatched,
        // Each at its own tempo, without a grid to put the beats on top of each other, through the crossover.
        Crossfade,
        // Steady beats that can't be matched: A plays to the end of its music, B's first bar comes right on that downbeat.
        // Only faded, so that neither clicks, too short for the crossover.
        Cut,
    }

    // How track A goes over into track B. StartA is the second of A where B comes in, Seconds how long it takes as heard,
    // StartB the second of B it comes in with, TempoB the tempo B plays at, of its own, for its beats to be A's.
    internal sealed record TransitionPlan(double StartA, double Seconds, double StartB, double TempoB, TransitionKind Kind);

    internal static class TransitionPlanner
    {
        public const int Bars = 16;
        // Shorter, the filters would sweep too fast to sound like a mix.
        public const int MinBars = 4;
        // Further, stretching would be heard.
        public const double MaxTempoChange = 0.08;
        public const double CrossfadeSeconds = 4;
        // Long enough not to click.
        public const double CutSeconds = 0.02;
        // A transition doesn't start sooner than this from what plays: what's rendered can't change.
        private const double MinLeadSeconds = 0.05;

        // The earliest second of A where the next track can come in, for asking what the next track is in time. Its tempo
        // decides how, so it can be later.
        public static double EarliestStart(double lengthA, BeatGrid? gridA)
        {
            var crossfade = Math.Max(0, (gridA is null ? lengthA : MusicEnd(gridA, lengthA)) - CrossfadeSeconds);
            return gridA is { IsSteady: true } ? Math.Clamp(MusicEnd(gridA, lengthA) - (Bars * gridA.BarSeconds), 0, crossfade) : crossfade;
        }

        // nowA is the second of A that plays next. A plays at tempoA of its own.
        public static TransitionPlan Plan(double nowA, double lengthA, BeatGrid? gridA, double tempoA, BeatGrid? gridB)
        {
            if (gridA is { IsSteady: true } && gridB is { IsSteady: true })
            {
                var tempoB = gridA.BeatsPerMinute * tempoA / gridB.BeatsPerMinute;
                if (Math.Abs(Math.Log(tempoB)) <= Math.Log(1 + MaxTempoChange))
                {
                    // The last bars of A's music, with B's first bars over them. Too late for all of them, from A's next bar.
                    var bar = gridA.BarSeconds;
                    var end = MusicEnd(gridA, lengthA);
                    var start = Math.Max(end - (Bars * bar), DownbeatAtOrAfter(gridA, nowA + MinLeadSeconds));
                    var bars = (int)Math.Round((end - start) / bar);
                    if (bars >= MinBars)
                    {
                        return new TransitionPlan(start, bars * bar / tempoA, FirstBar(gridB), tempoB, TransitionKind.BeatMatched);
                    }
                }

                // Too far apart, or too late: rather than beats at two tempos over each other, one after the other.
                return Cut(nowA, lengthA, gridA, tempoA, gridB);
            }

            // Over by the end of A's music, not in a tail after it.
            var crossfadeEnd = gridA is null ? lengthA : MusicEnd(gridA, lengthA);
            var crossfadeStart = Math.Max(nowA + MinLeadSeconds, crossfadeEnd - CrossfadeSeconds);
            var seconds = Math.Max(0, Math.Min(CrossfadeSeconds, lengthA - crossfadeStart)) / tempoA;
            return new TransitionPlan(crossfadeStart, seconds, 0, 1, TransitionKind.Crossfade);
        }

        // On the downbeat where A's music ends, or the next one when A is past it. B fades in before its first bar, so that
        // this is heard whole.
        private static TransitionPlan Cut(double nowA, double lengthA, BeatGrid gridA, double tempoA, BeatGrid gridB)
        {
            var firstBar = FirstBar(gridB);
            var lead = Math.Min(CutSeconds, firstBar);
            var downbeat = Math.Max(MusicEnd(gridA, lengthA), DownbeatAtOrAfter(gridA, nowA + MinLeadSeconds + (lead * tempoA)));
            // Without a downbeat left in the file, at its end.
            var startA = Math.Max(Math.Min(downbeat, lengthA) - (lead * tempoA), nowA + MinLeadSeconds);
            return new TransitionPlan(startA, CutSeconds, firstBar - lead, 1, TransitionKind.Cut);
        }

        // Where A's music is over: the bar after its last beat. A last hit on the first beat of a bar ends it right there. On
        // a downbeat in any case, the transition starts a whole number of bars before: when the bar after the last beat
        // isn't in the file any more, the bar before.
        private static double MusicEnd(BeatGrid grid, double length)
        {
            var end = DownbeatAtOrAfter(grid, grid.MusicEnd - (grid.BeatSeconds / 4));
            while (end > length && end - grid.BarSeconds > 0)
            {
                end -= grid.BarSeconds;
            }
            return end;
        }

        private static double DownbeatAtOrAfter(BeatGrid grid, double seconds)
        {
            var bars = Math.Ceiling((seconds - grid.FirstDownbeat) / grid.BarSeconds);
            return grid.FirstDownbeat + (bars * grid.BarSeconds);
        }

        // B comes in with the bar its music starts in, or its first whole bar.
        private static double FirstBar(BeatGrid grid)
        {
            var bars = Math.Floor((grid.MusicStart + (grid.BeatSeconds / 4) - grid.FirstDownbeat) / grid.BarSeconds);
            return grid.FirstDownbeat + (Math.Max(0, bars) * grid.BarSeconds);
        }
    }

    // How the transition sounds, over its course p from 0 to 1. A crossover that moves up: A is high-passed and B low-passed
    // at the same cutoff, from the bottom of hearing to the top. B comes in from its bass up while A goes out from its bass
    // up, so their bass lines don't play over each other. A also fades in the last quarter, a high-pass at 10 kHz still
    // lets cymbals through, and B in the first eighth, so its first low-passed kick doesn't thump in. A cut only fades.
    internal static class TransitionShape
    {
        // Of A's high-pass and B's low-pass: both off at the ends, a tenth of the octaves each tenth of the way.
        public static double Crossover(double p) => Filter.LowestHz * Math.Pow(Filter.HighestHz / Filter.LowestHz, Math.Clamp(p, 0, 1));

        // Equal power: what fades out and in sounds as loud together.
        public static double OutgoingGain(double p, TransitionKind kind)
        {
            var from = kind == TransitionKind.BeatMatched ? 0.75 : 0;
            return p <= from ? 1 : Math.Cos(Math.Clamp((p - from) / (1 - from), 0, 1) * Math.PI / 2);
        }

        public static double IncomingGain(double p, TransitionKind kind)
        {
            var until = kind == TransitionKind.BeatMatched ? 0.125 : 1;
            return p >= until ? 1 : Math.Sin(Math.Clamp(p / until, 0, 1) * Math.PI / 2);
        }
    }
}
