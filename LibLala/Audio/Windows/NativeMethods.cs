using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace LibLala.Audio.Windows
{
    // The parts of the waveOut API and Media Foundation that the output and the decoders use. Both come with Windows, Media
    // Foundation not with the N editions without the Media Feature Pack.
    [SupportedOSPlatform("windows")]
    internal static unsafe partial class NativeMethods
    {
        private const string Winmm = "winmm.dll";
        private const string MediaFoundation = "mfplat.dll";
        private const string MediaFoundationReader = "mfreadwrite.dll";
        private const string Ole32 = "ole32.dll";

        public const uint WaveMapper = uint.MaxValue;
        public const uint CallbackEvent = 0x50000;
        public const uint WaveHeaderDone = 1;

        public const uint MediaFoundationVersion = 0x20070;
        // Without sockets, only files are read.
        public const uint StartupLite = 1;
        public const uint FirstAudioStream = 0xFFFFFFFD;
        public const uint AllStreams = 0xFFFFFFFE;
        public const uint MediaSource = 0xFFFFFFFF;
        public const uint ReaderError = 0x1;
        public const uint ReaderEndOfStream = 0x2;
        public const ushort VarTypeInt64 = 20;
        public const ushort VarTypeUInt64 = 21;

        public const uint InProcessServer = 1;
        public const uint FlushMessage = 0;
        public const uint DrainMessage = 1;
        public const uint BeginStreamingMessage = 0x10000000;
        public const uint StartOfStreamMessage = 0x10000003;
        public const uint ProvidesSamples = 0x100;
        // MF_E_TRANSFORM_NEED_MORE_INPUT, it gave all it has.
        public const int NeedMoreInput = unchecked((int)0xC00D6D72);

        // MF_PD_DURATION, of the presentation, in 100 ns.
        public static readonly Guid DurationAttribute = new(0x6c990d33, 0xbb8e, 0x477a, 0x85, 0x98, 0x0d, 0x5d, 0x96, 0xfc, 0xd8, 0x8a);
        // CLSID_CMP3DecMediaObject and CLSID_CResamplerMediaObject.
        public static readonly Guid Mp3DecoderClass = new(0xbbeea841, 0x0a63, 0x4f52, 0xa7, 0xab, 0xa9, 0xb3, 0xa8, 0x4e, 0xd3, 0x8a);
        public static readonly Guid ResamplerClass = new(0xf447b69e, 0x1884, 0x4a7e, 0x80, 0x55, 0x34, 0x6f, 0x74, 0xd6, 0xed, 0xb3);
        // IID_IMFTransform.
        public static readonly Guid TransformInterface = new(0xbf94c121, 0x5b05, 0x4e6f, 0x80, 0x00, 0xba, 0x59, 0x89, 0x61, 0x41, 0x4d);

        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [LibraryImport(Winmm, EntryPoint = "waveOutOpen")]
        public static partial uint WaveOutOpen(out IntPtr device, uint deviceId, WaveFormat* format, IntPtr callback, IntPtr instance, uint flags);

        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [LibraryImport(Winmm, EntryPoint = "waveOutPrepareHeader")]
        public static partial uint WaveOutPrepareHeader(IntPtr device, WaveHeader* header, uint size);

        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [LibraryImport(Winmm, EntryPoint = "waveOutUnprepareHeader")]
        public static partial uint WaveOutUnprepareHeader(IntPtr device, WaveHeader* header, uint size);

        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [LibraryImport(Winmm, EntryPoint = "waveOutWrite")]
        public static partial uint WaveOutWrite(IntPtr device, WaveHeader* header, uint size);

        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [LibraryImport(Winmm, EntryPoint = "waveOutPause")]
        public static partial uint WaveOutPause(IntPtr device);

        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [LibraryImport(Winmm, EntryPoint = "waveOutRestart")]
        public static partial uint WaveOutRestart(IntPtr device);

        // Stops right away and hands back every buffer, marked done.
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [LibraryImport(Winmm, EntryPoint = "waveOutReset")]
        public static partial uint WaveOutReset(IntPtr device);

        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [LibraryImport(Winmm, EntryPoint = "waveOutClose")]
        public static partial uint WaveOutClose(IntPtr device);

        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [LibraryImport(MediaFoundation)]
        public static partial int MFStartup(uint version, uint flags);

        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [LibraryImport(MediaFoundation)]
        public static partial int MFCreateMediaType(out IntPtr type);

        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [LibraryImport(MediaFoundation)]
        public static partial int MFInitMediaTypeFromWaveFormatEx(IntPtr type, WaveFormat* format, uint size);

        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [LibraryImport(MediaFoundation)]
        public static partial int MFCreateSample(out IntPtr sample);

        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [LibraryImport(MediaFoundation)]
        public static partial int MFCreateMemoryBuffer(uint maxLength, out IntPtr buffer);

        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [LibraryImport(MediaFoundationReader, StringMarshalling = StringMarshalling.Utf16)]
        public static partial int MFCreateSourceReaderFromURL(string url, IntPtr attributes, out IntPtr reader);

        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [LibraryImport(Ole32)]
        public static partial int CoCreateInstance(in Guid type, IntPtr outer, uint context, in Guid iid, out IntPtr instance);

        // The methods of the COM interfaces, by their place in the vtable. IUnknown's three come first, then those of the
        // interface it derives from.

        // IMFSourceReader: GetStreamSelection, SetStreamSelection, GetNativeMediaType, GetCurrentMediaType,
        // SetCurrentMediaType, SetCurrentPosition, ReadSample, Flush, GetServiceForStream, GetPresentationAttribute.
        public static int SetStreamSelection(IntPtr reader, uint stream, bool isSelected) =>
            ((delegate* unmanaged[Stdcall]<IntPtr, uint, int, int>)Method(reader, 4))(reader, stream, isSelected ? 1 : 0);

        public static int SetCurrentMediaType(IntPtr reader, uint stream, IntPtr type) =>
            ((delegate* unmanaged[Stdcall]<IntPtr, uint, uint*, IntPtr, int>)Method(reader, 7))(reader, stream, null, type);

        // In 100 ns. The reader lands where a sample starts, at or before it.
        public static int SetCurrentPosition(IntPtr reader, long time)
        {
            var timeFormat = Guid.Empty;
            var position = new PropVariant { VarType = VarTypeInt64, Value = time };
            return ((delegate* unmanaged[Stdcall]<IntPtr, Guid*, PropVariant*, int>)Method(reader, 8))(reader, &timeFormat, &position);
        }

        // The sample is null at the end of the stream, its time is in 100 ns.
        public static int ReadSample(IntPtr reader, uint stream, out uint flags, out long time, out IntPtr sample)
        {
            uint actualStream;
            fixed (uint* flagsPointer = &flags)
            fixed (long* timePointer = &time)
            fixed (IntPtr* samplePointer = &sample)
            {
                return ((delegate* unmanaged[Stdcall]<IntPtr, uint, uint, uint*, uint*, long*, IntPtr*, int>)Method(reader, 9))(reader, stream, 0, &actualStream, flagsPointer, timePointer, samplePointer);
            }
        }

        public static int GetPresentationAttribute(IntPtr reader, uint stream, Guid key, PropVariant* value) =>
            ((delegate* unmanaged[Stdcall]<IntPtr, uint, Guid*, PropVariant*, int>)Method(reader, 12))(reader, stream, &key, value);

        // IMFSample, after IMFAttributes' 30: GetSampleFlags, SetSampleFlags, GetSampleTime, SetSampleTime,
        // GetSampleDuration, SetSampleDuration, GetBufferCount, GetBufferByIndex, ConvertToContiguousBuffer, AddBuffer, ...
        public static int SetSampleTime(IntPtr sample, long time) =>
            ((delegate* unmanaged[Stdcall]<IntPtr, long, int>)Method(sample, 36))(sample, time);

        public static int SetSampleDuration(IntPtr sample, long duration) =>
            ((delegate* unmanaged[Stdcall]<IntPtr, long, int>)Method(sample, 38))(sample, duration);

        public static int ConvertToContiguousBuffer(IntPtr sample, out IntPtr buffer)
        {
            fixed (IntPtr* bufferPointer = &buffer)
            {
                return ((delegate* unmanaged[Stdcall]<IntPtr, IntPtr*, int>)Method(sample, 41))(sample, bufferPointer);
            }
        }

        public static int AddBuffer(IntPtr sample, IntPtr buffer) =>
            ((delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int>)Method(sample, 42))(sample, buffer);

        // IMFMediaBuffer: Lock, Unlock, GetCurrentLength, SetCurrentLength, GetMaxLength.
        public static int Lock(IntPtr buffer, out byte* data, out uint length)
        {
            uint maxLength;
            fixed (byte** dataPointer = &data)
            fixed (uint* lengthPointer = &length)
            {
                return ((delegate* unmanaged[Stdcall]<IntPtr, byte**, uint*, uint*, int>)Method(buffer, 3))(buffer, dataPointer, &maxLength, lengthPointer);
            }
        }

        public static int Unlock(IntPtr buffer) => ((delegate* unmanaged[Stdcall]<IntPtr, int>)Method(buffer, 4))(buffer);

        public static int SetCurrentLength(IntPtr buffer, uint length) =>
            ((delegate* unmanaged[Stdcall]<IntPtr, uint, int>)Method(buffer, 6))(buffer, length);

        // IMFTransform: GetStreamLimits, GetStreamCount, GetStreamIDs, GetInputStreamInfo, GetOutputStreamInfo,
        // GetAttributes, GetInputStreamAttributes, GetOutputStreamAttributes, DeleteInputStream, AddInputStreams,
        // GetInputAvailableType, GetOutputAvailableType, SetInputType, SetOutputType, GetInputCurrentType,
        // GetOutputCurrentType, GetInputStatus, GetOutputStatus, SetOutputBounds, ProcessEvent, ProcessMessage,
        // ProcessInput, ProcessOutput.
        public static int GetOutputStreamInfo(IntPtr transform, out OutputStreamInfo info)
        {
            fixed (OutputStreamInfo* infoPointer = &info)
            {
                return ((delegate* unmanaged[Stdcall]<IntPtr, uint, OutputStreamInfo*, int>)Method(transform, 7))(transform, 0, infoPointer);
            }
        }

        public static int SetInputType(IntPtr transform, IntPtr type) =>
            ((delegate* unmanaged[Stdcall]<IntPtr, uint, IntPtr, uint, int>)Method(transform, 15))(transform, 0, type, 0);

        public static int SetOutputType(IntPtr transform, IntPtr type) =>
            ((delegate* unmanaged[Stdcall]<IntPtr, uint, IntPtr, uint, int>)Method(transform, 16))(transform, 0, type, 0);

        public static int ProcessMessage(IntPtr transform, uint message) =>
            ((delegate* unmanaged[Stdcall]<IntPtr, uint, nuint, int>)Method(transform, 23))(transform, message, 0);

        public static int ProcessInput(IntPtr transform, IntPtr sample) =>
            ((delegate* unmanaged[Stdcall]<IntPtr, uint, IntPtr, uint, int>)Method(transform, 24))(transform, 0, sample, 0);

        public static int ProcessOutput(IntPtr transform, OutputDataBuffer* output)
        {
            uint status;
            return ((delegate* unmanaged[Stdcall]<IntPtr, uint, uint, OutputDataBuffer*, uint*, int>)Method(transform, 25))(transform, 0, 1, output, &status);
        }

        private static void* Method(IntPtr instance, int slot) => (*(void***)instance)[slot];
    }

    // WAVEFORMATEX, packed like in C.
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct WaveFormat
    {
        private const ushort IeeeFloat = 3;

        public ushort FormatTag;
        public ushort Channels;
        public uint SamplesPerSecond;
        public uint AverageBytesPerSecond;
        public ushort BlockAlign;
        public ushort BitsPerSample;
        public ushort ExtraSize;

        // AudioFormat's.
        public static WaveFormat Float => FloatOf(AudioFormat.SampleRate, AudioFormat.Channels);

        // Interleaved 32-bit float.
        public static WaveFormat FloatOf(int sampleRate, int channels) => new()
        {
            FormatTag = IeeeFloat,
            Channels = (ushort)channels,
            SamplesPerSecond = (uint)sampleRate,
            AverageBytesPerSecond = (uint)(sampleRate * channels * sizeof(float)),
            BlockAlign = (ushort)(channels * sizeof(float)),
            BitsPerSample = 8 * sizeof(float),
        };
    }

    // MPEGLAYER3WAVEFORMAT, what the MP3 decoder takes.
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct Mp3WaveFormat
    {
        private const ushort MpegLayer3 = 0x55;
        private const ushort MpegId = 1;
        private const uint PaddingOff = 2;

        public WaveFormat Format;
        public ushort Id;
        public uint Flags;
        public ushort BlockSize;
        public ushort FramesPerBlock;
        public ushort CodecDelay;

        public static Mp3WaveFormat Of(int sampleRate, int channels, int bitsPerSecond, int frameBytes) => new()
        {
            Format = new WaveFormat
            {
                FormatTag = MpegLayer3,
                Channels = (ushort)channels,
                SamplesPerSecond = (uint)sampleRate,
                AverageBytesPerSecond = (uint)(bitsPerSecond / 8),
                BlockAlign = 1,
                ExtraSize = 12,
            },
            Id = MpegId,
            Flags = PaddingOff,
            BlockSize = (ushort)frameBytes,
            FramesPerBlock = 1,
        };
    }

    // WAVEHDR.
    [StructLayout(LayoutKind.Sequential)]
    internal struct WaveHeader
    {
        public IntPtr Data;
        public uint BufferLength;
        public uint BytesRecorded;
        public IntPtr User;
        public uint Flags;
        public uint Loops;
        public IntPtr Next;
        public IntPtr Reserved;
    }

    // PROPVARIANT, with room for the largest value on 64 bit. Only ever holds a number here, nothing to clear.
    [StructLayout(LayoutKind.Sequential)]
    internal struct PropVariant
    {
        public ushort VarType;
        public ushort Reserved1;
        public ushort Reserved2;
        public ushort Reserved3;
        public long Value;
        public IntPtr Rest;
    }

    // MFT_OUTPUT_STREAM_INFO.
    [StructLayout(LayoutKind.Sequential)]
    internal struct OutputStreamInfo
    {
        public uint Flags;
        public uint Size;
        public uint Alignment;
    }

    // MFT_OUTPUT_DATA_BUFFER.
    [StructLayout(LayoutKind.Sequential)]
    internal struct OutputDataBuffer
    {
        public uint StreamId;
        public IntPtr Sample;
        public uint Status;
        public IntPtr Events;
    }
}
