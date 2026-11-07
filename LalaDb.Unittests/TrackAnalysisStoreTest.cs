using System;
using System.IO;
using System.Linq;
using LalaDb.Data;
using LalaDb.Model;
using LibLala.Audio;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace LalaDb.Unittests
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "The teardown disposes them.")]
    public class TrackAnalysisStoreTest
    {
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

        private string AddTrack(string name, int beatsPerMinute = 0)
        {
            var path = Path.Combine(_dir, name);
            File.WriteAllBytes(path, [1, 2, 3]);
            AddTrackRow(path, name, beatsPerMinute);
            return path;
        }

        private void AddTrackRow(string path, string name, int beatsPerMinute = 0)
        {
            using var context = new LalaContext(_options);
            context.Add(new Track { Title = name, Path = path, BeatsPerMinute = beatsPerMinute, Album = new Album { Name = name } });
            context.SaveChanges();
        }

        private static readonly BeatGrid Grid = new(127.998, 0.312, 2.5, 400, 1.249, 0.312, 180.5);

        [Test]
        public void ATrack_IsToBeAnalyzed_WithItsTaggedTempo()
        {
            var path = AddTrack("a.mp3", beatsPerMinute: 128);

            var track = _store.TracksToAnalyze().Single();

            Assert.That(track.Path, Is.EqualTo(path));
            Assert.That(track.TaggedBeatsPerMinute, Is.EqualTo(128));
            Assert.That(track.Stamp, Is.EqualTo(FileStamp.Of(path)));
        }

        [Test]
        public void AnAnalyzedTrack_HasItsGrid_AndIsntToBeAnalyzedAgain()
        {
            var path = AddTrack("a.mp3");

            _store.Save(path, FileStamp.Of(path)!.Value, Grid);

            Assert.That(_store.FindGrid(path), Is.EqualTo(Grid));
            Assert.That(_store.TracksToAnalyze(), Is.Empty);
        }

        [Test]
        public void ATrackWithoutBeat_HasNoGrid_AndIsntToBeAnalyzedAgain()
        {
            var path = AddTrack("a.mp3");

            _store.Save(path, FileStamp.Of(path)!.Value, grid: null);

            Assert.That(_store.FindGrid(path), Is.Null);
            Assert.That(_store.TracksToAnalyze(), Is.Empty);
        }

        [Test]
        public void AChangedFile_IsToBeAnalyzedAgain_AndItsOldGridIsGone()
        {
            var path = AddTrack("a.mp3");
            _store.Save(path, FileStamp.Of(path)!.Value, Grid);

            File.WriteAllBytes(path, [1, 2, 3, 4]);

            Assert.That(_store.FindGrid(path), Is.Null);
            Assert.That(_store.TracksToAnalyze().Select(t => t.Path), Is.EqualTo(new[] { path }));
        }

        [Test]
        public void SavingAgain_ReplacesTheAnalysis()
        {
            var path = AddTrack("a.mp3");
            _store.Save(path, FileStamp.Of(path)!.Value, grid: null);

            _store.Save(path, FileStamp.Of(path)!.Value, Grid);

            Assert.That(_store.FindGrid(path), Is.EqualTo(Grid));
            using var context = new LalaContext(_options);
            Assert.That(context.Set<TrackAnalysis>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void AnAnalysis_OutlivesAScan()
        {
            var path = AddTrack("a.mp3");
            _store.Save(path, FileStamp.Of(path)!.Value, Grid);

            // A scan replaces all tracks.
            using (var context = new LalaContext(_options))
            {
                context.RemoveRange(context.Set<Track>());
                context.SaveChanges();
            }
            AddTrackRow(path, "a.mp3");

            Assert.That(_store.TracksToAnalyze(), Is.Empty);
            Assert.That(_store.FindGrid(path), Is.EqualTo(Grid));
        }

        [Test]
        public void AMissingFile_IsntToBeAnalyzed()
        {
            var path = AddTrack("a.mp3");
            File.Delete(path);

            Assert.That(_store.TracksToAnalyze(), Is.Empty);
        }
    }
}
