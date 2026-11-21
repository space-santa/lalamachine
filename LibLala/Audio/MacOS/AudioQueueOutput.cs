using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;

namespace LibLala.Audio.MacOS
{
    // Plays through an AudioQueue to the Mac's current output. The queue calls back on its own thread whenever it played a
    // buffer, which is then rendered again and put back in the queue.
    [SupportedOSPlatform("macos")]
    public sealed class AudioQueueOutput : IAudioOutput
    {
        // Three buffers of 46 ms: what Reset drops is at most that much ahead, and Render has 92 ms to come back.
        private const int BufferCount = 3;
        private const int FramesPerBuffer = 2048;

        private readonly Lock _gate = new();
        private readonly IAudioRenderer _renderer;
        private readonly IntPtr _queue;
        private readonly IntPtr[] _buffers = new IntPtr[BufferCount];
        private GCHandle _self;
        // The buffers are in the queue, rendered. Reset takes them out.
        private bool _hasBuffers;
        // Started and not paused. Starting takes 20 to 100 ms, so Reset leaves the queue running for the Play that follows.
        private bool _isRunning;
        // While the queue drops its buffers, the callbacks for them must not render and put them back.
        private bool _isResetting;
        // The queue refused a buffer: the others play out, and nothing is rendered until Reset.
        private bool _isFailed;
        private bool _isDisposed;
        private float _volume = 1;

        public unsafe AudioQueueOutput(IAudioRenderer renderer)
        {
            ArgumentNullException.ThrowIfNull(renderer);
            _renderer = renderer;
            _self = GCHandle.Alloc(this);

            var format = AudioStreamBasicDescription.Float;
            var status = NativeMethods.AudioQueueNewOutput(&format, &OnBufferPlayed, GCHandle.ToIntPtr(_self), IntPtr.Zero, IntPtr.Zero, 0, out _queue);
            if (status != 0)
            {
                _self.Free();
                throw new IOException($"Can't play audio: macOS error {NativeMethods.Describe(status)}.");
            }

            try
            {
                for (var i = 0; i < BufferCount; i++)
                {
                    Check(NativeMethods.AudioQueueAllocateBuffer(_queue, FramesPerBuffer * AudioFormat.Channels * sizeof(float), out _buffers[i]));
                }
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        public float Volume
        {
            get => _volume;
            set
            {
                _volume = Math.Clamp(value, 0, 1);
                _ = NativeMethods.AudioQueueSetParameter(_queue, NativeMethods.VolumeParameter, _volume);
            }
        }

        public void Play()
        {
            // After it failed, what's still in the queue comes out first: all of it is rendered anew.
            bool isFailed;
            lock (_gate)
            {
                isFailed = _isFailed;
            }
            if (isFailed)
            {
                Reset();
            }

            bool isStarting;
            lock (_gate)
            {
                ObjectDisposedException.ThrowIf(_isDisposed, this);
                if (!_hasBuffers)
                {
                    // Before, so that Reset takes out what got into the queue when one fails.
                    _hasBuffers = true;
                    foreach (var buffer in _buffers)
                    {
                        RenderAndEnqueue(buffer);
                    }
                }
                isStarting = !_isRunning;
                _isRunning = true;
            }

            var status = isStarting ? NativeMethods.AudioQueueStart(_queue, IntPtr.Zero) : 0;
            if (status != 0)
            {
                Pause();
                Reset();
                throw new IOException($"Can't play audio: macOS error {NativeMethods.Describe(status)}.");
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
            }
            _ = NativeMethods.AudioQueuePause(_queue);
        }

        public void Reset()
        {
            // Taking the lock also waits for a callback that is rendering right now.
            lock (_gate)
            {
                if (!_hasBuffers || _isDisposed)
                {
                    return;
                }
                _isResetting = true;
            }

            // Not holding the lock: this waits for the callbacks of the dropped buffers, and they take it.
            _ = NativeMethods.AudioQueueReset(_queue);

            lock (_gate)
            {
                _isResetting = false;
                _hasBuffers = false;
                _isFailed = false;
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
            }

            // Also frees the buffers. Its callbacks for them see _isDisposed, and none come after it returns.
            _ = NativeMethods.AudioQueueDispose(_queue, immediately: true);
            _self.Free();
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        private static void OnBufferPlayed(IntPtr self, IntPtr queue, IntPtr buffer)
        {
            if (GCHandle.FromIntPtr(self).Target is AudioQueueOutput output)
            {
                output.BufferPlayed(buffer);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "An exception that leaves a callback from native code ends the app.")]
        private void BufferPlayed(IntPtr buffer)
        {
            lock (_gate)
            {
                if (_isResetting || _isDisposed || !_hasBuffers || _isFailed)
                {
                    return;
                }

                try
                {
                    _renderer.Played();
                    RenderAndEnqueue(buffer);
                }
                catch (Exception)
                {
                    Fail();
                }
            }
        }

        // The queue refuses buffers, the device is gone: the others would play out, and then nothing.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "An exception that leaves a callback from native code ends the app.")]
        private void Fail()
        {
            _isFailed = true;
            try
            {
                _renderer.OutputFailed();
            }
            catch (Exception)
            {
                // The output stops rendering all the same.
            }
        }

        private unsafe void RenderAndEnqueue(IntPtr buffer)
        {
            var queueBuffer = (AudioQueueBuffer*)buffer;
            var samples = new Span<float>((void*)queueBuffer->AudioData, (int)(queueBuffer->AudioDataBytesCapacity / sizeof(float)));
            _renderer.Render(samples);
            queueBuffer->AudioDataByteSize = queueBuffer->AudioDataBytesCapacity;
            Check(NativeMethods.AudioQueueEnqueueBuffer(_queue, buffer, 0, IntPtr.Zero));
        }

        private static void Check(int status)
        {
            if (status != 0)
            {
                throw new IOException($"Can't play audio: macOS error {NativeMethods.Describe(status)}.");
            }
        }
    }
}
