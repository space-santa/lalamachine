using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LalaDb.Model;
using LibLala.Audio;
using LibLala.Audio.Analysis;
using NUnit.Framework;

namespace Lalamachine.ViewModel.Unittests
{
    public class WaveformReaderTest
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);
        // What the store has, as the reader stored it.
        private ConcurrentDictionary<string, (FileStamp Stamp, Waveform Waveform)> _stored = null!;
        private ConcurrentQueue<string> _opened = null!;

        [SetUp]
        public void SetUp()
        {
            _stored = new();
            _opened = new();
        }

        private WaveformReader Reader(Func<string, IAudioDecoder> openDecoder) => new(
            path =>
            {
                _opened.Enqueue(path);
                return openDecoder(path);
            },
            path => _stored.TryGetValue(path, out var stored) ? stored.Waveform : null,
            (path, stamp, waveform) => _stored[path] = (stamp, waveform));

        [Test]
        public void ATrackAskedForAgain_WhileItsRead_IsntReadTwice()
        {
            using var gate = new ManualResetEventSlim();
            using var reader = Reader(path => new ToneDecoder(seconds: 1) { Gate = gate });

            var first = reader.Read("a");
            var again = reader.Read("a");
            gate.Set();

            Assert.That(again, Is.SameAs(first));
            Assert.That(ResultOf(first), Is.Not.Null);
            Assert.That(_opened, Is.EqualTo(new[] { "a" }));
        }

        [Test]
        public void AStoredWaveform_IsntReadFromTheFile()
        {
            var stored = Waveform.FromBytes([255, 128]);
            _stored["a"] = (new FileStamp(1, 1), stored);
            using var reader = Reader(path => new ToneDecoder(seconds: 1));

            Assert.That(ResultOf(reader.Read("a")), Is.SameAs(stored));
            Assert.That(_opened, Is.Empty);
        }

        [Test]
        public void AWaveformReadFromTheFile_IsStored_AsTheFileWasBeforeItWasRead()
        {
            var path = Path.GetTempFileName();
            try
            {
                var before = FileStamp.Of(path);
                using var reader = Reader(file =>
                {
                    File.WriteAllBytes(file, [1, 2, 3]);
                    return new ToneDecoder(seconds: 1);
                });

                var waveform = ResultOf(reader.Read(path));

                Assert.That(_stored[path].Stamp, Is.EqualTo(before));
                Assert.That(_stored[path].Waveform, Is.SameAs(waveform).And.Not.Null);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Test]
        public void AWaveformThatCantBeStored_IsShownAllTheSame()
        {
            var path = Path.GetTempFileName();
            try
            {
                using var reader = new WaveformReader(file => new ToneDecoder(seconds: 1), file => null, (file, stamp, waveform) => throw new IOException("The database is busy."));

                Assert.That(ResultOf(reader.Read(path)), Is.Not.Null);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Test]
        public void ATrackThatCouldntBeRead_IsReadAgain_TheDriveMayBeBack()
        {
            var isConnected = false;
            using var reader = Reader(path => isConnected ? new ToneDecoder(seconds: 1) : throw new IOException("The drive is gone."));

            Assert.That(ResultOf(reader.Read("a")), Is.Null);
            isConnected = true;

            Assert.That(ResultOf(reader.Read("a")), Is.Not.Null);
        }

        [Test]
        public void TheReadOfATrackThatNeitherPlaysNorIsMixedIn_IsStopped()
        {
            using var gate = new ManualResetEventSlim();
            using var reader = Reader(path => new ToneDecoder(seconds: 1) { Gate = path == "a" ? gate : null });
            var a = reader.Read("a");

            // Next, and next again.
            _ = reader.Read("b");
            _ = reader.Read("c");
            gate.Set();

            Assert.That(ResultOf(a), Is.Null);
        }

        [Test]
        public void Disposed_ItStopsTheReadsGoingOn()
        {
            using var gate = new ManualResetEventSlim();
            var reader = Reader(path => new ToneDecoder(seconds: 1) { Gate = gate });
            var a = reader.Read("a");

            reader.Dispose();
            gate.Set();

            Assert.That(ResultOf(a), Is.Null);
        }

        private static Waveform? ResultOf(Task<Waveform?> read)
        {
            Assert.That(read.Wait(Timeout), Is.True);
            return read.Result;
        }
    }

    // A tone at half of full scale. With a gate, its first read waits until the gate is open.
    internal sealed class ToneDecoder : IAudioDecoder
    {
        private long _position;

        public ToneDecoder(double seconds)
        {
            Length = (long)(seconds * AudioFormat.SampleRate);
        }

        public long Length { get; }
        public ManualResetEventSlim? Gate { get; init; }

        public int Read(Span<float> buffer)
        {
            Gate?.Wait();
            var frames = (int)Math.Min(buffer.Length / AudioFormat.Channels, Length - _position);
            buffer[..(frames * AudioFormat.Channels)].Fill(0.5f);
            _position += frames;
            return frames;
        }

        public void Seek(long frame) => _position = frame;

        public void Dispose() { }
    }
}
