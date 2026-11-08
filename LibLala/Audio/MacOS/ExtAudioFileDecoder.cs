using System;
using System.IO;
using System.Runtime.Versioning;
using System.Text;

namespace LibLala.Audio.MacOS
{
    // Decodes with macOS's own codecs, which read mp3 and m4a (AAC and ALAC), and converts to AudioFormat.
    [SupportedOSPlatform("macos")]
    public sealed class ExtAudioFileDecoder : IAudioDecoder
    {
        private readonly ExtAudioFileHandle _file;
        private readonly string _path;
        private readonly double _fileSampleRate;

        public unsafe ExtAudioFileDecoder(string path)
        {
            ArgumentNullException.ThrowIfNull(path);
            _path = path;

            var bytes = Encoding.UTF8.GetBytes(path);
            IntPtr url;
            fixed (byte* buffer = bytes)
            {
                url = NativeMethods.CFURLCreateFromFileSystemRepresentation(IntPtr.Zero, buffer, bytes.Length, isDirectory: false);
            }
            if (url == IntPtr.Zero)
            {
                throw new IOException($"Can't open {path}.");
            }

            int status;
            try
            {
                status = NativeMethods.ExtAudioFileOpenURL(url, out _file);
            }
            finally
            {
                NativeMethods.CFRelease(url);
            }

            try
            {
                Check(status);
                var fileFormat = default(AudioStreamBasicDescription);
                var size = (uint)sizeof(AudioStreamBasicDescription);
                Check(NativeMethods.ExtAudioFileGetProperty(_file, NativeMethods.FileDataFormatProperty, ref size, &fileFormat));
                _fileSampleRate = fileFormat.SampleRate;

                long fileFrames = 0;
                size = sizeof(long);
                Check(NativeMethods.ExtAudioFileGetProperty(_file, NativeMethods.FileLengthFramesProperty, ref size, &fileFrames));
                Length = ToClientFrames(fileFrames);

                var clientFormat = AudioStreamBasicDescription.Float;
                Check(NativeMethods.ExtAudioFileSetProperty(_file, NativeMethods.ClientDataFormatProperty, (uint)sizeof(AudioStreamBasicDescription), &clientFormat));
            }
            catch
            {
                _file.Dispose();
                throw;
            }
        }

        public long Length { get; }

        public unsafe int Read(Span<float> buffer)
        {
            var frames = buffer.Length / AudioFormat.Channels;
            var total = 0;
            fixed (float* data = buffer)
            {
                // A read can stop short of what was asked for before the end.
                while (total < frames)
                {
                    var count = (uint)(frames - total);
                    var list = new AudioBufferList
                    {
                        NumberBuffers = 1,
                        Buffer = new AudioBuffer
                        {
                            NumberChannels = AudioFormat.Channels,
                            DataByteSize = count * sizeof(float) * AudioFormat.Channels,
                            Data = (IntPtr)(data + (total * AudioFormat.Channels)),
                        },
                    };
                    Check(NativeMethods.ExtAudioFileRead(_file, ref count, &list));
                    if (count == 0)
                    {
                        break;
                    }
                    total += (int)count;
                }
            }
            return total;
        }

        public void Seek(long frame) => Check(NativeMethods.ExtAudioFileSeek(_file, (long)Math.Round(frame * _fileSampleRate / AudioFormat.SampleRate)));

        public void Dispose() => _file.Dispose();

        private long ToClientFrames(long fileFrames) => (long)Math.Round(fileFrames * AudioFormat.SampleRate / _fileSampleRate);

        private void Check(int status)
        {
            if (status != 0)
            {
                throw new IOException($"Can't read {_path}: macOS error {NativeMethods.Describe(status)}.");
            }
        }
    }
}
