using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace LibLala.Audio.Windows
{
    // A Media Foundation transform with one stream of audio in and one out: Windows's MP3 decoder or its resampler. Used from
    // one thread at a time, it takes in some and then gives out all it has.
    [SupportedOSPlatform("windows")]
    internal sealed unsafe class MediaTransform : IDisposable
    {
        // What one frame of mp3 makes, or what the resampler makes of it, fits many times. More comes with the next call.
        private const int OutputBytes = 1 << 16;

        private readonly string _path;
        private IntPtr _transform;
        // Given to the transform for its output, every time. Zero when it brings its own.
        private IntPtr _output;
        private IntPtr _outputBuffer;

        // Of the class with the formats, of the file at the path.
        public MediaTransform(Guid type, WaveFormat* input, int inputSize, WaveFormat output, string path)
        {
            _path = path;
            Check(NativeMethods.CoCreateInstance(type, IntPtr.Zero, NativeMethods.InProcessServer, NativeMethods.TransformInterface, out _transform));
            try
            {
                SetType(input, inputSize, isInput: true);
                SetType(&output, sizeof(WaveFormat), isInput: false);

                Check(NativeMethods.GetOutputStreamInfo(_transform, out var info));
                if ((info.Flags & NativeMethods.ProvidesSamples) == 0)
                {
                    Check(NativeMethods.MFCreateMemoryBuffer((uint)Math.Max(OutputBytes, info.Size), out _outputBuffer));
                    Check(NativeMethods.MFCreateSample(out _output));
                    Check(NativeMethods.AddBuffer(_output, _outputBuffer));
                }

                Check(NativeMethods.ProcessMessage(_transform, NativeMethods.BeginStreamingMessage));
                Check(NativeMethods.ProcessMessage(_transform, NativeMethods.StartOfStreamMessage));
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        // Of the input, at the time and for as long, in 100 ns.
        public void Write(ReadOnlySpan<byte> data, long time, long duration)
        {
            Check(NativeMethods.MFCreateMemoryBuffer((uint)data.Length, out var buffer));
            try
            {
                Check(NativeMethods.Lock(buffer, out var into, out _));
                data.CopyTo(new Span<byte>(into, data.Length));
                Check(NativeMethods.Unlock(buffer));
                Check(NativeMethods.SetCurrentLength(buffer, (uint)data.Length));

                Check(NativeMethods.MFCreateSample(out var sample));
                try
                {
                    Check(NativeMethods.AddBuffer(sample, buffer));
                    Check(NativeMethods.SetSampleTime(sample, time));
                    Check(NativeMethods.SetSampleDuration(sample, duration));
                    Check(NativeMethods.ProcessInput(_transform, sample));
                }
                finally
                {
                    Marshal.Release(sample);
                }
            }
            finally
            {
                Marshal.Release(buffer);
            }
        }

        // Appends all the output there is, interleaved float samples.
        public void Read(SampleQueue into)
        {
            while (true)
            {
                if (_outputBuffer != IntPtr.Zero)
                {
                    Check(NativeMethods.SetCurrentLength(_outputBuffer, 0));
                }
                var output = new OutputDataBuffer { Sample = _output };
                var status = NativeMethods.ProcessOutput(_transform, &output);
                if (output.Events != IntPtr.Zero)
                {
                    Marshal.Release(output.Events);
                }
                if (status == NativeMethods.NeedMoreInput)
                {
                    return;
                }
                Check(status);

                try
                {
                    Check(NativeMethods.ConvertToContiguousBuffer(output.Sample, out var buffer));
                    try
                    {
                        Check(NativeMethods.Lock(buffer, out var data, out var length));
                        into.Append(new ReadOnlySpan<float>(data, (int)(length / sizeof(float))));
                        Check(NativeMethods.Unlock(buffer));
                    }
                    finally
                    {
                        Marshal.Release(buffer);
                    }
                }
                finally
                {
                    if (_output == IntPtr.Zero)
                    {
                        Marshal.Release(output.Sample);
                    }
                }
            }
        }

        // Drops what it holds, for input from somewhere else.
        public void Flush() => Check(NativeMethods.ProcessMessage(_transform, NativeMethods.FlushMessage));

        // There's no more input, Read gets what it holds.
        public void Drain() => Check(NativeMethods.ProcessMessage(_transform, NativeMethods.DrainMessage));

        public void Dispose()
        {
            Release(ref _output);
            Release(ref _outputBuffer);
            Release(ref _transform);
        }

        private static void Release(ref IntPtr instance)
        {
            if (instance != IntPtr.Zero)
            {
                Marshal.Release(instance);
                instance = IntPtr.Zero;
            }
        }

        private void SetType(WaveFormat* format, int size, bool isInput)
        {
            Check(NativeMethods.MFCreateMediaType(out var type));
            try
            {
                Check(NativeMethods.MFInitMediaTypeFromWaveFormatEx(type, format, (uint)size));
                Check(isInput ? NativeMethods.SetInputType(_transform, type) : NativeMethods.SetOutputType(_transform, type));
            }
            finally
            {
                Marshal.Release(type);
            }
        }

        private void Check(int status) => MediaFoundation.Check(status, _path);
    }
}
