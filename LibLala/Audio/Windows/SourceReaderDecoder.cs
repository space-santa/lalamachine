using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace LibLala.Audio.Windows
{
    // Decodes with Media Foundation's source reader, which puts in the decoder and a resampler that make AudioFormat. Seeks
    // land on the frame in an m4a, within a sample when it has to be resampled. Not in a VBR mp3, see Mp3Decoder.
    [SupportedOSPlatform("windows")]
    internal sealed class SourceReaderDecoder : IAudioDecoder
    {
        // A seek goes this far before the frame and drops what comes before it: an AAC frame needs the one before, and the
        // resampler starts from silence.
        private const int PrerollFrames = 4096;
        // 10 ms, where a sample at AudioFormat's rate and one at 48, 32, 24 or 22.05 kHz start at the same time. A resampler
        // that starts there puts out what it does when reading on, unless the file can only start elsewhere, like AAC.
        private const int SharedSampleFrames = 441;

        private readonly string _path;
        private IntPtr _reader;
        // What the last sample had that Read didn't return yet, interleaved.
        private readonly SampleQueue _pending = new();
        // The frame Read returns next.
        private long _position;
        // The reader lands where a sample starts, at or before where it was asked to go. Lined up by the time of that sample,
        // the first one after opening or seeking, the frames before are skipped. Null once lined up.
        private long? _alignTo = 0;
        private bool _isEndOfStream;

        public unsafe SourceReaderDecoder(string path)
        {
            _path = path;
            Check(NativeMethods.MFCreateSourceReaderFromURL(path, IntPtr.Zero, out _reader));
            try
            {
                Check(NativeMethods.SetStreamSelection(_reader, NativeMethods.AllStreams, isSelected: false));
                Check(NativeMethods.SetStreamSelection(_reader, NativeMethods.FirstAudioStream, isSelected: true));

                Check(NativeMethods.MFCreateMediaType(out var type));
                try
                {
                    var format = WaveFormat.Float;
                    Check(NativeMethods.MFInitMediaTypeFromWaveFormatEx(type, &format, (uint)sizeof(WaveFormat)));
                    Check(NativeMethods.SetCurrentMediaType(_reader, NativeMethods.FirstAudioStream, type));
                }
                finally
                {
                    Marshal.Release(type);
                }

                var duration = default(PropVariant);
                Check(NativeMethods.GetPresentationAttribute(_reader, NativeMethods.MediaSource, NativeMethods.DurationAttribute, &duration));
                if (duration.VarType != NativeMethods.VarTypeUInt64)
                {
                    throw new IOException($"Can't read {path}: Windows doesn't know how long it is.");
                }
                Length = (long)Math.Round(duration.Value * AudioFormat.SampleRate / MediaFoundation.TicksPerSecond);
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        // By the length the file says it has. Read doesn't go past it, and can end before it.
        public long Length { get; }

        public int Read(Span<float> buffer)
        {
            var frames = (int)Math.Clamp(Length - _position, 0, buffer.Length / AudioFormat.Channels);
            var total = 0;
            while (total < frames && (_pending.Count > 0 || ReadSample()))
            {
                var count = Math.Min(frames - total, _pending.Count / AudioFormat.Channels) * AudioFormat.Channels;
                _pending.Samples[..count].CopyTo(buffer[(total * AudioFormat.Channels)..]);
                _pending.Skip(count);
                total += count / AudioFormat.Channels;
            }
            _position += total;
            return total;
        }

        public void Seek(long frame)
        {
            _pending.Clear();
            _position = frame;
            // Past the end the reader can't go, and there's nothing to read anyway.
            _isEndOfStream = frame >= Length;
            _alignTo = _isEndOfStream ? null : frame;
            if (!_isEndOfStream)
            {
                var start = Math.Max(0, frame - PrerollFrames) / SharedSampleFrames * SharedSampleFrames;
                Check(NativeMethods.SetCurrentPosition(_reader, MediaFoundation.Time(start, AudioFormat.SampleRate)));
            }
        }

        public void Dispose()
        {
            if (_reader != IntPtr.Zero)
            {
                Marshal.Release(_reader);
                _reader = IntPtr.Zero;
            }
        }

        // The next sample into _pending. False at the end of the stream.
        private unsafe bool ReadSample()
        {
            while (!_isEndOfStream)
            {
                Check(NativeMethods.ReadSample(_reader, NativeMethods.FirstAudioStream, out var flags, out var time, out var sample));
                if ((flags & NativeMethods.ReaderError) != 0)
                {
                    throw new IOException($"Can't read {_path}: Windows can't decode it any further.");
                }
                _isEndOfStream = (flags & NativeMethods.ReaderEndOfStream) != 0;
                if (sample == IntPtr.Zero)
                {
                    continue;
                }

                try
                {
                    Check(NativeMethods.ConvertToContiguousBuffer(sample, out var buffer));
                    try
                    {
                        Check(NativeMethods.Lock(buffer, out var data, out var length));
                        var frames = (int)(length / sizeof(float) / AudioFormat.Channels);
                        Take(new ReadOnlySpan<float>(data, frames * AudioFormat.Channels), time);
                        Check(NativeMethods.Unlock(buffer));
                    }
                    finally
                    {
                        Marshal.Release(buffer);
                    }
                }
                finally
                {
                    Marshal.Release(sample);
                }
                if (_pending.Count > 0)
                {
                    return true;
                }
            }
            return false;
        }

        // Keeps what of the samples comes at or after the frame to line up with.
        private void Take(ReadOnlySpan<float> samples, long time)
        {
            if (_alignTo is { } alignTo)
            {
                var start = (long)Math.Round(time * AudioFormat.SampleRate / MediaFoundation.TicksPerSecond);
                var skip = (int)Math.Clamp(alignTo - start, 0, samples.Length / AudioFormat.Channels);
                if (skip == samples.Length / AudioFormat.Channels)
                {
                    return;
                }
                samples = samples[(skip * AudioFormat.Channels)..];
                _alignTo = null;
            }
            _pending.Append(samples);
        }

        private void Check(int status) => MediaFoundation.Check(status, _path);
    }
}
