using System;

namespace LibLala.Audio.Analysis
{
    // The magnitudes of the spectrum of a real signal, through a complex FFT of half the size.
    internal sealed class RealFft
    {
        private readonly int _half;
        private readonly int[] _bitReversed;
        private readonly float[] _cos;
        private readonly float[] _sin;
        // Twiddles that split the half-size result into the even and odd samples' spectra.
        private readonly float[] _splitCos;
        private readonly float[] _splitSin;
        private readonly float[] _re;
        private readonly float[] _im;

        // Size is a power of two.
        public RealFft(int size)
        {
            _half = size / 2;
            var bits = (int)Math.Log2(_half);
            _bitReversed = new int[_half];
            for (var i = 0; i < _half; i++)
            {
                var reversed = 0;
                for (var bit = 0; bit < bits; bit++)
                {
                    if ((i & (1 << bit)) != 0)
                    {
                        reversed |= 1 << (bits - 1 - bit);
                    }
                }
                _bitReversed[i] = reversed;
            }

            _cos = new float[_half / 2];
            _sin = new float[_half / 2];
            for (var i = 0; i < _half / 2; i++)
            {
                _cos[i] = (float)Math.Cos(-2 * Math.PI * i / _half);
                _sin[i] = (float)Math.Sin(-2 * Math.PI * i / _half);
            }

            _splitCos = new float[_half];
            _splitSin = new float[_half];
            for (var k = 0; k < _half; k++)
            {
                _splitCos[k] = (float)Math.Cos(-2 * Math.PI * k / size);
                _splitSin[k] = (float)Math.Sin(-2 * Math.PI * k / size);
            }

            _re = new float[_half];
            _im = new float[_half];
        }

        // Signal has the size, magnitudes gets half of it: bins 0 up to the one below Nyquist.
        public void Magnitudes(ReadOnlySpan<float> signal, Span<float> magnitudes)
        {
            var re = _re;
            var im = _im;
            var half = _half;

            // Even samples as the real parts, odd ones as the imaginary parts.
            for (var i = 0; i < half; i++)
            {
                var j = _bitReversed[i];
                re[j] = signal[2 * i];
                im[j] = signal[(2 * i) + 1];
            }

            for (var length = 2; length <= half; length <<= 1)
            {
                var span = length >> 1;
                var step = half / length;
                for (var start = 0; start < half; start += length)
                {
                    for (var k = 0; k < span; k++)
                    {
                        var wr = _cos[k * step];
                        var wi = _sin[k * step];
                        var a = start + k;
                        var b = a + span;
                        var tr = (re[b] * wr) - (im[b] * wi);
                        var ti = (re[b] * wi) + (im[b] * wr);
                        re[b] = re[a] - tr;
                        im[b] = im[a] - ti;
                        re[a] += tr;
                        im[a] += ti;
                    }
                }
            }

            magnitudes[0] = Math.Abs(re[0] + im[0]);
            for (var k = 1; k < half; k++)
            {
                // The even samples' spectrum plus the odd samples' one turned by the split twiddle.
                var conjugateRe = re[half - k];
                var conjugateIm = -im[half - k];
                var evenRe = 0.5f * (re[k] + conjugateRe);
                var evenIm = 0.5f * (im[k] + conjugateIm);
                var oddRe = 0.5f * (im[k] - conjugateIm);
                var oddIm = -0.5f * (re[k] - conjugateRe);
                var xr = evenRe + (_splitCos[k] * oddRe) - (_splitSin[k] * oddIm);
                var xi = evenIm + (_splitCos[k] * oddIm) + (_splitSin[k] * oddRe);
                magnitudes[k] = MathF.Sqrt((xr * xr) + (xi * xi));
            }
        }
    }
}
