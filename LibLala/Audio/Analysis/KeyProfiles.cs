using System;

namespace LibLala.Audio.Analysis
{
    // What tracks in a key have of each note: how much a note counts for or against a key, by its octave and by how many
    // semitones above the key's tonic it is. Fitted to the keys that Mixed In Key found for the 3200 tracks of a DJ's
    // library, electronic music and rock for the most part. For tracks left out of the fitting, 84% get the same key and
    // 93% that one or a neighbour of it on the Camelot wheel. The profiles from listening tests, Krumhansl's, get 50%.
    internal static class KeyProfiles
    {
        public const int Octaves = 6;

        // A note's share of the sound counts by its logarithm: twice the share is a step up, however much there is.
        private const double ShareScale = 100;

        // Per octave, from the one that starts at the C of 33 Hz, and per semitone above the tonic.
        private static readonly double[][] Major =
        [
            [0.163, -0.135, -0.087, -0.447, -0.269, 0.143, -0.501, 0.310, -0.139, -0.323, -0.498, 0.261],
            [2.331, -0.587, -0.464, -0.729, 0.201, 0.515, -0.602, 1.535, -0.774, -0.224, -0.379, -0.647],
            [2.502, -1.161, -0.283, -0.512, 0.472, 0.482, -1.032, 1.835, -0.971, 0.000, -1.250, -0.588],
            [2.326, -1.053, -0.239, -0.917, 1.647, -0.034, -1.394, 1.941, -0.975, -0.277, -1.273, -0.349],
            [1.776, -1.886, 0.838, -1.047, 1.466, -0.617, -0.969, 1.678, -0.895, -0.269, -1.032, 0.346],
            [0.517, -1.086, 0.544, -0.886, 1.127, -0.377, -0.405, 0.907, -0.754, 0.249, -0.518, 0.567],
        ];

        private static readonly double[][] Minor =
        [
            [1.851, 0.251, -0.004, 0.019, -0.194, -0.245, 0.118, -0.012, 0.529, -0.642, -0.285, 0.136],
            [2.804, 0.113, -0.867, -0.110, -1.255, -0.718, -1.031, 1.550, 0.631, -1.141, -0.246, 0.094],
            [2.795, -0.700, -0.589, 0.474, -0.031, -0.580, -1.029, 2.330, 0.003, -1.457, 0.189, -0.894],
            [2.290, -1.096, 0.200, 0.712, 0.148, -0.193, -0.864, 1.929, -0.620, -1.752, 0.807, -0.957],
            [1.577, -1.102, 0.103, 1.035, -0.630, -0.103, -0.301, 1.387, -0.695, -0.681, 1.124, -1.100],
            [0.893, -0.948, 0.216, 0.300, -0.064, -0.215, -0.149, 1.253, -0.499, -0.625, 0.466, -0.511],
        ];

        // What each key has ahead of the others before any note counts, by its tonic from C. Fitted with the profiles: the
        // notes alone find some keys too often and others too rarely. Without these, 80% of the tracks get Mixed In Key's
        // key.
        private static readonly double[] MajorLead = [1.571, 0.557, 0.099, -0.342, -1.306, 1.330, -0.557, -0.280, 0.344, -0.379, 0.036, -1.073];
        private static readonly double[] MinorLead = [1.756, -0.118, 1.280, -0.307, -1.294, 1.058, 0.201, -0.283, -0.958, 0.456, 0.387, -1.741];

        // The key that the notes fit best. Shares has how much of the sound each note is, 12 per octave from C.
        public static MusicalKey BestKey(ReadOnlySpan<double> shares)
        {
            Span<double> levels = stackalloc double[Octaves * 12];
            for (var note = 0; note < levels.Length; note++)
            {
                levels[note] = Math.Log(1 + (ShareScale * shares[note]));
            }

            var best = (Score: double.NegativeInfinity, Key: default(MusicalKey));
            for (var tonic = 0; tonic < 12; tonic++)
            {
                var major = Score(levels, Major, tonic) + MajorLead[tonic];
                if (major > best.Score)
                {
                    best = (major, new MusicalKey(tonic, isMinor: false));
                }
                var minor = Score(levels, Minor, tonic) + MinorLead[tonic];
                if (minor > best.Score)
                {
                    best = (minor, new MusicalKey(tonic, isMinor: true));
                }
            }
            return best.Key;
        }

        private static double Score(ReadOnlySpan<double> levels, double[][] profile, int tonic)
        {
            double score = 0;
            for (var octave = 0; octave < Octaves; octave++)
            {
                for (var semitone = 0; semitone < 12; semitone++)
                {
                    score += profile[octave][semitone] * levels[(octave * 12) + ((tonic + semitone) % 12)];
                }
            }
            return score;
        }
    }
}
