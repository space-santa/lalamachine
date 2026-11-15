using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using LalaDb.Data;
using LalaDb.Model;
using LibLala.Audio;
using LibLala.Audio.Analysis;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Lalamachine.ViewModel.Unittests
{
    public class TrackAnalysisViewModelTest
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);
        private DbContextOptions<LalaContext> _options = null!;
        private TrackAnalysisStore _store = null!;
        private string _dir = "";

        [SetUp]
        public void SetUp()
        {
            _dir = Path.Combine(Path.GetTempPath(), $"lalatest-{Guid.NewGuid()}");
            Directory.CreateDirectory(_dir);
            // A file, like the app's, and a connection for each context: the analysis reads and writes from threads of its
            // own, one connection isn't for more than one thread at a time.
            _options = new DbContextOptionsBuilder<LalaContext>().UseSqlite($"Data Source={Path.Combine(_dir, "musiclib.db")}").Options;
            using (var context = new LalaContext(_options))
            {
                context.Database.Migrate();
            }
            _store = new TrackAnalysisStore(() => new LalaContext(_options));
        }

        [TearDown]
        public void TearDown()
        {
            // Pooled connections keep the file open.
            SqliteConnection.ClearAllPools();
            Directory.Delete(_dir, recursive: true);
        }

        private string AddTrack(string name, string initialKey = "", int beatsPerMinute = 0)
        {
            var path = Path.Combine(_dir, name);
            File.WriteAllBytes(path, [1]);
            using var context = new LalaContext(_options);
            context.Add(new Track { Title = name, Path = path, InitialKey = initialKey, BeatsPerMinute = beatsPerMinute, Album = new Album { Name = name } });
            context.SaveChanges();
            return path;
        }

        private TrackAnalysis Analysis(string path)
        {
            using var context = new LalaContext(_options);
            return context.Set<TrackAnalysis>().Single(a => a.Path == path);
        }

        // Runs the analysis to its end, with what it raises on this thread like on the UI's.
        private List<TrackAnalyzedEventArgs> Analyze(Func<string, IAudioDecoder> openDecoder)
        {
            var found = new List<TrackAnalyzedEventArgs>();
            var previous = SynchronizationContext.Current;
            var posted = new PostedActions();
            SynchronizationContext.SetSynchronizationContext(posted);
            try
            {
                using var analysis = new TrackAnalysisViewModel(_store, openDecoder, threadCount: 1);
                analysis.TrackAnalyzed += (o, e) => found.Add(e);
                analysis.Start();
                Assert.That(SpinWait.SpinUntil(() => { posted.Run(); return !analysis.IsAnalyzing; }, Timeout), Is.True);
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previous);
            }
            return found;
        }

        [Test]
        public void ATrackWithoutKeyAndTempoInItsTags_GetsBothFound_AndSaysSo()
        {
            var path = AddTrack("a.mp3");

            var found = Analyze(path => new ChordDecoder());

            Assert.That(Analysis(path).Key, Is.EqualTo("8B"));
            Assert.That(found.Select(e => (e.Path, e.InitialKey, e.BeatsPerMinute)), Is.EqualTo(new[] { (path, (string?)"8B", (uint?)0) }));
            Assert.That(_store.TracksToAnalyze(), Is.Empty);
        }

        [Test]
        public void ATrackWithKeyAndTempoInItsTags_IsOnlyAnalyzedForItsBeats_AndNothingIsToShow()
        {
            var path = AddTrack("a.mp3", initialKey: "5A", beatsPerMinute: 128);

            var found = Analyze(path => new ChordDecoder());

            Assert.That(found, Is.Empty);
            Assert.That(Analysis(path).Key, Is.Empty);
            Assert.That(Analysis(path).KeyAnalyzerVersion, Is.Zero);
            Assert.That(_store.TracksToAnalyze(), Is.Empty);
        }

        [Test]
        public void ATrackWhoseBeatsAreFound_IsOnlyReadForItsKey_WhenItLosesTheOneInItsTags()
        {
            var path = AddTrack("a.mp3", initialKey: "5A", beatsPerMinute: 128);
            Analyze(path => new ChordDecoder());
            using (var context = new LalaContext(_options))
            {
                context.Set<Track>().Single().InitialKey = "";
                context.SaveChanges();
            }

            var found = Analyze(path => new ChordDecoder());

            Assert.That(found.Select(e => (e.InitialKey, e.BeatsPerMinute)), Is.EqualTo(new[] { ((string?)"8B", (uint?)null) }));
            Assert.That(Analysis(path).Key, Is.EqualTo("8B"));
        }

        // Version 4 only moves the beats where the cues say: a track without cues keeps what version 3 found in it, and
        // what version 2 found with a tagged tempo, without being decoded for it again. The silence would have no beat.
        [TestCase(3, 0, true)]
        [TestCase(2, 128, true)]
        [TestCase(2, 0, false)]
        public void ATrackWhoseBeatsTheCuesDontMove_KeepsWhatAVersionBeforeFound(int version, int taggedBeatsPerMinute, bool isKept)
        {
            var path = AddTrack("a.mp3", initialKey: "5A", beatsPerMinute: taggedBeatsPerMinute);
            var grid = new BeatGrid(127.998, 0.312, 2.5, 400, 1.249, 0.312, 180.5);
            _store.Save(_store.TracksToAnalyze().Single(), grid, key: null);
            using (var context = new LalaContext(_options))
            {
                context.Set<TrackAnalysis>().Single().AnalyzerVersion = version;
                context.SaveChanges();
            }

            var found = Analyze(path => new SilentDecoder());

            Assert.That(_store.FindGrid(path), Is.EqualTo(isKept ? grid : null));
            Assert.That(Analysis(path).AnalyzerVersion, Is.EqualTo(BeatGridAnalyzer.Version));
            Assert.That(found.Select(e => e.BeatsPerMinute), Is.EqualTo(taggedBeatsPerMinute > 0 ? Array.Empty<uint?>() : new uint?[] { isKept ? 128u : 0u }));
            Assert.That(_store.TracksToAnalyze(), Is.Empty);
        }

        [Test]
        public void ATrackWithoutSound_HasNoKey_AndSaysSo()
        {
            var path = AddTrack("a.mp3");

            var found = Analyze(path => new SilentDecoder());

            Assert.That(found.Select(e => (e.InitialKey, e.BeatsPerMinute)), Is.EqualTo(new[] { ((string?)"", (uint?)0) }));
            Assert.That(Analysis(path).KeyAnalyzerVersion, Is.EqualTo(KeyAnalyzer.Version));
        }

        [Test]
        public void Start_AnalyzesTheTracks_AndIsDoneThen()
        {
            AddTrack("a.mp3");
            AddTrack("b.mp3");
            using var analysis = new TrackAnalysisViewModel(_store, path => new SilentDecoder(), threadCount: 2);

            analysis.Start();

            Assert.That(SpinWait.SpinUntil(() => !analysis.IsAnalyzing, Timeout), Is.True);
            Assert.That(_store.TracksToAnalyze(), Is.Empty);
        }

        [Test]
        public void ATrackThatCantBeRead_IsLeftForNextTime()
        {
            var broken = AddTrack("broken.mp3");
            AddTrack("fine.mp3");
            using var analysis = new TrackAnalysisViewModel(_store, path => path == broken ? throw new IOException("Not audio.") : new SilentDecoder(), threadCount: 1);

            analysis.Start();

            Assert.That(SpinWait.SpinUntil(() => !analysis.IsAnalyzing, Timeout), Is.True);
            Assert.That(_store.TracksToAnalyze().Select(t => t.Path), Is.EqualTo(new[] { broken }));
        }

        [Test]
        public void AScan_StopsTheAnalysis_AndItsEndStartsItAgain()
        {
            AddTrack("a.mp3");
            using var analysis = new TrackAnalysisViewModel(_store, path => new SilentDecoder(), threadCount: 1);
            analysis.Start();

            analysis.ScanningChangedHandler(this, new ScanningChangedEventArgs(scanning: true));
            Assert.That(analysis.IsAnalyzing, Is.False);

            analysis.ScanningChangedHandler(this, new ScanningChangedEventArgs(scanning: false));
            Assert.That(analysis.IsAnalyzing, Is.True);
            Assert.That(SpinWait.SpinUntil(() => !analysis.IsAnalyzing, Timeout), Is.True);
            Assert.That(_store.TracksToAnalyze(), Is.Empty);
        }

        // Twenty seconds of a C major chord over its root, without a beat.
        private sealed class ChordDecoder : IAudioDecoder
        {
            private static readonly double[] Frequencies = [65.41, 130.81, 261.63, 329.63, 392.00, 523.25];
            private long _position;

            public long Length => 20 * AudioFormat.SampleRate;

            public int Read(Span<float> buffer)
            {
                var frames = (int)Math.Min(buffer.Length / AudioFormat.Channels, Length - _position);
                for (var i = 0; i < frames; i++)
                {
                    var seconds = (_position + i) / (double)AudioFormat.SampleRate;
                    var sample = (float)Frequencies.Sum(frequency => 0.1 * Math.Sin(2 * Math.PI * frequency * seconds));
                    buffer[i * AudioFormat.Channels] = sample;
                    buffer[(i * AudioFormat.Channels) + 1] = sample;
                }
                _position += frames;
                return frames;
            }

            public void Seek(long frame) => _position = frame;

            public void Dispose() { }
        }

        // Five seconds of nothing, which has no beat.
        private sealed class SilentDecoder : IAudioDecoder
        {
            private long _position;

            public long Length => 5 * AudioFormat.SampleRate;

            public int Read(Span<float> buffer)
            {
                var frames = (int)Math.Min(buffer.Length / AudioFormat.Channels, Length - _position);
                buffer[..(frames * AudioFormat.Channels)].Clear();
                _position += frames;
                return frames;
            }

            public void Seek(long frame) => _position = frame;

            public void Dispose() { }
        }
    }
}
