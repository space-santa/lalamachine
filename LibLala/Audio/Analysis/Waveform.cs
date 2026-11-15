using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LibLala.Audio.Analysis
{
    // How loud a track is along its length, to draw it: the RMS of both channels in each slice of SliceFrames. Not the
    // peaks, they are near full scale all through a mastered track.
    public sealed class Waveform
    {
        // Goes up when what's stored changes, so that the waveforms are read again.
        public const int Version = 1;
        // 50 ms. A track of 6 minutes has 7200 slices, a few for each bar even on a wide screen.
        public const int SliceFrames = AudioFormat.SampleRate / 20;
        private const int SlicesPerRead = 8;
        // Stored as a byte for each slice, a share of the loudest one: 7 kB for 6 minutes. A step is a fifth of a pixel in
        // a waveform 48 pixels high.
        private const float StoredLoudest = byte.MaxValue;

        private readonly float[] _levels;

        internal Waveform(float[] levels)
        {
            _levels = levels;
        }

        public static double SliceSeconds => (double)SliceFrames / AudioFormat.SampleRate;

        public int SliceCount => _levels.Length;

        // The whole track, till the decoder has no more: its length may be an estimate.
        public static Waveform Read(IAudioDecoder decoder, CancellationToken token)
        {
            ArgumentNullException.ThrowIfNull(decoder);

            var waveform = new WaveformBuilder();
            var block = new float[SliceFrames * SlicesPerRead * AudioFormat.Channels];
            int read;
            while ((read = decoder.Read(block)) > 0)
            {
                token.ThrowIfCancellationRequested();
                waveform.Add(block.AsSpan(0, read * AudioFormat.Channels));
            }
            return waveform.Build();
        }

        // As ToBytes stored it.
        public static Waveform FromBytes(ReadOnlySpan<byte> stored)
        {
            var levels = new float[stored.Length];
            for (var s = 0; s < stored.Length; s++)
            {
                levels[s] = stored[s] / StoredLoudest;
            }
            return new Waveform(levels);
        }

        // Each slice as a share of the loudest one, from 0 to 255. Shown, it looks the same.
        public byte[] ToBytes()
        {
            var stored = new byte[_levels.Length];
            var loudest = _levels.Length > 0 ? _levels.Max() : 0;
            if (loudest > 0)
            {
                for (var s = 0; s < _levels.Length; s++)
                {
                    stored[s] = (byte)Math.Round(_levels[s] / loudest * StoredLoudest);
                }
            }
            return stored;
        }

        // The first seconds of the track drawn as count bars side by side, each as long as the others: the RMS of each, as
        // a share of the loudest bar's. Bars past its end are 0, and all are 0 in a silent track.
        public float[] Bars(int count, double seconds)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(count);

            var bars = new float[count];
            if (seconds <= 0)
            {
                return bars;
            }
            var slicesPerBar = seconds / SliceSeconds / count;
            float loudest = 0;
            for (var b = 0; b < count; b++)
            {
                var from = (int)Math.Min(SliceCount, Math.Floor(b * slicesPerBar));
                // At least one slice, when bars are shorter than slices.
                var to = (int)Math.Min(SliceCount, Math.Max(from + 1, Math.Floor((b + 1) * slicesPerBar)));
                double squares = 0;
                for (var s = from; s < to; s++)
                {
                    squares += (double)_levels[s] * _levels[s];
                }
                bars[b] = to > from ? (float)Math.Sqrt(squares / (to - from)) : 0;
                loudest = Math.Max(loudest, bars[b]);
            }
            if (loudest > 0)
            {
                for (var b = 0; b < count; b++)
                {
                    bars[b] /= loudest;
                }
            }
            return bars;
        }
    }

    // The waveform of a track from its frames as they are read: for the analysis, which reads the track anyway.
    public sealed class WaveformBuilder
    {
        // Not as many as the decoder's length says: that may be an estimate, or way off in a broken file.
        private readonly List<float> _levels = [];
        private double _squares;
        private int _frames;

        // The next frames of the track, as AudioFormat has them: whole frames, left and right.
        public void Add(ReadOnlySpan<float> frames)
        {
            for (var i = 0; i + 1 < frames.Length; i += AudioFormat.Channels)
            {
                var left = frames[i];
                var right = frames[i + 1];
                _squares += ((double)left * left) + ((double)right * right);
                if (++_frames == Waveform.SliceFrames)
                {
                    _levels.Add(Rms(_squares, _frames));
                    _squares = 0;
                    _frames = 0;
                }
            }
        }

        // Of what was added, the last slice what's left of the track.
        public Waveform Build() => new(_frames > 0 ? [.. _levels, Rms(_squares, _frames)] : [.. _levels]);

        private static float Rms(double squares, int frames) => (float)Math.Sqrt(squares / (frames * AudioFormat.Channels));
    }
}
