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
        // Of a beat, where the 16ths between the kick and the off-beat are.
        private static readonly double[] Sixteenths = [0.25, 0.75];
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
        // Hi-hats this loud on the 16ths between the kicks and the off-beat hi-hats, a quarter and three quarters into each
        // beat.
        public double SixteenthHiHat { get; init; }
        // A bass note this far into each beat, as loud as the kick. With a hi-hat there it is in the bass and the click range
        // at once, like a kick: on the off-beat, or on a 16th with SixteenthHiHat.
        public double? BassNoteAt { get; init; }
        // The kicks alone move ahead and back by up to that many ms, over 20 s.
        public double KickDriftMs { get; init; }
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
            var sinceKick = sinceBeat - (KickDriftMs / 1000 * Math.Sin(2 * Math.PI * seconds / 20));
            var k = sinceKick - (Math.Floor(sinceKick / period) * period);
            if (sinceKick >= 0 && HasKick && drums)
            {
                // A sine falling from 120 to 50 Hz.
                var phase = 2 * Math.PI * ((50 * k) + (70 * 0.03 * (1 - Math.Exp(-k / 0.03))));
                sample += 0.8 * Math.Exp(-k / 0.08) * Math.Sin(phase);
                if (HasClick)
                {
                    sample += 0.3 * Math.Exp(-k / 0.004) * Math.Sin(2 * Math.PI * 3000 * k);
                }
            }
            var sinceOffBeat = t - (period / 2);
            if (sinceBeat >= 0 && sinceOffBeat >= 0 && HasHiHat && drums)
            {
                sample += 0.2 * Math.Exp(-sinceOffBeat / 0.02) * ((_random.NextDouble() * 2) - 1);
            }
            foreach (var sixteenth in Sixteenths)
            {
                var sinceSixteenth = t - (sixteenth * period);
                if (sinceBeat >= 0 && sinceSixteenth >= 0 && SixteenthHiHat > 0 && drums)
                {
                    sample += SixteenthHiHat * Math.Exp(-sinceSixteenth / 0.02) * ((_random.NextDouble() * 2) - 1);
                }
            }
            var sinceBassNote = t - ((BassNoteAt ?? 0) * period);
            if (sinceBeat >= 0 && sinceBassNote >= 0 && BassNoteAt is not null && drums)
            {
                sample += 0.8 * Math.Exp(-sinceBassNote / 0.08) * Math.Sin(2 * Math.PI * 55 * sinceBassNote);
            }
            return (float)sample;
        }
    }
}
