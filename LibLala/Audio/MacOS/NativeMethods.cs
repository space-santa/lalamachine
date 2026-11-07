using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Win32.SafeHandles;

namespace LibLala.Audio.MacOS
{
    // The parts of AudioToolbox and CoreFoundation that the decoder and the output use. Both come with macOS.
    [SupportedOSPlatform("macos")]
    internal static unsafe partial class NativeMethods
    {
        private const string AudioToolbox = "/System/Library/Frameworks/AudioToolbox.framework/AudioToolbox";
        private const string CoreFoundation = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";

        public const uint FileDataFormatProperty = 0x66666d74; // 'ffmt'
        public const uint ClientDataFormatProperty = 0x63666d74; // 'cfmt'
        public const uint FileLengthFramesProperty = 0x2366726d; // '#frm'
        public const uint VolumeParameter = 1;

        [LibraryImport(CoreFoundation)]
        public static partial IntPtr CFURLCreateFromFileSystemRepresentation(IntPtr allocator, byte* buffer, nint length, [MarshalAs(UnmanagedType.U1)] bool isDirectory);

        [LibraryImport(CoreFoundation)]
        public static partial void CFRelease(IntPtr value);

        [LibraryImport(AudioToolbox)]
        public static partial int ExtAudioFileOpenURL(IntPtr url, out ExtAudioFileHandle file);

        [LibraryImport(AudioToolbox)]
        public static partial int ExtAudioFileGetProperty(ExtAudioFileHandle file, uint property, ref uint size, void* data);

        [LibraryImport(AudioToolbox)]
        public static partial int ExtAudioFileSetProperty(ExtAudioFileHandle file, uint property, uint size, void* data);

        [LibraryImport(AudioToolbox)]
        public static partial int ExtAudioFileRead(ExtAudioFileHandle file, ref uint frames, AudioBufferList* data);

        // In frames of the file, not of the client format.
        [LibraryImport(AudioToolbox)]
        public static partial int ExtAudioFileSeek(ExtAudioFileHandle file, long frame);

        [LibraryImport(AudioToolbox)]
        public static partial int ExtAudioFileDispose(IntPtr file);

        [LibraryImport(AudioToolbox)]
        public static partial int AudioQueueNewOutput(AudioStreamBasicDescription* format, delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr, void> callback, IntPtr userData, IntPtr runLoop, IntPtr runLoopMode, uint flags, out IntPtr queue);

        [LibraryImport(AudioToolbox)]
        public static partial int AudioQueueAllocateBuffer(IntPtr queue, uint byteSize, out IntPtr buffer);

        [LibraryImport(AudioToolbox)]
        public static partial int AudioQueueEnqueueBuffer(IntPtr queue, IntPtr buffer, uint packetDescriptionCount, IntPtr packetDescriptions);

        [LibraryImport(AudioToolbox)]
        public static partial int AudioQueueStart(IntPtr queue, IntPtr startTime);

        [LibraryImport(AudioToolbox)]
        public static partial int AudioQueuePause(IntPtr queue);

        // Immediately: synchronously, after the callbacks for the dropped buffers.
        [LibraryImport(AudioToolbox)]
        public static partial int AudioQueueStop(IntPtr queue, [MarshalAs(UnmanagedType.U1)] bool immediately);

        // Drops the buffers, synchronously after their callbacks.
        [LibraryImport(AudioToolbox)]
        public static partial int AudioQueueReset(IntPtr queue);

        [LibraryImport(AudioToolbox)]
        public static partial int AudioQueueDispose(IntPtr queue, [MarshalAs(UnmanagedType.U1)] bool immediately);

        [LibraryImport(AudioToolbox)]
        public static partial int AudioQueueSetParameter(IntPtr queue, uint parameter, float value);

        // An OSStatus is often four characters, like 'fmt?'.
        public static string Describe(int status)
        {
            Span<char> chars = [(char)((status >> 24) & 0xFF), (char)((status >> 16) & 0xFF), (char)((status >> 8) & 0xFF), (char)(status & 0xFF)];
            foreach (var c in chars)
            {
                if (c < ' ' || c > '~')
                {
                    return status.ToString(System.Globalization.CultureInfo.InvariantCulture);
                }
            }
            return $"'{new string(chars)}'";
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AudioStreamBasicDescription
    {
        private const uint LinearPcm = 0x6c70636d; // 'lpcm'
        private const uint IsFloat = 1;
        private const uint IsPacked = 8;

        public double SampleRate;
        public uint FormatId;
        public uint FormatFlags;
        public uint BytesPerPacket;
        public uint FramesPerPacket;
        public uint BytesPerFrame;
        public uint ChannelsPerFrame;
        public uint BitsPerChannel;
        public uint Reserved;

        // AudioFormat's.
        public static AudioStreamBasicDescription Float => new()
        {
            SampleRate = AudioFormat.SampleRate,
            FormatId = LinearPcm,
            FormatFlags = IsFloat | IsPacked,
            BytesPerPacket = sizeof(float) * AudioFormat.Channels,
            FramesPerPacket = 1,
            BytesPerFrame = sizeof(float) * AudioFormat.Channels,
            ChannelsPerFrame = AudioFormat.Channels,
            BitsPerChannel = 8 * sizeof(float),
        };
    }

    // With one buffer, which is enough for interleaved audio. Nested like in C, the buffer starts at the pointer's alignment.
    [StructLayout(LayoutKind.Sequential)]
    internal struct AudioBufferList
    {
        public uint NumberBuffers;
        public AudioBuffer Buffer;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AudioBuffer
    {
        public uint NumberChannels;
        public uint DataByteSize;
        public IntPtr Data;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AudioQueueBuffer
    {
        public uint AudioDataBytesCapacity;
        public IntPtr AudioData;
        public uint AudioDataByteSize;
    }

    [SupportedOSPlatform("macos")]
    internal sealed class ExtAudioFileHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public ExtAudioFileHandle() : base(ownsHandle: true) { }

        protected override bool ReleaseHandle() => NativeMethods.ExtAudioFileDispose(handle) == 0;
    }
}
