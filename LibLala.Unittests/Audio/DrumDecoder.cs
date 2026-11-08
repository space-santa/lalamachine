using System;
using LibLala.Audio;

namespace LibLala.Unittests.Audio
{
    // A drum loop: a kick that starts exactly on each beat, a hi-hat on the off-beats, and some noise. The tempo can drift
    // like a band's.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA5394:Do not use insecure randomness", Justification = "Noise for a test signal.")]
    internal sealed class DrumDecoder : IAudioDecoder
    {
        private readonly double _bpm;
        private readonly double _firstBeat;
        private readonly double _driftMs;
        private readonly Random _random = new(7);
        private long _position;

        // Drift moves the beats ahead and back by up to that many ms, over half a minute.
        public DrumDecoder(double bpm, double firstBeat, double seconds = 120, double driftMs = 0)
        {
            _bpm = bpm;
            _firstBeat = firstBeat;
            _driftMs = driftMs;
            Length = (long)(seconds * AudioFormat.SampleRate);
        }

        public long Length { get; }
        public bool HasKick { get; init; } = true;
        // A kick drum's beater, a 3 kHz tick at its start. An 808 has none.
        public bool HasClick { get; init; } = true;
        public bool HasHiHat { get; init; } = true;
        // A pad chord that changes on the first beat of each bar: the beat with this number, and every fourth after it.
        public int? Downbeat { get; init; }
        // The drums only play in between, in seconds.
        public double BeatsFrom { get; init; }
        public double BeatsUntil { get; init; } = double.MaxValue;
        public double Noise { get; init; } = 0.02;

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
            var period = 60 / _bpm;
            var drifted = seconds - (_driftMs / 1000 * Math.Sin(2 * Math.PI * seconds / 30));
            var sinceBeat = drifted - _firstBeat;
            var beat = Math.Floor(sinceBeat / period);
            var t = sinceBeat - (beat * period);
            var sample = Noise * ((_random.NextDouble() * 2) - 1);
            if (Downbeat is { } downbeat && sinceBeat >= 0)
            {
                var bar = (int)Math.Floor((beat - downbeat) / 4);
                var root = 110 * Math.Pow(2, (bar * 5 % 12) / 12.0);
                sample += 0.1 * (Math.Sin(2 * Math.PI * root * seconds) + Math.Sin(2 * Math.PI * root * 1.5 * seconds));
            }
            var drums = seconds >= BeatsFrom && seconds <= BeatsUntil;
            if (sinceBeat >= 0 && HasKick && drums)
            {
                // A sine falling from 120 to 50 Hz.
                var phase = 2 * Math.PI * ((50 * t) + (70 * 0.03 * (1 - Math.Exp(-t / 0.03))));
                sample += 0.8 * Math.Exp(-t / 0.08) * Math.Sin(phase);
                if (HasClick)
                {
                    sample += 0.3 * Math.Exp(-t / 0.004) * Math.Sin(2 * Math.PI * 3000 * t);
                }
            }
            var sinceOffBeat = t - (period / 2);
            if (sinceBeat >= 0 && sinceOffBeat >= 0 && HasHiHat && drums)
            {
                sample += 0.2 * Math.Exp(-sinceOffBeat / 0.02) * ((_random.NextDouble() * 2) - 1);
            }
            return (float)sample;
        }
    }
}
