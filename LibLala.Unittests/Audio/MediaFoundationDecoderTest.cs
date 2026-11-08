using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using LibLala.Audio;
using LibLala.Audio.Windows;
using NUnit.Framework;
using TestHelpers;

namespace LibLala.Unittests.Audio
{
    [SupportedOSPlatform("windows")]
    public sealed class MediaFoundationDecoderTest : AudioDecoderContractTest
    {
        protected override string? Unsupported =>
            !OperatingSystem.IsWindows() ? "Decodes with Windows's codecs."
            : !HasLibrary("mfplat.dll") || !HasLibrary("mp3dmod.dll") ? "This Windows has no Media Foundation, like a Windows Server without it."
            : null;

        [Test]
        public void Mp3WithTags_IsReadLikeWithout()
        {
            var plain = Path.Combine(Dir, "plain.mp3");
            var tagged = Path.Combine(Dir, "tagged.mp3");
            TestMp3.Write(plain);
            TestMp3.Write(tagged, title: "A title", artist: "An artist");
            using var plainDecoder = Open(plain);

            using var decoder = Open(tagged);

            Assert.That(decoder.Length, Is.EqualTo(plainDecoder.Length));
            Assert.That(ReadAll(decoder).Length / AudioFormat.Channels, Is.EqualTo(decoder.Length));
        }

        [Test]
        public void Mp3WithSomethingBetweenItsFrames_IsReadPastIt()
        {
            var plain = Path.Combine(Dir, "plain.mp3");
            var broken = Path.Combine(Dir, "broken.mp3");
            TestMp3.Write(plain);
            var bytes = File.ReadAllBytes(plain);
            // After 60 of its frames of 417 bytes.
            File.WriteAllBytes(broken, [.. bytes.Take(60 * 417), .. new byte[100], .. bytes.Skip(60 * 417)]);
            using var plainDecoder = Open(plain);

            using var decoder = Open(broken);

            Assert.That(decoder.Length, Is.EqualTo(plainDecoder.Length));
            Assert.That(ReadAll(decoder).Length / AudioFormat.Channels, Is.EqualTo(decoder.Length));
        }

        // Media Foundation's own mp3 source estimates where to go in a VBR file and lands tens of ms off. The second file has
        // no VBR header, its length comes from its frames.
        [TestCase("vbr-44100.mp3")]
        [TestCase("vbr-48000-mono.mp3")]
        public void Seek_InAVbrMp3_LandsWhereReadingOnDoes(string file)
        {
            using var decoder = Open(Fixture(file));
            var all = ReadAll(decoder);
            // 1.5 s, and the encoder's delay and padding, less than two frames.
            Assert.That(all.Length / AudioFormat.Channels, Is.EqualTo(decoder.Length).And.InRange(66150, 66150 + 2304));
            Assert.That(all.Max(Math.Abs), Is.GreaterThan(0.1), "There's nothing to hear, the test would show nothing.");

            foreach (var frame in new long[] { 0, 1, 1151, 4410, 22050, 33333, decoder.Length - 1000 })
            {
                decoder.Seek(frame);
                var buffer = new float[1000 * AudioFormat.Channels];
                var read = decoder.Read(buffer);

                Assert.That(read, Is.EqualTo(1000));
                Assert.That(buffer, Is.EqualTo(all.AsSpan((int)frame * AudioFormat.Channels, buffer.Length).ToArray()).Within(1e-6), $"After a seek to {frame}");
            }
        }

        [Test]
        public void MonoMp3_IsOnBothSides()
        {
            using var decoder = Open(Fixture("vbr-48000-mono.mp3"));

            var all = ReadAll(decoder);

            for (var i = 0; i < all.Length; i += AudioFormat.Channels)
            {
                Assert.That(all[i + 1], Is.EqualTo(all[i]));
            }
        }

        // Where the position slider is all the way to the right.
        [Test]
        public void Seek_ToTheEndOfAnMp3_LeavesNothingToRead()
        {
            using var decoder = Open(Fixture("vbr-44100.mp3"));

            decoder.Seek(decoder.Length);

            Assert.That(decoder.Read(new float[1000 * AudioFormat.Channels]), Is.Zero);
        }

        [Test]
        public void Seek_ToTheEndOfAnythingElse_LeavesNothingToRead()
        {
            using var decoder = Open(WriteWav("a.wav"));

            decoder.Seek(decoder.Length);

            Assert.That(decoder.Read(new float[1000 * AudioFormat.Channels]), Is.Zero);
        }

        protected override IAudioDecoder Open(string path) => new MediaFoundationDecoder(path);

        private static bool HasLibrary(string name)
        {
            if (!NativeLibrary.TryLoad(name, typeof(MediaFoundationDecoderTest).Assembly, DllImportSearchPath.System32, out var library))
            {
                return false;
            }
            NativeLibrary.Free(library);
            return true;
        }

        private static string Fixture(string file) => Path.Combine(TestContext.CurrentContext.TestDirectory, "Audio", "Files", file);

        // A second of silence at 44.1 kHz.
        private string WriteWav(string file)
        {
            var path = Path.Combine(Dir, file);
            using var writer = new BinaryWriter(File.Create(path));
            const int dataSize = 44100 * 4;
            writer.Write("RIFF"u8);
            writer.Write(36 + dataSize);
            writer.Write("WAVEfmt "u8);
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)2);
            writer.Write(44100);
            writer.Write(44100 * 4);
            writer.Write((short)4);
            writer.Write((short)16);
            writer.Write("data"u8);
            writer.Write(dataSize);
            writer.Write(new byte[dataSize]);
            return path;
        }
    }
}
