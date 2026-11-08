using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace LibLala.Audio.Windows
{
    // Decodes an mp3 frame by frame with Windows's MP3 decoder, and resamples with its resampler where the file isn't at
    // AudioFormat's rate. Seeks land on the frame, like reading from the start would: Media Foundation's own source for mp3
    // lands tens of ms off in a VBR file, which puts the beats of a transition off.
    // After a flush, the decoder's output starts with the first frame given to it, and the resampler's with its first
    // sample, measured. A frame takes up to 511 bytes of the frames before it, so a seek decodes from a few frames before.
    [SupportedOSPlatform("windows")]
    internal sealed unsafe class Mp3Decoder : IAudioDecoder
    {
        // The decoder puts out a sample this many later, dropping as many at the start. The last ones never come out.
        private const int DecoderDelay = 528;
        // Decoded before the frame a seek lands in, for the bits it takes from them and for the frame before it.
        private const int PrerollBytes = 1536;
        private const int PrerollFrames = 2;
        // After a seek the resampler starts from silence. It comes out like when reading on after this many, measured 64.
        private const int ResamplerSettleFrames = 256;

        private readonly Mp3Frames _frames;
        private readonly string _path;
        private readonly MediaTransform _decoder;
        // Null when the file is at AudioFormat's rate.
        private readonly MediaTransform? _resampler;
        // A resampler that starts at a multiple of this many samples of the file starts at a whole frame of AudioFormat.
        private readonly int _resamplerStep;
        private readonly byte[] _frame = new byte[Mp3Frames.MaxFrameBytes];
        // At the file's rate, and at AudioFormat's ready to be read. Both in the file's channels.
        private readonly SampleQueue _decoded = new();
        private readonly SampleQueue _ready = new();
        private int _nextFrame;
        // Of the file, of what goes into the resampler next.
        private long _resamplerPosition;
        // What the decoder and the resampler put out first after a seek, before where it went.
        private long _decodedToDrop;
        private long _readyToDrop;
        private bool _isDecoderDrained;
        private bool _isResamplerDrained;
        // The frame Read returns next.
        private long _position;

        // Takes over the frames once it's made.
        private Mp3Decoder(Mp3Frames frames, string path)
        {
            _frames = frames;
            _path = path;
            try
            {
                var input = Mp3WaveFormat.Of(frames.SampleRate, frames.Channels, frames.BitsPerSecond, frames.FirstFrameBytes);
                var decoded = WaveFormat.FloatOf(frames.SampleRate, frames.Channels);
                _decoder = new MediaTransform(NativeMethods.Mp3DecoderClass, &input.Format, sizeof(Mp3WaveFormat), decoded, path);
                if (frames.SampleRate != AudioFormat.SampleRate)
                {
                    _resampler = new MediaTransform(NativeMethods.ResamplerClass, &decoded, sizeof(WaveFormat), WaveFormat.FloatOf(AudioFormat.SampleRate, frames.Channels), path);
                    _resamplerStep = frames.SampleRate / GreatestCommonDivisor(frames.SampleRate, AudioFormat.SampleRate);
                }

                var samples = Math.Max(0, ((long)frames.Count * frames.SamplesPerFrame) - DecoderDelay);
                Length = samples * AudioFormat.SampleRate / frames.SampleRate;
                Start(0);
            }
            catch
            {
                _resampler?.Dispose();
                _decoder?.Dispose();
                throw;
            }
        }

        public long Length { get; }

        // Null when the file doesn't start like an mp3, past its tags. Throws an IOException when it can't be read.
        public static Mp3Decoder? TryOpen(string path)
        {
            Mp3Frames? frames = null;
            try
            {
                frames = Mp3Frames.TryOpen(path);
                if (frames is null)
                {
                    return null;
                }
                var decoder = new Mp3Decoder(frames, path);
                frames = null;
                return decoder;
            }
            finally
            {
                frames?.Dispose();
            }
        }

        public int Read(Span<float> buffer)
        {
            var channels = _frames.Channels;
            var frames = (int)Math.Clamp(Length - _position, 0, buffer.Length / AudioFormat.Channels);
            var total = 0;
            while (total < frames && (_ready.Count > 0 || Produce()))
            {
                var count = Math.Min(_ready.Count / channels, frames - total);
                var from = _ready.Samples;
                var into = buffer[(total * AudioFormat.Channels)..];
                if (channels == AudioFormat.Channels)
                {
                    from[..(count * channels)].CopyTo(into);
                }
                else
                {
                    // Mono, on both sides.
                    for (var i = 0; i < count; i++)
                    {
                        into[i * AudioFormat.Channels] = from[i];
                        into[(i * AudioFormat.Channels) + 1] = from[i];
                    }
                }
                _ready.Skip(count * channels);
                total += count;
            }
            _position += total;
            return total;
        }

        public void Seek(long frame)
        {
            _decoder.Flush();
            _resampler?.Flush();
            Start(frame);
        }

        public void Dispose()
        {
            _resampler?.Dispose();
            _decoder?.Dispose();
            _frames.Dispose();
        }

        private static int GreatestCommonDivisor(int a, int b) => b == 0 ? a : GreatestCommonDivisor(b, a % b);

        // From the frame of AudioFormat on, with the decoder and the resampler flushed.
        private void Start(long frame)
        {
            _decoded.Clear();
            _ready.Clear();
            _position = frame;
            _isResamplerDrained = _resampler is null;
            // Past the end there's nothing to decode.
            _isDecoderDrained = frame >= Length;
            if (_isDecoderDrained)
            {
                return;
            }

            // Where in the file the output starts, before the frame for the resampler to settle. At a whole frame of
            // AudioFormat, for its samples to come out exactly where they do when reading on.
            var rate = _frames.SampleRate;
            long start = frame;
            if (_resampler is not null)
            {
                var settled = (frame - ResamplerSettleFrames) * rate / AudioFormat.SampleRate;
                start = Math.Max(0, settled / _resamplerStep * _resamplerStep);
            }
            _readyToDrop = frame - (start * AudioFormat.SampleRate / rate);
            _resamplerPosition = start;

            var target = (int)(start / _frames.SamplesPerFrame);
            var first = target;
            if (_frames.OffsetOf(target) is { } targetOffset)
            {
                while (first > 0 && (target - first < PrerollFrames || targetOffset - _frames.OffsetOf(first) < PrerollBytes))
                {
                    first--;
                }
            }
            _nextFrame = first;
            _decodedToDrop = start - ((long)first * _frames.SamplesPerFrame);
        }

        // Puts more into _ready, false at the end.
        private bool Produce()
        {
            while (_ready.Count == 0)
            {
                if (_decoded.Count > 0)
                {
                    if (_resampler is null)
                    {
                        _ready.Append(_decoded.Samples);
                    }
                    else
                    {
                        var samples = _decoded.Count / _frames.Channels;
                        var time = MediaFoundation.Time(_resamplerPosition, _frames.SampleRate);
                        _resampler.Write(MemoryMarshal.AsBytes(_decoded.Samples), time, MediaFoundation.Time(_resamplerPosition + samples, _frames.SampleRate) - time);
                        _resamplerPosition += samples;
                        _resampler.Read(_ready);
                    }
                    _decoded.Clear();
                }
                else if (!_isDecoderDrained)
                {
                    Decode();
                    _decoded.Skip(Drop(ref _decodedToDrop, _decoded.Count / _frames.Channels));
                    continue;
                }
                else if (!_isResamplerDrained)
                {
                    _resampler!.Drain();
                    _resampler.Read(_ready);
                    _isResamplerDrained = true;
                }
                else
                {
                    return false;
                }
                _ready.Skip(Drop(ref _readyToDrop, _ready.Count / _frames.Channels));
            }
            return true;
        }

        // The next frame into the decoder, or the end, and what it puts out into _decoded.
        private void Decode()
        {
            var bytes = _frames.Read(_nextFrame, _frame);
            if (bytes == 0)
            {
                _decoder.Drain();
                _isDecoderDrained = true;
            }
            else
            {
                var samplesPerFrame = _frames.SamplesPerFrame;
                var time = MediaFoundation.Time((long)_nextFrame * samplesPerFrame, _frames.SampleRate);
                _decoder.Write(_frame.AsSpan(0, bytes), time, MediaFoundation.Time((long)(_nextFrame + 1) * samplesPerFrame, _frames.SampleRate) - time);
                _nextFrame++;
            }
            _decoder.Read(_decoded);
        }

        // How many samples of the frames there are to drop, counted off.
        private int Drop(ref long toDrop, int frames)
        {
            var dropped = (int)Math.Min(toDrop, frames);
            toDrop -= dropped;
            return dropped * _frames.Channels;
        }
    }
}
