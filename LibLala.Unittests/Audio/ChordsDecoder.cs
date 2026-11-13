using System;
using LibLala.Audio;

namespace LibLala.Unittests.Audio
{
    // Chords in a key: the triads on the first, fourth and fifth note of its scale and the first again, two seconds each,
    // over their roots in the bass. Every tone has a few overtones, like an organ's.
    internal sealed class ChordsDecoder : IAudioDecoder
    {
        private const double ChordSeconds = 2;
        private static readonly int[] Roots = [0, 5, 7, 0];
        private static readonly double[] Overtones = [1, 0.5, 0.3, 0.2];

        private readonly MusicalKey _key;
        private readonly double _tuning;
        private long _position;

        // The tuning in cents: 0 has the A above middle C at 440 Hz.
        public ChordsDecoder(MusicalKey key, double seconds = 24, double tuningCents = 0)
        {
            _key = key;
            _tuning = Math.Pow(2, tuningCents / 1200);
            Length = (long)(seconds * AudioFormat.SampleRate);
        }

        public long Length { get; }
        public double Volume { get; init; } = 0.08;

        public int Read(Span<float> buffer)
        {
            var frames = (int)Math.Min(buffer.Length / AudioFormat.Channels, Length - _position);
            for (var i = 0; i < frames; i++)
            {
                var sample = Sample((_position + i) / (double)AudioFormat.SampleRate);
                buffer[i * AudioFormat.Channels] = sample;
                buffer[(i * AudioFormat.Channels) + 1] = sample;
            }
            _position += frames;
            return frames;
        }

        public void Seek(long frame) => _position = frame;

        public void Dispose() { }

        private float Sample(double seconds)
        {
            var chord = (int)(seconds / ChordSeconds);
            var sinceChord = seconds - (chord * ChordSeconds);
            // The C below middle C, plus the key's tonic, plus the chord's root in the scale.
            var root = 48 + _key.Tonic + Roots[chord % Roots.Length];
            var third = _key.IsMinor ? 3 : 4;
            var sum = (2 * Tone(root - 12, seconds)) + Tone(root, seconds) + Tone(root + third, seconds) + Tone(root + 7, seconds);
            // Fades in and out, a chord doesn't click.
            var fade = Math.Min(1, Math.Min(sinceChord, ChordSeconds - sinceChord) / 0.02);
            return (float)(Volume * fade * sum);
        }

        private double Tone(int note, double seconds)
        {
            var frequency = 440 * Math.Pow(2, (note - 69) / 12.0) * _tuning;
            double sum = 0;
            for (var overtone = 0; overtone < Overtones.Length; overtone++)
            {
                sum += Overtones[overtone] * Math.Sin(2 * Math.PI * frequency * (overtone + 1) * seconds);
            }
            return sum;
        }
    }
}
