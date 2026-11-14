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
    // StartB the second of B it comes in with, TempoB the tempo B plays at, of its own, for its beats to be A's. Before,
    // A goes to the tempo it plays at through the transition by RampA, which is over at StartA.
    internal sealed record TransitionPlan(double StartA, double Seconds, double StartB, double TempoB, TransitionKind Kind, TempoRamp RampA)
    {
        // Of A's own, through the transition.
        public double TempoA => RampA.To;
    }

    // A track's tempo going from From to To, by where the track is: from its second Start to its second End. Of the
    // track's own, before Start it's From, after End To.
    internal readonly record struct TempoRamp(double Start, double End, double From, double To)
    {
        // The tempo stays as it is.
        public static TempoRamp None(double second, double tempo) => new(second, second, tempo, tempo);

        public double At(double second) =>
            second >= End ? To : second <= Start ? From : From + ((To - From) * (second - Start) / (End - Start));
    }

    internal static class TransitionPlanner
    {
        public const int Bars = 16;
        // Shorter, the filters would sweep too fast to sound like a mix.
        public const int MinBars = 4;
        // Before the transition A goes halfway to B's tempo over this many bars, after it B goes back to its own over as many.
        public const int RampBars = 8;
        // Fewer, A's tempo would jump. Without the room for it A keeps its tempo, and B goes all the way.
        public const int MinRampBars = 1;
        // For each track. Further, SoundTouch's stretching is heard: 7% was, grainy.
        public const double MaxTempoChange = 0.05;
        public const double CrossfadeSeconds = 4;
        // Long enough not to click.
        public const double CutSeconds = 0.02;
        // A transition doesn't start sooner than this from what plays: what's rendered can't change.
        private const double MinLeadSeconds = 0.05;

        // The earliest second of A where the next track can change how it plays, for asking what the next track is in time:
        // where A's tempo starts going to B's before a beat matched transition. B's tempo decides how, so it can be later.
        public static double EarliestStart(double lengthA, BeatGrid? gridA)
        {
            var crossfade = Math.Max(0, (gridA is null ? lengthA : MusicEnd(gridA, lengthA)) - CrossfadeSeconds);
            return gridA is { IsSteady: true } ? Math.Clamp(MusicEnd(gridA, lengthA) - ((RampBars + Bars) * gridA.BarSeconds), 0, crossfade) : crossfade;
        }

        // nowA is the second of A that plays next, A plays at tempoA of its own there. Nothing starts before it, and A's
        // tempo doesn't change before rampFromA either: with nowA 0 the transition is where it would be from the start of A,
        // while A's tempo only changes from where it plays.
        public static TransitionPlan Plan(double nowA, double lengthA, BeatGrid? gridA, double tempoA, BeatGrid? gridB, double rampFromA = 0)
        {
            if (gridA is { IsSteady: true } && gridB is { IsSteady: true })
            {
                // The last bars of A's music, with B's first bars over them. Too late for all of them, from A's next bar.
                var bar = gridA.BarSeconds;
                var end = MusicEnd(gridA, lengthA);
                var start = Math.Max(end - (Bars * bar), DownbeatAtOrAfter(gridA, nowA + MinLeadSeconds));
                var bars = (int)Math.Round((end - start) / bar);
                if (bars >= MinBars)
                {
                    // Each goes halfway, so that neither changes much: A in the bars before, B through the transition. Too
                    // late for all of those bars, A in the whole bars left. Without them, B goes all the way to A's tempo.
                    var rampStart = Math.Max(start - (RampBars * bar), DownbeatAtOrAfter(gridA, Math.Max(nowA, rampFromA) + MinLeadSeconds));
                    var halfway = Math.Sqrt(gridB.BeatsPerMinute / gridA.BeatsPerMinute);
                    var rampA = Math.Round((start - rampStart) / bar) >= MinRampBars && IsWithinReach(halfway)
                        ? new TempoRamp(rampStart, start, tempoA, halfway)
                        : TempoRamp.None(start, tempoA);
                    var tempoB = gridA.BeatsPerMinute * rampA.To / gridB.BeatsPerMinute;
                    if (IsWithinReach(tempoB))
                    {
                        return new TransitionPlan(start, bars * bar / rampA.To, FirstBar(gridB), tempoB, TransitionKind.BeatMatched, rampA);
                    }
                }

                // Too far apart, or too late: rather than beats at two tempos over each other, one after the other.
                return Cut(nowA, lengthA, gridA, tempoA, gridB);
            }

            // Over by the end of A's music, not in a tail after it.
            var crossfadeEnd = gridA is null ? lengthA : MusicEnd(gridA, lengthA);
            var crossfadeStart = Math.Max(nowA + MinLeadSeconds, crossfadeEnd - CrossfadeSeconds);
            var seconds = Math.Max(0, Math.Min(CrossfadeSeconds, lengthA - crossfadeStart)) / tempoA;
            return new TransitionPlan(crossfadeStart, seconds, 0, 1, TransitionKind.Crossfade, TempoRamp.None(crossfadeStart, tempoA));
        }

        // The tempo, of the track's own, isn't so far from it that the stretching is heard.
        private static bool IsWithinReach(double tempo) => Math.Abs(Math.Log(tempo)) <= Math.Log(1 + MaxTempoChange) + 1e-12;

        // On the downbeat where A's music ends, or the next one when A is past it. B fades in before its first bar, so that
        // this is heard whole.
        private static TransitionPlan Cut(double nowA, double lengthA, BeatGrid gridA, double tempoA, BeatGrid gridB)
        {
            var firstBar = FirstBar(gridB);
            var lead = Math.Min(CutSeconds, firstBar);
            var downbeat = Math.Max(MusicEnd(gridA, lengthA), DownbeatAtOrAfter(gridA, nowA + MinLeadSeconds + (lead * tempoA)));
            // Without a downbeat left in the file, at its end.
            var startA = Math.Max(Math.Min(downbeat, lengthA) - (lead * tempoA), nowA + MinLeadSeconds);
            return new TransitionPlan(startA, CutSeconds, firstBar - lead, 1, TransitionKind.Cut, TempoRamp.None(startA, tempoA));
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
