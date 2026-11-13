using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;

namespace LibLala.Audio.Windows
{
    // Plays through waveOut to Windows's default output, and moves along when that changes. The device sets an event when it
    // played a buffer, the output's thread then renders it again and gives it back.
    [SupportedOSPlatform("windows")]
    public sealed unsafe class WaveOutOutput : IAudioOutput
    {
        // Three buffers of 46 ms, like on macOS: what Reset drops is at most that much ahead, and Render has 92 ms to come back.
        private const int BufferCount = 3;
        private const int FramesPerBuffer = 2048;
        private const int SamplesPerBuffer = FramesPerBuffer * AudioFormat.Channels;

        private readonly Lock _gate = new();
        private readonly IAudioRenderer _renderer;
        // Set by the device when it's done with a buffer, and by Dispose.
        private readonly AutoResetEvent _done = new(false);
        private readonly Thread _thread;
        // Each a WaveHeader and its samples after it, in native memory: the device reads them while they're queued.
        private readonly IntPtr[] _buffers = new IntPtr[BufferCount];
        // Of the buffers in the device, oldest first.
        private readonly Queue<int> _queued = new();
        // Opened by Play, and closed again when Play fails: the next one may find a device.
        private IntPtr _device;
        // The buffers were rendered and given to the device. Reset takes them back.
        private bool _hasBuffers;
        private bool _isRunning;
        private bool _isDisposed;
        private float _volume = 1;
        // Of the end of the last buffer. The next one goes from it to the volume, a jump would click.
        private float _gain = 1;

        public WaveOutOutput(IAudioRenderer renderer)
        {
            ArgumentNullException.ThrowIfNull(renderer);
            _renderer = renderer;
            for (var i = 0; i < BufferCount; i++)
            {
                var header = (WaveHeader*)NativeMemory.AllocZeroed((nuint)(sizeof(WaveHeader) + (SamplesPerBuffer * sizeof(float))));
                header->Data = (IntPtr)(header + 1);
                header->BufferLength = SamplesPerBuffer * sizeof(float);
                _buffers[i] = (IntPtr)header;
            }
            _thread = new Thread(Run) { IsBackground = true, Priority = ThreadPriority.Highest, Name = "Audio output" };
            _thread.Start();
        }

        // Applied as the buffers are rendered, it's heard after what the device has queued.
        public float Volume
        {
            get => Volatile.Read(ref _volume);
            set => Volatile.Write(ref _volume, Math.Clamp(value, 0, 1));
        }

        public void Play()
        {
            lock (_gate)
            {
                ObjectDisposedException.ThrowIf(_isDisposed, this);
                try
                {
                    Open();
                    if (!_hasBuffers)
                    {
                        // Before, so that Reset takes back what got to the device when one fails.
                        _hasBuffers = true;
                        // Paused while they go in, so that they play as one from the first on.
                        Check(NativeMethods.WaveOutPause(_device));
                        for (var i = 0; i < BufferCount; i++)
                        {
                            RenderAndWrite(i);
                        }
                    }
                    Check(NativeMethods.WaveOutRestart(_device));
                    _isRunning = true;
                }
                catch (IOException)
                {
                    Close();
                    throw;
                }
            }
        }

        public void Pause()
        {
            lock (_gate)
            {
                if (!_isRunning || _isDisposed)
                {
                    return;
                }
                _isRunning = false;
                _ = NativeMethods.WaveOutPause(_device);
            }
        }

        public void Reset()
        {
            // Taking the lock also waits for the thread while it renders.
            lock (_gate)
            {
                if (!_hasBuffers || _isDisposed)
                {
                    return;
                }
                _hasBuffers = false;
                // The device hands them back marked done. The thread doesn't find them queued, they don't count as played.
                _queued.Clear();
                _ = NativeMethods.WaveOutReset(_device);
            }
        }

        public void Dispose()
        {
            lock (_gate)
            {
                if (_isDisposed)
                {
                    return;
                }
                _isDisposed = true;
                Close();
            }

            // The thread stops, and nothing uses the buffers any more.
            _done.Set();
            _thread.Join();
            _done.Dispose();
            foreach (var buffer in _buffers)
            {
                NativeMemory.Free((void*)buffer);
            }
        }

        private static void Check(uint result)
        {
            if (result != 0)
            {
                throw new IOException($"Can't play audio: Windows error {result}.");
            }
        }

        private void Open()
        {
            if (_device != IntPtr.Zero)
            {
                return;
            }
            var format = WaveFormat.Float;
            Check(NativeMethods.WaveOutOpen(out _device, NativeMethods.WaveMapper, &format, _done.SafeWaitHandle.DangerousGetHandle(), IntPtr.Zero, NativeMethods.CallbackEvent));
            foreach (var buffer in _buffers)
            {
                ((WaveHeader*)buffer)->Flags = 0;
                Check(NativeMethods.WaveOutPrepareHeader(_device, (WaveHeader*)buffer, (uint)sizeof(WaveHeader)));
            }
        }

        private void Close()
        {
            if (_device == IntPtr.Zero)
            {
                return;
            }
            _ = NativeMethods.WaveOutReset(_device);
            foreach (var buffer in _buffers)
            {
                _ = NativeMethods.WaveOutUnprepareHeader(_device, (WaveHeader*)buffer, (uint)sizeof(WaveHeader));
            }
            _ = NativeMethods.WaveOutClose(_device);
            _device = IntPtr.Zero;
            _queued.Clear();
            _hasBuffers = false;
            _isRunning = false;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "An exception that leaves the output's thread ends the app.")]
        private void Run()
        {
            while (true)
            {
                _done.WaitOne();
                lock (_gate)
                {
                    if (_isDisposed)
                    {
                        return;
                    }

                    // The device is done with them in the order they went in.
                    while (_queued.TryPeek(out var i) && (Volatile.Read(ref ((WaveHeader*)_buffers[i])->Flags) & NativeMethods.WaveHeaderDone) != 0)
                    {
                        _queued.Dequeue();
                        try
                        {
                            _renderer.Played();
                            RenderAndWrite(i);
                        }
                        catch (Exception)
                        {
                            // The buffer stays out, it plays on with the others.
                        }
                    }
                }
            }
        }

        private void RenderAndWrite(int i)
        {
            var header = (WaveHeader*)_buffers[i];
            var samples = new Span<float>((void*)header->Data, SamplesPerBuffer);
            _renderer.Render(samples);
            ApplyVolume(samples);
            header->Flags &= ~NativeMethods.WaveHeaderDone;
            Check(NativeMethods.WaveOutWrite(_device, header, (uint)sizeof(WaveHeader)));
            _queued.Enqueue(i);
        }

        private void ApplyVolume(Span<float> samples)
        {
            var from = _gain;
            var to = Volatile.Read(ref _volume);
            _gain = to;
            if (from == 1 && to == 1)
            {
                return;
            }
            for (var frame = 0; frame < FramesPerBuffer; frame++)
            {
                var gain = from + ((to - from) * (frame + 1) / FramesPerBuffer);
                samples[frame * AudioFormat.Channels] *= gain;
                samples[(frame * AudioFormat.Channels) + 1] *= gain;
            }
        }
    }
}
