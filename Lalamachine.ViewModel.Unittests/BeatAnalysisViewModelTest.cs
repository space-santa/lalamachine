using System;
using System.IO;
using System.Linq;
using System.Threading;
using LalaDb.Data;
using LalaDb.Model;
using LibLala.Audio;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Lalamachine.ViewModel.Unittests
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "The teardown disposes them.")]
    public class BeatAnalysisViewModelTest
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);
        private SqliteConnection _connection = null!;
        private DbContextOptions<LalaContext> _options = null!;
        private TrackAnalysisStore _store = null!;
        private string _dir = "";

        [SetUp]
        public void SetUp()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();
            _options = new DbContextOptionsBuilder<LalaContext>().UseSqlite(_connection).Options;
            using (var context = new LalaContext(_options))
            {
                context.Database.Migrate();
            }
            _store = new TrackAnalysisStore(() => new LalaContext(_options));
            _dir = Path.Combine(Path.GetTempPath(), $"lalatest-{Guid.NewGuid()}");
            Directory.CreateDirectory(_dir);
        }

        [TearDown]
        public void TearDown()
        {
            _connection.Dispose();
            Directory.Delete(_dir, recursive: true);
        }

        private string AddTrack(string name)
        {
            var path = Path.Combine(_dir, name);
            File.WriteAllBytes(path, [1]);
            using var context = new LalaContext(_options);
            context.Add(new Track { Title = name, Path = path, Album = new Album { Name = name } });
            context.SaveChanges();
            return path;
        }

        [Test]
        public void Start_AnalyzesTheTracks_AndIsDoneThen()
        {
            AddTrack("a.mp3");
            AddTrack("b.mp3");
            using var analysis = new BeatAnalysisViewModel(_store, path => new SilentDecoder(), threadCount: 2);

            analysis.Start();

            Assert.That(SpinWait.SpinUntil(() => !analysis.IsAnalyzing, Timeout), Is.True);
            Assert.That(_store.TracksToAnalyze(), Is.Empty);
        }

        [Test]
        public void ATrackThatCantBeRead_IsLeftForNextTime()
        {
            var broken = AddTrack("broken.mp3");
            AddTrack("fine.mp3");
            using var analysis = new BeatAnalysisViewModel(_store, path => path == broken ? throw new IOException("Not audio.") : new SilentDecoder(), threadCount: 1);

            analysis.Start();

            Assert.That(SpinWait.SpinUntil(() => !analysis.IsAnalyzing, Timeout), Is.True);
            Assert.That(_store.TracksToAnalyze().Select(t => t.Path), Is.EqualTo(new[] { broken }));
        }

        [Test]
        public void AScan_StopsTheAnalysis_AndItsEndStartsItAgain()
        {
            AddTrack("a.mp3");
            using var analysis = new BeatAnalysisViewModel(_store, path => new SilentDecoder(), threadCount: 1);
            analysis.Start();

            analysis.ScanningChangedHandler(this, new ScanningChangedEventArgs(scanning: true));
            Assert.That(analysis.IsAnalyzing, Is.False);

            analysis.ScanningChangedHandler(this, new ScanningChangedEventArgs(scanning: false));
            Assert.That(analysis.IsAnalyzing, Is.True);
            Assert.That(SpinWait.SpinUntil(() => !analysis.IsAnalyzing, Timeout), Is.True);
            Assert.That(_store.TracksToAnalyze(), Is.Empty);
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
