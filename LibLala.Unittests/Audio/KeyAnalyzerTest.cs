using System;
using System.Collections.Generic;
using System.Linq;
using LibLala.Audio;
using LibLala.Audio.Analysis;
using NUnit.Framework;

namespace LibLala.Unittests.Audio
{
    public sealed class KeyAnalyzerTest
    {
        private static IEnumerable<TestCaseData> AllKeys()
        {
            foreach (var isMinor in new[] { false, true })
            {
                for (var tonic = 0; tonic < 12; tonic++)
                {
                    var key = new MusicalKey(tonic, isMinor);
                    yield return new TestCaseData(key).SetArgDisplayNames(key.Camelot);
                }
            }
        }

        [TestCaseSource(nameof(AllKeys))]
        public void ChordsInAKey_AreHeardInThatKey(MusicalKey key)
        {
            using var decoder = new ChordsDecoder(key);

            Assert.That(KeyAnalyzer.Analyze(decoder), Is.EqualTo(key));
        }

        // A record that ran a little fast or slow, or a band that tuned to its piano.
        [TestCase(-40)]
        [TestCase(-20)]
        [TestCase(25)]
        [TestCase(40)]
        public void ATrackThatIsntTunedTo440Hz_IsHeardInItsKeyAllTheSame(double cents)
        {
            var key = new MusicalKey(9, isMinor: true);
            using var decoder = new ChordsDecoder(key, tuningCents: cents);

            Assert.That(KeyAnalyzer.Analyze(decoder), Is.EqualTo(key));
        }

        [Test]
        public void DrumsOverTheChords_DontChangeTheKey()
        {
            var key = new MusicalKey(5, isMinor: true);
            using var chords = new ChordsDecoder(key);
            using var drums = new DrumDecoder(128, 0.25, seconds: 24);
            using var decoder = new MixDecoder(chords, drums);

            Assert.That(KeyAnalyzer.Analyze(decoder), Is.EqualTo(key));
        }

        [Test]
        public void Silence_HasNoKey()
        {
            using var decoder = new ChordsDecoder(new MusicalKey(0, isMinor: false), seconds: 30) { Volume = 0 };

            Assert.That(KeyAnalyzer.Analyze(decoder), Is.Null);
        }

        [Test]
        public void ASoundOfASecond_IsTooShortToTellAKey()
        {
            using var decoder = new ChordsDecoder(new MusicalKey(0, isMinor: false), seconds: 1);

            Assert.That(KeyAnalyzer.Analyze(decoder), Is.Null);
        }

        [Test]
        public void TheKey_IsTheSame_InWhateverPiecesTheTrackComes()
        {
            var key = new MusicalKey(2, isMinor: false);
            using var whole = new ChordsDecoder(key);
            using var pieces = new ChordsDecoder(key);
            var analyzer = new KeyAnalyzer();
            var sizes = new[] { 2, 4096, 14, 100000, 882, 31072 };
            var buffer = new float[sizes.Max()];
            for (var i = 0; ; i++)
            {
                var read = pieces.Read(buffer.AsSpan(0, sizes[i % sizes.Length]));
                if (read == 0)
                {
                    break;
                }
                analyzer.Add(buffer.AsSpan(0, read * AudioFormat.Channels));
            }

            Assert.That(analyzer.Key(), Is.EqualTo(key));
            Assert.That(analyzer.Key(), Is.EqualTo(KeyAnalyzer.Analyze(whole)));
        }

        [Test]
        public void TheBeatAnalysis_LetsTheKeyAnalyzerHearTheTrack()
        {
            var key = new MusicalKey(7, isMinor: true);
            using var chords = new ChordsDecoder(key, seconds: 60);
            using var drums = new DrumDecoder(128, 0.25, seconds: 60);
            using var decoder = new MixDecoder(chords, drums);
            var analyzer = new KeyAnalyzer();

            var grid = BeatGridAnalyzer.Analyze(decoder, taggedBpm: null, cueSeconds: [], analyzer);

            Assert.That(grid!.BeatsPerMinute, Is.EqualTo(128).Within(0.01));
            Assert.That(analyzer.Key(), Is.EqualTo(key));
        }

        // Two tracks at once.
        private sealed class MixDecoder : IAudioDecoder
        {
            private readonly IAudioDecoder _first;
            private readonly IAudioDecoder _second;

            public MixDecoder(IAudioDecoder first, IAudioDecoder second)
            {
                _first = first;
                _second = second;
            }

            public long Length => Math.Min(_first.Length, _second.Length);

            public int Read(Span<float> buffer)
            {
                var other = new float[buffer.Length];
                var frames = Math.Min(_first.Read(buffer), _second.Read(other));
                for (var i = 0; i < frames * AudioFormat.Channels; i++)
                {
                    buffer[i] += other[i];
                }
                return frames;
            }

            public void Seek(long frame)
            {
                _first.Seek(frame);
                _second.Seek(frame);
            }

            public void Dispose() { }
        }
    }
}
