using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32.SafeHandles;

namespace LibLala.Audio.Windows
{
    // The frames of an mp3 file, MPEG Layer III, found as they're needed: where each one starts, to read it and to seek to
    // it exactly. The file is read in chunks, it can be on a share.
    internal sealed class Mp3Frames : IDisposable
    {
        // 144 * 320 kbit/s / 32 kHz, and a byte of padding.
        public const int MaxFrameBytes = 1441;
        // How far it looks for the first frame past the tags, and for the next one past something that isn't a frame.
        private const int SearchBytes = 64 * 1024;
        private const int ChunkBytes = 64 * 1024;
        // A header that matches the first one has these bits like it: the sync, version, layer and sample rate.
        private const uint SameBits = 0xFFFE0C00;

        private static readonly int[] Mpeg1Kilobits = [0, 32, 40, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 256, 320];
        private static readonly int[] Mpeg2Kilobits = [0, 8, 16, 24, 32, 40, 48, 56, 64, 80, 96, 112, 128, 144, 160];
        private static readonly int[] Mpeg1Rates = [44100, 48000, 32000];

        private readonly SafeFileHandle _file;
        private readonly byte[] _chunk = new byte[ChunkBytes];
        // Where each frame of audio starts, of those found so far.
        private readonly List<long> _offsets = [];
        private Header _first;
        // Nothing read yet.
        private long _chunkOffset = -1;
        private int _chunkLength;
        // Where the frame after the last one found starts, or would.
        private long _next;
        private bool _isAllFound;

        private Mp3Frames(SafeFileHandle file)
        {
            _file = file;
        }

        public int SampleRate => _first.SampleRate;
        public int Channels => _first.IsMono ? 1 : 2;
        public int SamplesPerFrame => _first.SamplesPerFrame;
        public int BitsPerSecond => _first.BitsPerSecond;
        public int FirstFrameBytes => _first.Bytes;

        // Of audio, by the VBR header the encoder wrote into the first frame, or else by finding them all.
        public int Count { get; private set; }

        // Null when the file doesn't start like an mp3, past its tags. Throws an IOException when it can't be read.
        public static Mp3Frames? TryOpen(string path)
        {
            SafeFileHandle file;
            try
            {
                // Shared, tags can be written while it plays.
                file = File.OpenHandle(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException)
            {
                throw new IOException($"Can't read {path}: {e.Message}", e);
            }

            var frames = new Mp3Frames(file);
            try
            {
                if (frames.Open())
                {
                    return frames;
                }
            }
            catch
            {
                frames.Dispose();
                throw;
            }
            frames.Dispose();
            return null;
        }

        // Of a frame, null past the last one.
        public long? OffsetOf(int index) => Find(index) ? _offsets[index] : null;

        // Reads the frame into the buffer, MaxFrameBytes fit. Returns its length, 0 past the last one.
        public int Read(int index, Span<byte> buffer)
        {
            if (index >= Count || !Find(index))
            {
                return 0;
            }
            Span<byte> header = stackalloc byte[4];
            ReadAt(_offsets[index], header);
            var bytes = Header.Parse(header)?.Bytes ?? 0;
            return ReadAt(_offsets[index], buffer[..bytes]);
        }

        public void Dispose() => _file.Dispose();

        // False when there's no audio.
        private bool Open()
        {
            if (Search(SkipTags(), null) is not { } first)
            {
                return false;
            }
            _first = first.Header;

            Span<byte> frame = stackalloc byte[MaxFrameBytes];
            var count = VbrFrameCount(frame[..ReadAt(first.Offset, frame[..first.Header.Bytes])], first.Header);
            // The VBR header is in a frame of its own, without audio.
            _next = count is null ? first.Offset : first.Offset + first.Header.Bytes;
            if (count > 0)
            {
                Count = count.Value;
            }
            else
            {
                Count = int.MaxValue;
                while (FindNext())
                {
                }
                Count = _offsets.Count;
            }
            return Count > 0;
        }

        // Of Xing, Info (Xing for a constant bitrate) and VBRI headers. Null when there's none.
        private static int? VbrFrameCount(ReadOnlySpan<byte> frame, Header header)
        {
            var sideInfo = header.SamplesPerFrame == 1152 ? (header.IsMono ? 17 : 32) : (header.IsMono ? 9 : 17);
            var xing = 4 + sideInfo;
            if (frame.Length >= xing + 12 && (frame[xing..(xing + 4)].SequenceEqual("Xing"u8) || frame[xing..(xing + 4)].SequenceEqual("Info"u8)))
            {
                var hasFrames = (BinaryPrimitives.ReadUInt32BigEndian(frame[(xing + 4)..]) & 1) != 0;
                return hasFrames ? (int)Math.Min(BinaryPrimitives.ReadUInt32BigEndian(frame[(xing + 8)..]), int.MaxValue) : 0;
            }
            const int vbri = 4 + 32;
            if (frame.Length >= vbri + 18 && frame[vbri..(vbri + 4)].SequenceEqual("VBRI"u8))
            {
                return (int)Math.Min(BinaryPrimitives.ReadUInt32BigEndian(frame[(vbri + 14)..]), int.MaxValue);
            }
            return null;
        }

        // Past the ID3v2 tags at the start.
        private long SkipTags()
        {
            long offset = 0;
            Span<byte> tag = stackalloc byte[10];
            while (ReadAt(offset, tag) == tag.Length && tag[..3].SequenceEqual("ID3"u8))
            {
                var size = ((tag[6] & 0x7F) << 21) | ((tag[7] & 0x7F) << 14) | ((tag[8] & 0x7F) << 7) | (tag[9] & 0x7F);
                // With a footer, ten more.
                offset += 10 + size + ((tag[5] & 0x10) != 0 ? 10 : 0);
            }
            return offset;
        }

        // The first frame from the offset on, within SearchBytes, that a frame like it follows: one alone can be a few
        // bytes of a picture that look like a header. Like the given header when there is one.
        private (long Offset, Header Header)? Search(long from, Header? like)
        {
            Span<byte> bytes = stackalloc byte[4];
            for (var offset = from; offset < from + SearchBytes; offset++)
            {
                if (ReadAt(offset, bytes) < bytes.Length)
                {
                    return null;
                }
                if (Header.Parse(bytes) is not { } header || (like is { } l && !header.IsLike(l)))
                {
                    continue;
                }
                var read = ReadAt(offset + header.Bytes, bytes);
                // At the end of the file, the last frame.
                if (read == 0 || (read == bytes.Length && Header.Parse(bytes) is { } next && next.IsLike(header)))
                {
                    return (offset, header);
                }
            }
            return null;
        }

        private bool Find(int index)
        {
            while (_offsets.Count <= index)
            {
                if (!FindNext())
                {
                    return false;
                }
            }
            return true;
        }

        private bool FindNext()
        {
            if (_isAllFound || _offsets.Count >= Count)
            {
                return false;
            }
            Span<byte> bytes = stackalloc byte[4];
            if (ReadAt(_next, bytes) == bytes.Length && Header.Parse(bytes) is { } header && header.IsLike(_first))
            {
                _offsets.Add(_next);
                _next += header.Bytes;
                return true;
            }

            // Something that isn't a frame, maybe a tag at the end. More audio can follow it.
            if (Search(_next + 1, _first) is { } found)
            {
                _offsets.Add(found.Offset);
                _next = found.Offset + found.Header.Bytes;
                return true;
            }
            _isAllFound = true;
            return false;
        }

        // Returns how much it read, less than fits only at the end of the file.
        private int ReadAt(long offset, Span<byte> buffer)
        {
            if (offset < _chunkOffset || offset + buffer.Length > _chunkOffset + _chunkLength)
            {
                if (offset == _chunkOffset && _chunkLength < ChunkBytes)
                {
                    // Read from here already, the file ends.
                    buffer = buffer[..Math.Max(0, _chunkLength)];
                }
                else
                {
                    _chunkOffset = offset;
                    _chunkLength = RandomAccess.Read(_file, _chunk, offset);
                }
            }
            var count = (int)Math.Clamp(_chunkOffset + _chunkLength - offset, 0, buffer.Length);
            _chunk.AsSpan((int)(offset - _chunkOffset), count).CopyTo(buffer);
            return count;
        }

        private readonly record struct Header(uint Bits, int SampleRate, int BitsPerSecond, int Bytes, int SamplesPerFrame, bool IsMono)
        {
            // Null when it isn't the header of a Layer III frame.
            public static Header? Parse(ReadOnlySpan<byte> bytes)
            {
                var bits = BinaryPrimitives.ReadUInt32BigEndian(bytes);
                var version = (int)((bits >> 19) & 3);
                var layer = (bits >> 17) & 3;
                var bitrate = (int)((bits >> 12) & 0xF);
                var rate = (int)((bits >> 10) & 3);
                // The sync, Layer III, and none of the values that aren't allowed: version 1, a free or bad bitrate, sample
                // rate 3, emphasis 2.
                if ((bits & 0xFFE00000) != 0xFFE00000 || version == 1 || layer != 1 || bitrate is 0 or 15 || rate == 3 || (bits & 3) == 2)
                {
                    return null;
                }

                // Version 3 is MPEG-1, 2 is MPEG-2 at half the rate, 0 MPEG-2.5 at a quarter.
                var isMpeg1 = version == 3;
                var sampleRate = Mpeg1Rates[rate] >> (isMpeg1 ? 0 : version == 2 ? 1 : 2);
                var bitsPerSecond = 1000 * (isMpeg1 ? Mpeg1Kilobits : Mpeg2Kilobits)[bitrate];
                var padding = (int)((bits >> 9) & 1);
                var bytesPerFrame = ((isMpeg1 ? 144 : 72) * bitsPerSecond / sampleRate) + padding;
                return new Header(bits, sampleRate, bitsPerSecond, bytesPerFrame, isMpeg1 ? 1152 : 576, ((bits >> 6) & 3) == 3);
            }

            public bool IsLike(Header other) => (Bits & SameBits) == (other.Bits & SameBits) && IsMono == other.IsMono;
        }
    }
}
