using System.Linq;
using LalaDb.Data;
using LalaDb.Model;
using LibLala.Audio.Analysis;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace LalaDb.Unittests
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "The teardown disposes them.")]
    public class DetectedTagsTest
    {
        private SqliteConnection _connection = null!;
        private DbContextOptions<LalaContext> _options = null!;
        private LalaContext _context = null!;

        [SetUp]
        public void SetUp()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();
            _options = new DbContextOptionsBuilder<LalaContext>()
                .UseLazyLoadingProxies()
                .UseSqlite(_connection)
                .Options;
            _context = new LalaContext(_options);
            _context.Database.Migrate();
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
            _connection.Dispose();
        }

        private static string PathOf(string title)
        {
            return $"/nowhere/{title}.mp3";
        }

        private Track AddTrack(string title, string initialKey = "", int beatsPerMinute = 0)
        {
            var track = new Track { Title = title, Path = PathOf(title), Length = 100, InitialKey = initialKey, BeatsPerMinute = beatsPerMinute, Album = new Album { Name = title }, TrackNumber = 1, DiscNumber = 1 };
            _context.Add(track);
            _context.SaveChanges();
            return track;
        }

        // A steady grid, unless told otherwise.
        private void AddAnalysis(string title, string key, double beatsPerMinute, double deviationMs = 2, int beatCount = 400)
        {
            using var context = new LalaContext(_options);
            context.Add(new TrackAnalysis
            {
                Path = PathOf(title),
                Key = key,
                KeyAnalyzerVersion = KeyAnalyzer.Version,
                BeatsPerMinute = beatsPerMinute,
                BeatDeviationMs = deviationMs,
                BeatCount = beatCount,
                AnalyzerVersion = BeatGridAnalyzer.Version,
            });
            context.SaveChanges();
        }

        private LalaTags Build(Track track)
        {
            return LalaTags.Build(track, DetectedTags.Load(_context));
        }

        [Test]
        public void ATrackWithoutKeyAndTempoInItsTags_HasWhatTheAnalysisFound()
        {
            var track = AddTrack("a");
            AddAnalysis("a", "8A", 127.998);

            var tags = Build(track);

            Assert.That(tags.InitialKey, Is.EqualTo("8A"));
            Assert.That(tags.BeatsPerMinute, Is.EqualTo(128));
            Assert.That(tags.BeatsPerMinuteString, Is.EqualTo("128"));
        }

        [Test]
        public void KeyAndTempoInTheTags_StayWhateverTheAnalysisFound()
        {
            var track = AddTrack("a", initialKey: "5B", beatsPerMinute: 140);
            AddAnalysis("a", "8A", 127.998);

            var tags = Build(track);

            Assert.That(tags.InitialKey, Is.EqualTo("5B"));
            Assert.That(tags.BeatsPerMinute, Is.EqualTo(140));
        }

        [Test]
        public void ATrackWithOnlyOneOfThemInItsTags_HasTheOtherFromTheAnalysis()
        {
            var keyed = AddTrack("keyed", initialKey: "5B");
            var timed = AddTrack("timed", beatsPerMinute: 140);
            AddAnalysis("keyed", "", 127.998);
            AddAnalysis("timed", "8A", 127.998);

            Assert.That((Build(keyed).InitialKey, Build(keyed).BeatsPerMinute), Is.EqualTo(("5B", 128u)));
            Assert.That((Build(timed).InitialKey, Build(timed).BeatsPerMinute), Is.EqualTo(("8A", 140u)));
        }

        [Test]
        public void ATrackThatIsntAnalyzed_HasNeither()
        {
            var track = AddTrack("a");

            var tags = Build(track);

            Assert.That(tags.InitialKey, Is.Empty);
            Assert.That(tags.BeatsPerMinuteString, Is.Empty);
        }

        [Test]
        public void WhatAnotherVersionOfTheAnalysisFound_DoesntShow()
        {
            var track = AddTrack("a");
            AddAnalysis("a", "8A", 127.998);
            using (var context = new LalaContext(_options))
            {
                var analysis = context.Set<TrackAnalysis>().Single();
                analysis.KeyAnalyzerVersion = KeyAnalyzer.Version - 1;
                analysis.AnalyzerVersion = BeatGridAnalyzer.Version - 1;
                context.SaveChanges();
            }

            var tags = Build(track);

            Assert.That(tags.InitialKey, Is.Empty);
            Assert.That(tags.BeatsPerMinute, Is.Zero);
        }

        [Test]
        public void WhatTheAnalysisFindsLater_ShowsInAContextThatHasReadItBefore()
        {
            var track = AddTrack("a");
            AddAnalysis("a", "", 0);
            Assert.That(Build(track).InitialKey, Is.Empty);

            // The analysis writes through contexts of its own.
            using (var context = new LalaContext(_options))
            {
                var analysis = context.Set<TrackAnalysis>().Single();
                analysis.Key = "8A";
                analysis.BeatsPerMinute = 127.998;
                context.SaveChanges();
            }

            Assert.That((Build(track).InitialKey, Build(track).BeatsPerMinute), Is.EqualTo(("8A", 128u)));
        }

        [Test]
        public void ThePlaylistsAndAlbums_HaveWhatTheAnalysisFoundToo()
        {
            AddTrack("a");
            AddAnalysis("a", "8A", 127.998);
            var playlists = new PlaylistModel(_context);
            playlists.CreatePlaylist("list");
            playlists.AddTracks("list", [PathOf("a")]);
            var library = new MusicLibModel(_context, () => new LalaContext(_options));

            Assert.That(playlists.GetPlaylistTracks("list").Single().InitialKey, Is.EqualTo("8A"));
            Assert.That(library.GetAlbumTracks("a").Single().BeatsPerMinute, Is.EqualTo(128));
        }
    }
}
