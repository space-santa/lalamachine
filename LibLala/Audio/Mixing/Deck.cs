using System;
using System.Collections.Generic;
using SoundTouch;

namespace LibLala.Audio.Mixing
{
    // A track in the mix, at a tempo of its own: read as it is at 1, through SoundTouch otherwise, which keeps its key. Once
    // stretched it stays in SoundTouch, coming out of it would jump by what SoundTouch holds back.
    internal sealed class Deck : IDisposable
    {
        // Small, so that a changing tempo changes in small steps.
        private const int InputFrames = 1024;

        private readonly float[] _input = new float[InputFrames * AudioFormat.Channels];
        // What went into SoundTouch and didn't come out yet, oldest first. SoundTouch puts out frames/tempo for each, within
        // a few ms, measured.
        private readonly Queue<Fed> _fed = new();
        private SoundTouchProcessor? _stretcher;
        // Of the oldest in _fed, the frames that came out already.
        private double _outOfOldest;
        private long _readPosition;
        private bool _isDecoderDone;
        private bool _isFlushed;

        public Deck(IAudioDecoder decoder, BeatGrid? grid, int track)
        {
            Decoder = decoder;
            Grid = grid;
            Track = track;
        }

        public IAudioDecoder Decoder { get; }
        public BeatGrid? Grid { get; }
        public int Track { get; }
        public Filter LowPass { get; } = new(FilterKind.LowPass);
        public Filter HighPass { get; } = new(FilterKind.HighPass);

        // Of the track's own tempo, 1 plays it as it is.
        public double Tempo { get; set; } = 1;

        // Everything was rendered.
        public bool IsEnded { get; private set; }

        // The frame of the track that the next rendered frame comes from.
        public double Position
        {
            get
            {
                if (_stretcher is null || _fed.Count == 0)
                {
                    return _readPosition;
                }
                var oldest = _fed.Peek();
                return oldest.From + (_outOfOldest * oldest.Tempo);
            }
        }

        public void Seek(long frame)
        {
            Decoder.Seek(frame);
            _readPosition = frame;
            _fed.Clear();
            _outOfOldest = 0;
            _isDecoderDone = false;
            _isFlushed = false;
            IsEnded = false;
            _stretcher?.Clear();
            if (Tempo == 1)
            {
                _stretcher = null;
            }
        }

        // Fills the buffer, frames of the track and silence after its end. Returns the frames of the track. Throws an
        // IOException when the decoder does.
        public int Render(Span<float> buffer)
        {
            var frames = buffer.Length / AudioFormat.Channels;
            int rendered;
            if (_stretcher is null && Tempo == 1)
            {
                rendered = Decoder.Read(buffer);
                _readPosition += rendered;
                IsEnded = rendered < frames;
            }
            else
            {
                rendered = RenderStretched(buffer, frames);
            }
            buffer[(rendered * AudioFormat.Channels)..].Clear();
            return rendered;
        }

        public void Dispose() => Decoder.Dispose();

        private int RenderStretched(Span<float> buffer, int frames)
        {
            _stretcher ??= new SoundTouchProcessor { SampleRate = AudioFormat.SampleRate, Channels = AudioFormat.Channels };
            var rendered = 0;
            while (rendered < frames)
            {
                var received = _stretcher.ReceiveSamples(buffer[(rendered * AudioFormat.Channels)..], frames - rendered);
                if (received > 0)
                {
                    TakeOut(received);
                    rendered += received;
                    continue;
                }
                if (_isFlushed)
                {
                    IsEnded = true;
                    break;
                }
                if (_isDecoderDone)
                {
                    // Pads with silence to get the rest out.
                    _stretcher.Flush();
                    _isFlushed = true;
                    continue;
                }

                var read = Decoder.Read(_input);
                if (read > 0)
                {
                    _stretcher.Tempo = Tempo;
                    _stretcher.PutSamples(_input.AsSpan(0, read * AudioFormat.Channels), read);
                    _fed.Enqueue(new Fed(_readPosition, read, Tempo));
                    _readPosition += read;
                }
                _isDecoderDone = read < InputFrames;
            }
            return rendered;
        }

        // Moves the position on by frames that came out of SoundTouch.
        private void TakeOut(int frames)
        {
            double left = frames;
            while (left > 0 && _fed.Count > 0)
            {
                var oldest = _fed.Peek();
                var outOfIt = oldest.Frames / oldest.Tempo;
                var taken = Math.Min(left, outOfIt - _outOfOldest);
                _outOfOldest += taken;
                left -= taken;
                if (_outOfOldest >= outOfIt - 1e-9)
                {
                    _fed.Dequeue();
                    _outOfOldest = 0;
                }
            }
        }

        private readonly record struct Fed(long From, int Frames, double Tempo);
    }
}
