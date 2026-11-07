using System;
using System.IO;
using System.Runtime.Versioning;
using LibLala.Audio;
using LibLala.Audio.MacOS;
using NUnit.Framework;
using TestHelpers;

namespace LibLala.Unittests.Audio
{
    [SupportedOSPlatform("macos")]
    public sealed class ExtAudioFileDecoderTest
    {
        private string _dir = "";

        [SetUp]
        public void SetUp()
        {
            if (!OperatingSystem.IsMacOS())
            {
                Assert.Ignore("Decodes with macOS's codecs.");
            }
            _dir = Path.Combine(Path.GetTempPath(), $"lalatest-{Guid.NewGuid()}");
            Directory.CreateDirectory(_dir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_dir))
            {
                Directory.Delete(_dir, recursive: true);
            }
        }

        [Test]
        public void Mp3_IsReadToItsLength()
        {
            var path = Path.Combine(_dir, "silence.mp3");
            TestMp3.Write(path);

            using var decoder = new ExtAudioFileDecoder(path);

            // 120 frames of 1152 samples, less the decoder's delay.
            Assert.That(decoder.Length, Is.InRange(130000, 138240));
            Assert.That(ReadAll(decoder).Length / AudioFormat.Channels, Is.EqualTo(decoder.Length));
        }

        [Test]
        public void AnotherSampleRate_IsConvertedTo44100()
        {
            var path = Path.Combine(_dir, "a.wav");
            WriteSine(path, sampleRate: 48000, frames: 96000);

            using var decoder = new ExtAudioFileDecoder(path);

            Assert.That(decoder.Length, Is.EqualTo(88200));
            Assert.That(ReadAll(decoder).Length / AudioFormat.Channels, Is.EqualTo(88200).Within(1));
        }

        [Test]
        public void Seek_LandsOnTheFrame()
        {
            var path = Path.Combine(_dir, "a.wav");
            WriteSine(path, sampleRate: 48000, frames: 96000);
            using var decoder = new ExtAudioFileDecoder(path);
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
            var path = Path.Combine(_dir, "silence.mp3");
            TestMp3.Write(path);
            using var decoder = new ExtAudioFileDecoder(path);

            decoder.Seek(decoder.Length / 2);

            Assert.That(ReadAll(decoder).Length / AudioFormat.Channels, Is.EqualTo(decoder.Length - (decoder.Length / 2)).Within(1));
        }

        [Test]
        public void AFileThatIsntAudio_Throws()
        {
            var path = Path.Combine(_dir, "broken.mp3");
            TestMp3.WriteBroken(path);

            Assert.Throws<IOException>(() => new ExtAudioFileDecoder(path).Dispose());
        }

        [Test]
        public void AMissingFile_Throws()
        {
            Assert.Throws<IOException>(() => new ExtAudioFileDecoder(Path.Combine(_dir, "gone.mp3")).Dispose());
        }

        private static float[] ReadAll(ExtAudioFileDecoder decoder)
        {
            using var all = new MemoryStream();
            var buffer = new float[4096 * AudioFormat.Channels];
            int frames;
            do
            {
                frames = decoder.Read(buffer);
                all.Write(System.Runtime.InteropServices.MemoryMarshal.AsBytes(buffer.AsSpan(0, frames * AudioFormat.Channels)));
            }
            while (frames > 0);
            var samples = new float[all.Length / sizeof(float)];
            all.Position = 0;
            all.ReadExactly(System.Runtime.InteropServices.MemoryMarshal.AsBytes(samples.AsSpan()));
            return samples;
        }

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
