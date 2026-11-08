using System;
using System.Collections.Generic;
using System.IO;
using LibLala.Audio;

namespace LibLala.Unittests.Audio
{
    // Each sample is the number of its frame, so what plays shows where in the track it is from.
    internal sealed class CountingDecoder : IAudioDecoder
    {
        private long _position;

        public CountingDecoder(long length)
        {
            Length = length;
        }

        public long Length { get; }
        public bool IsDisposed { get; private set; }
        // The frame that Read reads next.
        public long Position => _position;
        // Called before the first Read.
        public Action? BeforeFirstRead { get; set; }
        // Every sample 0.
        public bool Silent { get; init; }
        // Read and Seek throw an IOException from this frame on.
        public long FailsFrom { get; set; } = long.MaxValue;
        // Called by each Seek, like reading the file there.
        public Action? OnSeek { get; set; }

        public int Read(Span<float> buffer)
        {
            BeforeFirstRead?.Invoke();
            BeforeFirstRead = null;
            if (_position >= FailsFrom)
            {
                throw new IOException("The drive is gone.");
            }

            var frames = (int)Math.Min(buffer.Length / AudioFormat.Channels, Length - _position);
            for (var i = 0; i < frames; i++)
            {
                buffer[i * AudioFormat.Channels] = Silent ? 0 : _position + i;
                buffer[(i * AudioFormat.Channels) + 1] = Silent ? 0 : _position + i;
            }
            _position += frames;
            return frames;
        }

        public void Seek(long frame)
        {
            OnSeek?.Invoke();
            if (frame >= FailsFrom)
            {
                throw new IOException("The drive is gone.");
            }
            _position = frame;
        }

        public void Dispose() => IsDisposed = true;
    }

    // A sine, at half of full scale unless said otherwise.
    internal sealed class SineDecoder : IAudioDecoder
    {
        private readonly double _hz;
        private long _position;

        public SineDecoder(double hz, long length)
        {
            _hz = hz;
            Length = length;
        }

        public long Length { get; }
        public double Amplitude { get; init; } = 0.5;

        public int Read(Span<float> buffer)
        {
            var frames = (int)Math.Min(buffer.Length / AudioFormat.Channels, Length - _position);
            for (var i = 0; i < frames; i++)
            {
                var sample = (float)(Amplitude * Math.Sin(2 * Math.PI * _hz * (_position + i) / AudioFormat.SampleRate));
                buffer[i * AudioFormat.Channels] = sample;
                buffer[(i * AudioFormat.Channels) + 1] = sample;
            }
            _position += frames;
            return frames;
        }

        public void Seek(long frame) => _position = frame;

        public void Dispose() { }
    }

    // For tests that render and report played themselves: renders nothing ahead.
    internal sealed class SilentOutput : IAudioOutput
    {
        public float Volume { get; set; } = 1;

        public void Play() { }

        public void Pause() { }

        public void Reset() { }

        public void Dispose() { }
    }

    // Works like AudioQueueOutput, with the queue's thread played by PlayBuffer.
    internal sealed class FakeOutput : IAudioOutput
    {
        public const int FramesPerBuffer = 4;
        public const int BufferCount = 3;

        private readonly IAudioRenderer _renderer;
        private readonly Queue<float[]> _queued = new();

        public FakeOutput(IAudioRenderer renderer)
        {
            _renderer = renderer;
        }

        public float Volume { get; set; } = 1;
        // Started and not paused, the device is held.
        public bool IsRunning { get; private set; }
        public int Starts { get; private set; }
        public bool FailsToPlay { get; set; }

        public void Play()
        {
            if (FailsToPlay)
            {
                throw new IOException("No speakers.");
            }
            if (_queued.Count == 0)
            {
                for (var i = 0; i < BufferCount; i++)
                {
                    _queued.Enqueue(Render());
                }
            }
            if (!IsRunning)
            {
                Starts++;
            }
            IsRunning = true;
        }

        public void Pause() => IsRunning = false;

        public void Reset() => _queued.Clear();

        public void Dispose()
        {
            Pause();
            Reset();
        }

        // Plays the oldest buffer and returns the frame numbers in it. The queue then renders it again.
        public float[] PlayBuffer()
        {
            if (!IsRunning || _queued.Count == 0)
            {
                throw new InvalidOperationException("The output doesn't play.");
            }
            var played = _queued.Dequeue();
            _renderer.Played();
            _queued.Enqueue(Render());

            var frames = new float[FramesPerBuffer];
            for (var i = 0; i < FramesPerBuffer; i++)
            {
                frames[i] = played[i * AudioFormat.Channels];
            }
            return frames;
        }

        private float[] Render()
        {
            var buffer = new float[FramesPerBuffer * AudioFormat.Channels];
            _renderer.Render(buffer);
            return buffer;
        }
    }
}
