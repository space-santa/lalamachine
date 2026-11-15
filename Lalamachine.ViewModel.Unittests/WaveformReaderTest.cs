using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LibLala.Audio;
using LibLala.Audio.Analysis;
using NUnit.Framework;

namespace Lalamachine.ViewModel.Unittests
{
    public class WaveformReaderTest
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

        [Test]
        public void ATrackReadBefore_IsntReadAgain()
        {
            var opened = new ConcurrentQueue<string>();
            using var reader = new WaveformReader(path =>
            {
                opened.Enqueue(path);
                return new ToneDecoder(seconds: 1);
            });

            var first = reader.Read("a");
            Assert.That(ResultOf(first), Is.Not.Null);
            var again = reader.Read("a");

            Assert.That(again, Is.SameAs(first));
            Assert.That(opened, Is.EqualTo(new[] { "a" }));
        }

        [Test]
        public void ATrackThatCouldntBeRead_IsReadAgain_TheDriveMayBeBack()
        {
            var isConnected = false;
            using var reader = new WaveformReader(path => isConnected ? new ToneDecoder(seconds: 1) : throw new IOException("The drive is gone."));

            Assert.That(ResultOf(reader.Read("a")), Is.Null);
            isConnected = true;

            Assert.That(ResultOf(reader.Read("a")), Is.Not.Null);
        }

        [Test]
        public void AReadThatDropsOut_IsStopped()
        {
            using var gate = new ManualResetEventSlim();
            using var reader = new WaveformReader(path => new ToneDecoder(seconds: 1) { Gate = path == "a" ? gate : null });
            var a = reader.Read("a");

            foreach (var path in new[] { "b", "c", "d", "e" })
            {
                _ = reader.Read(path);
            }
            gate.Set();

            Assert.That(ResultOf(a), Is.Null);
            Assert.That(reader.Read("a"), Is.Not.SameAs(a));
        }

        [Test]
        public void Disposed_ItStopsTheReadsGoingOn()
        {
            using var gate = new ManualResetEventSlim();
            var reader = new WaveformReader(path => new ToneDecoder(seconds: 1) { Gate = gate });
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
