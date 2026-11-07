using System;

namespace LibLala.Audio.Mixing
{
    internal enum FilterKind
    {
        LowPass,
        HighPass,
    }

    // A 4th order Butterworth filter, 24 dB per octave, whose cutoff can move while it plays: two biquads after another, by
    // the Audio EQ Cookbook. At the edge of hearing it's off and passes everything as it is.
    internal sealed class Filter
    {
        public const double LowestHz = 20;
        public const double HighestHz = 20000;
        // The Q of the two biquads of a 4th order Butterworth.
        private static readonly double[] Qs = [0.5412, 1.3066];

        private readonly FilterKind _kind;
        private readonly Biquad[] _stages = [new(), new()];
        private double _cutoff;

        public Filter(FilterKind kind)
        {
            _kind = kind;
            _cutoff = Off;
        }

        // Where it lets everything through.
        public double Off => _kind == FilterKind.LowPass ? HighestHz : LowestHz;

        public double Cutoff
        {
            get => _cutoff;
            set
            {
                var cutoff = Math.Clamp(value, LowestHz, HighestHz);
                if (cutoff == _cutoff)
                {
                    return;
                }
                _cutoff = cutoff;
                for (var i = 0; i < _stages.Length; i++)
                {
                    _stages[i].Set(_kind, cutoff, Qs[i]);
                }
            }
        }

        public bool IsOff => IsOffAt(_cutoff);

        // Filters interleaved frames of AudioFormat in place.
        public void Process(Span<float> frames)
        {
            if (IsOff)
            {
                // Without its old samples it would click when it comes on again. Silence is a clean start.
                foreach (var stage in _stages)
                {
                    stage.Clear();
                }
                return;
            }
            foreach (var stage in _stages)
            {
                stage.Process(frames);
            }
        }

        private bool IsOffAt(double cutoff) => _kind == FilterKind.LowPass ? cutoff >= HighestHz : cutoff <= LowestHz;

        // Transposed direct form II, one state per channel.
        private sealed class Biquad
        {
            private double _b0, _b1, _b2, _a1, _a2;
            private readonly double[] _z1 = new double[AudioFormat.Channels];
            private readonly double[] _z2 = new double[AudioFormat.Channels];

            public void Set(FilterKind kind, double cutoff, double q)
            {
                // Not up to Nyquist, the formulas fall apart there.
                var w0 = 2 * Math.PI * Math.Min(cutoff, 0.45 * AudioFormat.SampleRate) / AudioFormat.SampleRate;
                var cos = Math.Cos(w0);
                var alpha = Math.Sin(w0) / (2 * q);
                var a0 = 1 + alpha;
                var (b0, b1) = kind == FilterKind.LowPass ? ((1 - cos) / 2, 1 - cos) : ((1 + cos) / 2, -(1 + cos));
                _b0 = b0 / a0;
                _b1 = b1 / a0;
                _b2 = b0 / a0;
                _a1 = -2 * cos / a0;
                _a2 = (1 - alpha) / a0;
            }

            public void Clear()
            {
                Array.Clear(_z1);
                Array.Clear(_z2);
            }

            public void Process(Span<float> frames)
            {
                for (var i = 0; i < frames.Length; i++)
                {
                    var channel = i % AudioFormat.Channels;
                    double x = frames[i];
                    var y = (_b0 * x) + _z1[channel];
                    _z1[channel] = (_b1 * x) - (_a1 * y) + _z2[channel];
                    _z2[channel] = (_b2 * x) - (_a2 * y);
                    frames[i] = (float)y;
                }
            }
        }
    }
}
