using System;
using System.Numerics;

namespace LibLala.Audio.Analysis
{
    // Finds the key of a track. Every 0.4 s it takes the peaks of the spectrum for the tones that sound, and adds them up by
    // pitch over the whole track. How much of each of the twelve notes there is, in each octave, is compared to what tracks
    // in every key have of them: KeyProfiles.
    public sealed class KeyAnalyzer
    {
        // Goes up when the analysis changes, so that the keys are looked for again.
        public const int Version = 1;

        // An eighth of AudioFormat's sample rate has the tones up to 2 kHz, and at that rate a window of 1.5 s tells the
        // semitones apart all the way down in the bass.
        private const int Decimation = 8;
        private const double SampleRate = (double)AudioFormat.SampleRate / Decimation;
        private const int LowpassTaps = 128;
        private const int Window = 8192;
        private const int Hop = 2048;
        private const int BlockSamples = 16384;

        // Pitches from the C at 33 Hz up to the one at 2093 Hz, six octaves, in tenths of a semitone.
        private const int LowestNote = 24;
        private const int Octaves = KeyProfiles.Octaves;
        private const int BinsPerSemitone = 10;
        private const int Bins = Octaves * 12 * BinsPerSemitone;
        // The A above middle C, as a MIDI note and in Hz.
        private const int ConcertNote = 69;
        private const double ConcertHz = 440;

        // Quieter than -60 dB is silence.
        private const double SilencePower = 1e-6;
        // A peak has to be within 60 dB of the loudest one of its frame.
        private const float PeakFloor = 1e-3f;
        // Frames with sound that it takes to tell a key: four seconds.
        private const int MinFrames = 8;
        // A peak counts by how far it stands out of the octave around it: half an octave either way.
        private const double SurroundingsRatio = 1.4142;

        private static readonly float[] Lowpass = DesignLowpass();
        private static readonly float[] Hann = DesignHann();
        private static readonly int LowestBin = (int)(HzOf(LowestNote - 0.5) * Window / SampleRate);
        private static readonly int HighestBin = Math.Min((Window / 2) - 2, (int)(HzOf(LowestNote + (Octaves * 12) + 0.5) * Window / SampleRate) + 1);

        private readonly RealFft _fft = new(Window);
        // Mono samples that aren't decimated yet, and decimated ones that aren't in a frame yet.
        private readonly float[] _mono = new float[LowpassTaps + BlockSamples];
        private readonly float[] _samples = new float[Window + BlockSamples];
        private readonly float[] _frame = new float[Window];
        private readonly float[] _magnitudes = new float[Window / 2];
        // Sums[k] is the sum of the magnitudes below bin k.
        private readonly double[] _sums = new double[(Window / 2) + 1];
        private readonly double[] _pitches = new double[Bins];
        private int _monoCount;
        private int _sampleCount;
        private int _frames;

        // Null when there is too little sound in the track to tell.
        public static MusicalKey? Analyze(IAudioDecoder decoder)
        {
            var analyzer = new KeyAnalyzer();
            analyzer.Read(decoder);
            return analyzer.Key();
        }

        // All of the track, from where the decoder is.
        public void Read(IAudioDecoder decoder)
        {
            ArgumentNullException.ThrowIfNull(decoder);

            var block = new float[BlockSamples * AudioFormat.Channels];
            int read;
            while ((read = decoder.Read(block)) > 0)
            {
                Add(block.AsSpan(0, read * AudioFormat.Channels));
            }
        }

        // The next frames of the track, as AudioFormat has them: whole frames, left and right.
        public void Add(ReadOnlySpan<float> frames)
        {
            if (frames.Length % AudioFormat.Channels != 0)
            {
                throw new ArgumentException($"Not a whole number of frames of {AudioFormat.Channels} samples.", nameof(frames));
            }

            var offset = 0;
            while (offset < frames.Length)
            {
                var count = Math.Min((frames.Length - offset) / AudioFormat.Channels, _mono.Length - _monoCount);
                for (var i = 0; i < count; i++)
                {
                    _mono[_monoCount + i] = 0.5f * (frames[offset + (2 * i)] + frames[offset + (2 * i) + 1]);
                }
                _monoCount += count;
                offset += count * AudioFormat.Channels;
                Decimate();
            }
        }

        // The key of what was added so far. Null when that is too little sound to tell.
        public MusicalKey? Key()
        {
            if (_frames < MinFrames || Notes() is not { } notes)
            {
                return null;
            }
            return KeyProfiles.BestKey(notes);
        }

        // How much of each note there is, 12 per octave from C, as shares of all of it. A track that isn't tuned to 440 Hz has
        // its notes where its own tuning puts them. Null when there are no tones at all.
        private double[]? Notes()
        {
            // How far the peaks are off the semitones on average, around the circle of one semitone.
            double sin = 0;
            double cos = 0;
            for (var bin = 0; bin < Bins; bin++)
            {
                var angle = 2 * Math.PI * (bin % BinsPerSemitone) / BinsPerSemitone;
                sin += _pitches[bin] * Math.Sin(angle);
                cos += _pitches[bin] * Math.Cos(angle);
            }
            var tuning = (int)Math.Round(Math.Atan2(sin, cos) / (2 * Math.PI) * BinsPerSemitone);

            var notes = new double[Octaves * 12];
            double total = 0;
            for (var bin = 0; bin < Bins; bin++)
            {
                // The bins from half a semitone below a note to half a semitone above are that note's.
                var note = (int)Math.Floor((double)(bin - tuning + (BinsPerSemitone / 2)) / BinsPerSemitone);
                if (note >= 0 && note < notes.Length)
                {
                    notes[note] += _pitches[bin];
                    total += _pitches[bin];
                }
            }
            if (!double.IsFinite(total) || total <= 0)
            {
                return null;
            }
            for (var note = 0; note < notes.Length; note++)
            {
                notes[note] /= total;
            }
            return notes;
        }

        private void Decimate()
        {
            var start = 0;
            for (; start + LowpassTaps <= _monoCount; start += Decimation)
            {
                _samples[_sampleCount++] = Dot(_mono.AsSpan(start, LowpassTaps), Lowpass);
                if (_sampleCount == _samples.Length)
                {
                    AnalyzeFrames();
                }
            }
            Array.Copy(_mono, start, _mono, 0, _monoCount - start);
            _monoCount -= start;
            AnalyzeFrames();
        }

        private void AnalyzeFrames()
        {
            var start = 0;
            for (; start + Window <= _sampleCount; start += Hop)
            {
                double power = 0;
                for (var i = 0; i < Window; i++)
                {
                    var sample = _samples[start + i];
                    power += sample * sample;
                    _frame[i] = sample * Hann[i];
                }
                // A frame with samples that aren't numbers, from a broken file, says nothing about the key either.
                if (!double.IsFinite(power) || power / Window < SilencePower)
                {
                    continue;
                }
                _fft.Magnitudes(_frame, _magnitudes);
                AddPeaks();
                _frames++;
            }
            Array.Copy(_samples, start, _samples, 0, _sampleCount - start);
            _sampleCount -= start;
        }

        // Each peak of the spectrum counts for its pitch, by how many times louder it is than its surroundings: a tone that
        // stands out counts, also a quiet one next to a booming kick drum, and noise doesn't.
        private void AddPeaks()
        {
            var magnitudes = _magnitudes;
            for (var k = 0; k < magnitudes.Length; k++)
            {
                _sums[k + 1] = _sums[k] + magnitudes[k];
            }

            float loudest = 0;
            for (var k = LowestBin; k <= HighestBin; k++)
            {
                loudest = Math.Max(loudest, magnitudes[k]);
            }
            var floor = loudest * PeakFloor;
            if (floor <= 0)
            {
                return;
            }

            for (var k = Math.Max(1, LowestBin); k <= HighestBin; k++)
            {
                var magnitude = magnitudes[k];
                if (magnitude < floor || magnitude <= magnitudes[k - 1] || magnitude < magnitudes[k + 1])
                {
                    continue;
                }

                // Between the bins, by a parabola through the logs of the peak and its neighbours.
                var before = Math.Log(magnitudes[k - 1] + 1e-12);
                var at = Math.Log(magnitude);
                var after = Math.Log(magnitudes[k + 1] + 1e-12);
                var bend = before - (2 * at) + after;
                var offset = bend == 0 ? 0 : Math.Clamp(0.5 * (before - after) / bend, -0.5, 0.5);
                var amplitude = Math.Exp(at - (0.25 * (before - after) * offset));
                var hz = (k + offset) * SampleRate / Window;
                var note = ConcertNote + (12 * Math.Log2(hz / ConcertHz));
                var bin = (int)Math.Round((note - LowestNote) * BinsPerSemitone);
                if (bin < 0 || bin >= Bins)
                {
                    continue;
                }

                var from = Math.Max(1, (int)(k / SurroundingsRatio));
                var to = Math.Min(magnitudes.Length, (int)(k * SurroundingsRatio) + 1);
                var surroundings = (_sums[to] - _sums[from]) / (to - from);
                if (surroundings > 0)
                {
                    _pitches[bin] += Math.Max(0, (amplitude / surroundings) - 1);
                }
            }
        }

        private static double HzOf(double note) => ConcertHz * Math.Pow(2, (note - ConcertNote) / 12);

        // A windowed sinc at half amplitude where the new sample rate's Nyquist frequency is.
        private static float[] DesignLowpass()
        {
            var taps = new double[LowpassTaps];
            var cutoff = 0.5 / Decimation;
            double sum = 0;
            for (var i = 0; i < LowpassTaps; i++)
            {
                var x = i - ((LowpassTaps - 1) / 2.0);
                var sinc = x == 0 ? 2 * cutoff : Math.Sin(2 * Math.PI * cutoff * x) / (Math.PI * x);
                var blackman = 0.42 - (0.5 * Math.Cos(2 * Math.PI * i / (LowpassTaps - 1))) + (0.08 * Math.Cos(4 * Math.PI * i / (LowpassTaps - 1)));
                taps[i] = sinc * blackman;
                sum += taps[i];
            }
            return Array.ConvertAll(taps, tap => (float)(tap / sum));
        }

        private static float[] DesignHann()
        {
            var window = new float[Window];
            for (var i = 0; i < Window; i++)
            {
                window[i] = (float)(0.5 - (0.5 * Math.Cos(2 * Math.PI * i / Window)));
            }
            return window;
        }

        private static float Dot(ReadOnlySpan<float> a, ReadOnlySpan<float> b)
        {
            var sums = Vector<float>.Zero;
            var i = 0;
            for (; i <= a.Length - Vector<float>.Count; i += Vector<float>.Count)
            {
                sums += new Vector<float>(a[i..]) * new Vector<float>(b[i..]);
            }
            var sum = Vector.Sum(sums);
            for (; i < a.Length; i++)
            {
                sum += a[i] * b[i];
            }
            return sum;
        }
    }
}
