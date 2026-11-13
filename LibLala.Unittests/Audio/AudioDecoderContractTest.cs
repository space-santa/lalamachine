using System;
using System.IO;
using System.Runtime.InteropServices;
using LibLala.Audio;
using NUnit.Framework;
using TestHelpers;

namespace LibLala.Unittests.Audio
{
    // What every decoder does, with the OS's codecs, on files written here.
    public abstract class AudioDecoderContractTest
    {
        protected string Dir { get; private set; } = "";

        // Why the decoder can't decode here, null when it can.
        protected abstract string? Unsupported { get; }

        [SetUp]
        public void SetUp()
        {
            if (Unsupported is { } reason)
            {
                Assert.Ignore(reason);
            }
            Dir = Path.Combine(Path.GetTempPath(), $"lalatest-{Guid.NewGuid()}");
            Directory.CreateDirectory(Dir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(Dir))
            {
                Directory.Delete(Dir, recursive: true);
            }
        }

        [Test]
        public void Mp3_IsReadToItsLength()
        {
            var path = Path.Combine(Dir, "silence.mp3");
            TestMp3.Write(path);

            using var decoder = Open(path);

            // 120 frames of 1152 samples, less the decoder's delay.
            Assert.That(decoder.Length, Is.InRange(130000, 138240));
            Assert.That(ReadAll(decoder).Length / AudioFormat.Channels, Is.EqualTo(decoder.Length));
        }

        [Test]
        public void AnotherSampleRate_IsConvertedTo44100()
        {
            var path = Path.Combine(Dir, "a.wav");
            WriteSine(path, sampleRate: 48000, frames: 96000);

            using var decoder = Open(path);

            Assert.That(decoder.Length, Is.EqualTo(88200));
            Assert.That(ReadAll(decoder).Length / AudioFormat.Channels, Is.EqualTo(88200).Within(1));
        }

        [Test]
        public void Seek_LandsOnTheFrame()
        {
            var path = Path.Combine(Dir, "a.wav");
            WriteSine(path, sampleRate: 48000, frames: 96000);
            using var decoder = Open(path);
            var all = ReadAll(decoder);

            // Frame 16000 of the file.
            const int frame = 14700;
            decoder.Seek(frame);
            var buffer = new float[1000 * AudioFormat.Channels];
            decoder.Read(buffer);

            // The converter starts without the samples before, its first few frames differ a little.
            const int settled = 16 * AudioFormat.Channels;
            Assert.That(buffer[settled..], Is.EqualTo(all.AsSpan((frame * AudioFormat.Channels) + settled, buffer.Length - settled).ToArray()).Within(0.001));
        }

        [Test]
        public void Read_AfterSeek_ReturnsTheRest()
        {
            var path = Path.Combine(Dir, "silence.mp3");
            TestMp3.Write(path);
            using var decoder = Open(path);

            decoder.Seek(decoder.Length / 2);

            Assert.That(ReadAll(decoder).Length / AudioFormat.Channels, Is.EqualTo(decoder.Length - (decoder.Length / 2)).Within(1));
        }

        [Test]
        public void AFileThatIsntAudio_Throws()
        {
            var path = Path.Combine(Dir, "broken.mp3");
            TestMp3.WriteBroken(path);

            Assert.Throws<IOException>(() => Open(path).Dispose());
        }

        [Test]
        public void AMissingFile_Throws()
        {
            Assert.Throws<IOException>(() => Open(Path.Combine(Dir, "gone.mp3")).Dispose());
        }

        protected static float[] ReadAll(IAudioDecoder decoder)
        {
            ArgumentNullException.ThrowIfNull(decoder);
            using var all = new MemoryStream();
            var buffer = new float[4096 * AudioFormat.Channels];
            int frames;
            do
            {
                frames = decoder.Read(buffer);
                all.Write(MemoryMarshal.AsBytes(buffer.AsSpan(0, frames * AudioFormat.Channels)));
            }
            while (frames > 0);
            var samples = new float[all.Length / sizeof(float)];
            all.Position = 0;
            all.ReadExactly(MemoryMarshal.AsBytes(samples.AsSpan()));
            return samples;
        }

        protected abstract IAudioDecoder Open(string path);

        // 16-bit stereo, 440 Hz on the left and 660 Hz on the right.
        private static void WriteSine(string path, int sampleRate, int frames)
        {
            using var writer = new BinaryWriter(File.Create(path));
            const short channels = 2;
            const short bytesPerSample = 2;
            var dataSize = frames * channels * bytesPerSample;
            writer.Write("RIFF"u8);
            writer.Write(36 + dataSize);
            writer.Write("WAVEfmt "u8);
            writer.Write(16);
            writer.Write((short)1);
            writer.Write(channels);
            writer.Write(sampleRate);
            writer.Write(sampleRate * channels * bytesPerSample);
            writer.Write((short)(channels * bytesPerSample));
            writer.Write((short)(8 * bytesPerSample));
            writer.Write("data"u8);
            writer.Write(dataSize);
            for (var i = 0; i < frames; i++)
            {
                writer.Write((short)(16000 * Math.Sin(2 * Math.PI * 440 * i / sampleRate)));
                writer.Write((short)(16000 * Math.Sin(2 * Math.PI * 660 * i / sampleRate)));
            }
        }
    }
}
