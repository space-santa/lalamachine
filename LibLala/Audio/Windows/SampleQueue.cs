using System;

namespace LibLala.Audio.Windows
{
    // Interleaved samples on their way through the decoder, oldest first.
    internal sealed class SampleQueue
    {
        private float[] _samples = new float[16384];
        private int _start;
        private int _end;

        public int Count => _end - _start;

        public ReadOnlySpan<float> Samples => _samples.AsSpan(_start, Count);

        public void Append(ReadOnlySpan<float> samples)
        {
            if (_end + samples.Length > _samples.Length)
            {
                var kept = Samples;
                var into = Count + samples.Length > _samples.Length ? new float[Math.Max(2 * _samples.Length, Count + samples.Length)] : _samples;
                kept.CopyTo(into);
                _samples = into;
                _end = Count;
                _start = 0;
            }
            samples.CopyTo(_samples.AsSpan(_end));
            _end += samples.Length;
        }

        // Takes out the oldest, at most as many as there are.
        public void Skip(int count)
        {
            _start += Math.Min(count, Count);
            if (_start == _end)
            {
                Clear();
            }
        }

        public void Clear()
        {
            _start = 0;
            _end = 0;
        }
    }
}
