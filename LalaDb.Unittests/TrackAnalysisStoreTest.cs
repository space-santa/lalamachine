using System;
using System.IO;
using System.Linq;
using LalaDb.Data;
using LalaDb.Model;
using LibLala.Audio;
using LibLala.Audio.Analysis;
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

        // With a key in its tags, unless told otherwise: most tests are about the beats.
        private string AddTrack(string name, int beatsPerMinute = 0, string initialKey = "8A")
        {
            var path = Path.Combine(_dir, name);
            File.WriteAllBytes(path, [1, 2, 3]);
            AddTrackRow(path, name, beatsPerMinute, initialKey);
            return path;
        }

        private void AddTrackRow(string path, string name, int beatsPerMinute = 0, string initialKey = "8A")
        {
            using var context = new LalaContext(_options);
            context.Add(new Track { Title = name, Path = path, BeatsPerMinute = beatsPerMinute, InitialKey = initialKey, Album = new Album { Name = name } });
            context.SaveChanges();
        }

        // Saves what the store says is to be looked for in the one track there is.
        private void SaveAnalysis(BeatGrid? grid, MusicalKey? key = null)
        {
            _store.Save(_store.TracksToAnalyze().Single(), grid, key, Levels);
        }

        private TrackAnalysis Analysis(string path)
        {
            using var context = new LalaContext(_options);
            return context.Set<TrackAnalysis>().Single(a => a.Path == path);
        }

        private static readonly BeatGrid Grid = new(127.998, 0.312, 2.5, 400, 1.249, 0.312, 180.5);
        private static readonly MusicalKey AMinor = new(9, isMinor: true);
        private static readonly Waveform Levels = Waveform.FromBytes([255, 128, 0, 64]);

        [Test]
        public void ATrack_IsToBeAnalyzed_WithItsTaggedTempo()
        {
            var path = AddTrack("a.mp3", beatsPerMinute: 128);

            var track = _store.TracksToAnalyze().Single();

            Assert.That(track.Path, Is.EqualTo(path));
            Assert.That(track.TaggedBeatsPerMinute, Is.EqualTo(128));
            Assert.That(track.Stamp, Is.EqualTo(FileStamp.Of(path)));
            Assert.That((track.NeedsGrid, track.NeedsKey, track.NeedsWaveform), Is.EqualTo((true, false, true)));
        }

        [Test]
        public void AnAnalyzedTrack_HasItsGrid_AndIsntToBeAnalyzedAgain()
        {
            var path = AddTrack("a.mp3");

            SaveAnalysis(Grid);

            Assert.That(_store.FindGrid(path), Is.EqualTo(Grid));
            Assert.That(_store.TracksToAnalyze(), Is.Empty);
        }

        [Test]
        public void ATrackWithoutBeat_HasNoGrid_AndIsntToBeAnalyzedAgain()
        {
            var path = AddTrack("a.mp3");

            SaveAnalysis(grid: null);

            Assert.That(_store.FindGrid(path), Is.Null);
            Assert.That(_store.TracksToAnalyze(), Is.Empty);
        }

        [Test]
        public void AChangedFile_IsToBeAnalyzedAgain_AndItsOldGridIsGone()
        {
            var path = AddTrack("a.mp3");
            SaveAnalysis(Grid);

            File.WriteAllBytes(path, [1, 2, 3, 4]);

            Assert.That(_store.FindGrid(path), Is.Null);
            Assert.That(_store.TracksToAnalyze().Select(t => t.Path), Is.EqualTo(new[] { path }));
        }

        [Test]
        public void SavingAgain_ReplacesTheAnalysis()
        {
            var path = AddTrack("a.mp3");
            var track = _store.TracksToAnalyze().Single();
            _store.Save(track, grid: null, key: null, Levels);

            _store.Save(track, Grid, key: null, Levels);

            Assert.That(_store.FindGrid(path), Is.EqualTo(Grid));
            using var context = new LalaContext(_options);
            Assert.That(context.Set<TrackAnalysis>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void AGridWithAnIntro_IsFoundWithIt_AndWithoutItWhenFoundAgainWithout()
        {
            var path = AddTrack("a.mp3");
            var track = _store.TracksToAnalyze().Single();
            var withIntro = new BeatGrid(127.998, 0.312, 2.5, 400, 1.249, 70.6, 180.5) { Intro = new BeatGrid(128.01, 0.2, 1.5, 120, 0.2, 0.2, 55.4) };

            _store.Save(track, withIntro, key: null, Levels);
            var found = _store.FindGrid(path);
            _store.Save(track, Grid, key: null, Levels);

            Assert.That(found, Is.EqualTo(withIntro));
            Assert.That(found!.Intro, Is.EqualTo(withIntro.Intro));
            Assert.That(_store.FindGrid(path), Is.EqualTo(Grid));
            Assert.That(Analysis(path).IntroBeatsPerMinute, Is.Zero);
        }

        [Test]
        public void AnAnalysis_OutlivesAScan()
        {
            var path = AddTrack("a.mp3");
            SaveAnalysis(Grid);

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

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void ATrackWithoutAKeyInItsTags_IsToBeAnalyzedForItsKeyToo(string? initialKey)
        {
            AddTrack("a.mp3", initialKey: initialKey!);

            var track = _store.TracksToAnalyze().Single();

            Assert.That((track.NeedsGrid, track.NeedsKey), Is.EqualTo((true, true)));
        }

        [TestCase("o")]
        [TestCase("Unknown")]
        public void ATrackWhoseTagsHaveSomethingElseThanAKey_IsToBeAnalyzedForItsKeyToo(string initialKey)
        {
            AddTrack("a.mp3", initialKey: initialKey);

            var track = _store.TracksToAnalyze().Single();

            Assert.That((track.NeedsGrid, track.NeedsKey), Is.EqualTo((true, true)));
            Assert.That(track.TaggedKey, Is.EqualTo(initialKey));
        }

        // Written another way than on the Camelot wheel, it's still the key.
        [TestCase("Am")]
        [TestCase("8a")]
        [TestCase("1m")]
        public void ATrackWithAKeyInAnotherNotation_IsntAnalyzedForIt(string initialKey)
        {
            AddTrack("a.mp3", initialKey: initialKey);

            var track = _store.TracksToAnalyze().Single();

            Assert.That((track.NeedsGrid, track.NeedsKey), Is.EqualTo((true, false)));
        }

        [Test]
        public void AKeyThatWasFound_IsKept_AndIsntLookedForAgain()
        {
            var path = AddTrack("a.mp3", initialKey: "");

            SaveAnalysis(Grid, AMinor);

            Assert.That(Analysis(path).Key, Is.EqualTo("8A"));
            Assert.That(_store.FindGrid(path), Is.EqualTo(Grid));
            Assert.That(_store.TracksToAnalyze(), Is.Empty);
        }

        [Test]
        public void ATrackWithoutAKeyToFind_IsntLookedAtAgain()
        {
            var path = AddTrack("a.mp3", initialKey: "");

            SaveAnalysis(Grid, key: null);

            Assert.That(Analysis(path).Key, Is.Empty);
            Assert.That(_store.TracksToAnalyze(), Is.Empty);
        }

        [Test]
        public void ATrackThatLosesTheKeyInItsTags_IsAnalyzedForItsKeyOnly_AndKeepsItsGrid()
        {
            var path = AddTrack("a.mp3", initialKey: "8A");
            SaveAnalysis(Grid);
            // A scan after the tag was removed, by something that keeps the file's size and time.
            using (var context = new LalaContext(_options))
            {
                context.Set<Track>().Single().InitialKey = "";
                context.SaveChanges();
            }

            var track = _store.TracksToAnalyze().Single();
            _store.Save(track, grid: null, AMinor, waveform: null);

            Assert.That((track.NeedsGrid, track.NeedsKey, track.NeedsWaveform), Is.EqualTo((false, true, false)));
            Assert.That(_store.FindGrid(path), Is.EqualTo(Grid));
            Assert.That(Analysis(path).Key, Is.EqualTo("8A"));
            Assert.That(_store.TracksToAnalyze(), Is.Empty);
        }

        [Test]
        public void AChangedFile_LosesTheKeyThatWasFoundInIt()
        {
            var path = AddTrack("a.mp3", initialKey: "");
            SaveAnalysis(Grid, AMinor);

            // Now with a key in its tags: only the beats are looked for.
            File.WriteAllBytes(path, [1, 2, 3, 4]);
            using (var context = new LalaContext(_options))
            {
                context.Set<Track>().Single().InitialKey = "5B";
                context.SaveChanges();
            }
            SaveAnalysis(Grid);

            Assert.That(Analysis(path).Key, Is.Empty);
            Assert.That(Analysis(path).KeyAnalyzerVersion, Is.Zero);
        }

        // With what it found when that's what this version finds unless the cues move the beats: version 2 found the tempo
        // of a track without a tagged one in another way.
        [TestCase(2, 128, true)]
        [TestCase(3, 128, true)]
        [TestCase(3, 0, true)]
        [TestCase(2, 0, false)]
        public void WhatTheBeatAnalysisBeforeFound_IsFoundAgain_AndPlayedByUntilThen(int version, int taggedBeatsPerMinute, bool isEarlier)
        {
            var path = AddTrack("a.mp3", taggedBeatsPerMinute);
            SaveAnalysis(Grid);
            using (var context = new LalaContext(_options))
            {
                context.Set<TrackAnalysis>().Single().AnalyzerVersion = version;
                context.SaveChanges();
            }

            Assert.That(_store.TracksToAnalyze().Select(t => (t.Path, t.NeedsGrid, t.Earlier, t.EarlierVersion)), Is.EqualTo(new[] { (path, true, isEarlier ? Grid : null, isEarlier ? version : 0) }));
            Assert.That(_store.FindGrid(path), Is.EqualTo(Grid));
        }

        // So that it isn't lost when it is saved again as what this version finds.
        [Test]
        public void WhatTheBeatAnalysisBeforeFound_IsFoundAgain_WithItsIntro()
        {
            AddTrack("a.mp3");
            var withIntro = new BeatGrid(127.998, 0.312, 2.5, 400, 1.249, 70.6, 180.5) { Intro = new BeatGrid(128.01, 0.2, 1.5, 120, 0.2, 0.2, 55.4) };
            SaveAnalysis(withIntro);
            using (var context = new LalaContext(_options))
            {
                context.Set<TrackAnalysis>().Single().AnalyzerVersion = BeatGridAnalyzer.Version - 1;
                context.SaveChanges();
            }

            var earlier = _store.TracksToAnalyze().Single().Earlier;

            Assert.That(earlier, Is.EqualTo(withIntro));
            Assert.That(earlier!.Intro, Is.EqualTo(withIntro.Intro));
        }

        [Test]
        public void WhereTheBeatAnalysisBeforeFoundNoBeat_ItIsLookedForAgain()
        {
            AddTrack("a.mp3", beatsPerMinute: 128);
            SaveAnalysis(grid: null);
            using (var context = new LalaContext(_options))
            {
                context.Set<TrackAnalysis>().Single().AnalyzerVersion = 3;
                context.SaveChanges();
            }

            Assert.That(_store.TracksToAnalyze().Single().Earlier, Is.Null);
        }

        [Test]
        public void WhatAnOldBeatAnalysisFound_IsFoundAgain()
        {
            var path = AddTrack("a.mp3", beatsPerMinute: 128);
            SaveAnalysis(Grid);
            using (var context = new LalaContext(_options))
            {
                context.Set<TrackAnalysis>().Single().AnalyzerVersion = 1;
                context.SaveChanges();
            }

            Assert.That(_store.TracksToAnalyze().Select(t => (t.Path, t.Earlier)), Is.EqualTo(new[] { (path, (BeatGrid?)null) }));
            Assert.That(_store.FindGrid(path), Is.Null);
        }

        [Test]
        public void AMissingFile_IsntToBeAnalyzed()
        {
            var path = AddTrack("a.mp3");
            File.Delete(path);

            Assert.That(_store.TracksToAnalyze(), Is.Empty);
        }

        [Test]
        public void FindGrid_InADatabaseThatCantBeRead_IsNull()
        {
            var path = AddTrack("a.mp3");
            var broken = Path.Combine(_dir, "broken.db");
            File.WriteAllText(broken, new string('x', 4096));
            var options = new DbContextOptionsBuilder<LalaContext>().UseSqlite($"Data Source={broken};Pooling=False").Options;
            var store = new TrackAnalysisStore(() => new LalaContext(options));

            Assert.That(store.FindGrid(path), Is.Null);
            Assert.That(store.FindWaveform(path), Is.Null);
        }

        [Test]
        public void AStoredWaveform_IsFound_AndIsntReadAgain()
        {
            var path = AddTrack("a.mp3");

            SaveAnalysis(Grid);

            Assert.That(_store.FindWaveform(path)?.ToBytes(), Is.EqualTo(Levels.ToBytes()));
            Assert.That(_store.TracksToAnalyze(), Is.Empty);
        }

        [Test]
        public void AnAnalyzedTrackWithoutAWaveform_IsReadForItsWaveformOnly_AndKeepsItsGrid()
        {
            var path = AddTrack("a.mp3");
            _store.Save(_store.TracksToAnalyze().Single(), Grid, key: null, waveform: null);

            var track = _store.TracksToAnalyze().Single();
            _store.Save(track, grid: null, key: null, Levels);

            Assert.That((track.NeedsGrid, track.NeedsKey, track.NeedsWaveform), Is.EqualTo((false, false, true)));
            Assert.That(_store.FindGrid(path), Is.EqualTo(Grid));
            Assert.That(_store.FindWaveform(path), Is.Not.Null);
            Assert.That(_store.TracksToAnalyze(), Is.Empty);
        }

        [Test]
        public void TheWaveformOfAChangedFile_IsGone_AndReadAgain()
        {
            var path = AddTrack("a.mp3");
            SaveAnalysis(Grid);

            File.WriteAllBytes(path, [1, 2, 3, 4]);

            Assert.That(_store.FindWaveform(path), Is.Null);
            Assert.That(_store.TracksToAnalyze().Single().NeedsWaveform, Is.True);
        }

        [Test]
        public void AWaveformOfAnotherVersion_IsReadAgain()
        {
            var path = AddTrack("a.mp3");
            SaveAnalysis(Grid);
            using (var context = new LalaContext(_options))
            {
                context.Set<TrackWaveform>().Single().Version = Waveform.Version - 1;
                context.SaveChanges();
            }

            Assert.That(_store.FindWaveform(path), Is.Null);
            Assert.That(_store.TracksToAnalyze().Select(t => (t.NeedsGrid, t.NeedsWaveform)), Is.EqualTo(new[] { (false, true) }));
        }

        [Test]
        public void AWaveformThePlayerRead_IsntReadByTheAnalysis_AndIsReplacedWhenItIs()
        {
            var path = AddTrack("a.mp3");

            _store.SaveWaveform(path, FileStamp.Of(path)!.Value, Waveform.FromBytes([1, 2]));
            var track = _store.TracksToAnalyze().Single();
            _store.SaveWaveform(path, FileStamp.Of(path)!.Value, Levels);

            Assert.That((track.NeedsGrid, track.NeedsWaveform), Is.EqualTo((true, false)));
            Assert.That(_store.FindWaveform(path)?.ToBytes(), Is.EqualTo(Levels.ToBytes()));
            using var context = new LalaContext(_options);
            Assert.That(context.Set<TrackWaveform>().Count(), Is.EqualTo(1));
        }
    }
}
