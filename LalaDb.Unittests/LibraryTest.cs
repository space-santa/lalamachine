using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LalaDb.Data;
using LalaDb.Model;
using LibLala.MusicScanner;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using TestHelpers;

namespace LalaDb.Unittests
{
    public class LibraryTest
    {
        private string _musicDir = "";

        [SetUp]
        public void SetUp()
        {
            _musicDir = Path.Combine(Path.GetTempPath(), $"lalatest-{Guid.NewGuid()}");
            Directory.CreateDirectory(_musicDir);
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(_musicDir, recursive: true);
        }

        private static SqliteConnection OpenDatabase()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();
            using var context = CreateContext(connection);
            context.Database.Migrate();
            return connection;
        }

        private static LalaContext CreateContext(SqliteConnection connection)
        {
            var options = new DbContextOptionsBuilder<LalaContext>()
                .UseLazyLoadingProxies()
                .UseSqlite(connection)
                .Options;
            return new LalaContext(options);
        }

        [Test]
        public void PlaylistTrackWithMissingFileStillLoads()
        {
            using var connection = OpenDatabase();
            using var context = CreateContext(connection);
            var missingPath = Path.Combine(_musicDir, "on-the-unplugged-drive.mp3");
            context.Tracks!.Add(new Track { Title = "Gone for now", Path = missingPath, Length = 100, TrackNumber = 1, DiscNumber = 1, Album = new Album { Name = "Album" } });
            context.PlaylistTracks!.Add(new PlaylistTrack { Playlist = new Playlist { Name = "list" }, TrackPath = missingPath, Order = 1 });
            context.SaveChanges();

            var tracks = new PlaylistModel(context).GetPlaylistTracks("list");

            Assert.That(tracks.Select(x => x.Title), Is.EqualTo(new[] { "Gone for now" }));
        }

        [Test]
        public async Task RescanDoesNotDuplicateTracks()
        {
            TestMp3.Write(Path.Combine(_musicDir, "a.mp3"), "A", "Artist", "Album");
            TestMp3.Write(Path.Combine(_musicDir, "b.mp3"), "B", "Artist", "Album");
            using var connection = OpenDatabase();
            using var context = CreateContext(connection);
            var model = new MusicLibModel(context, () => CreateContext(connection));

            await model.ScanAsync(_musicDir).ConfigureAwait(false);
            await model.ScanAsync(_musicDir).ConfigureAwait(false);

            Assert.That(model.DisplayLib([], [], [], "")!.Select(x => x!.Title).Order(), Is.EqualTo(new[] { "A", "B" }));
            Assert.That(model.ArtistList([], ""), Is.EqualTo(new[] { "Artist" }));
            Assert.That(model.AlbumList([], [], ""), Is.EqualTo(new[] { "Album" }));
        }

        // The search narrows them down while nothing above them is selected. What is selected was found by it: all its
        // artists and albums are listed, as all its tracks are. Sorted either way, in English: Estonian has Z before T.
        [Test]
        [SetCulture("en-US")]
        public void ArtistsAndAlbums_AreSearchedOnlyWithoutASelectionAbove_AndSorted()
        {
            using var connection = OpenDatabase();
            using var context = CreateContext(connection);
            var house = new Genre { Name = "House" };
            var techno = new Genre { Name = "Techno" };
            void Add(string artist, string album, Genre genre)
            {
                var track = new Track { Title = album, Path = Path.Combine(_musicDir, $"{album}.mp3"), Album = context.Albums!.Local.SingleOrDefault(a => a.Name == album) ?? new Album { Name = album } };
                context.ArtistTracks!.Add(new ArtistTrack { Artist = context.Artists!.Local.SingleOrDefault(a => a.Name == artist) ?? new Artist { Name = artist }, Track = track });
                context.GenreTracks!.Add(new GenreTrack { Genre = genre, Track = track });
            }
            Add("Zed Love", "Zebra Love", house);
            Add("Ann", "Love Songs", house);
            Add("Love Inc", "Apple", house);
            Add("Tim Love", "Techno Love", techno);
            context.SaveChanges();
            // As read from the database, like after a scan.
            using var readContext = CreateContext(connection);
            var model = new MusicLibModel(readContext, () => CreateContext(connection));

            Assert.That(model.ArtistList([], "love"), Is.EqualTo(new[] { "Love Inc", "Tim Love", "Zed Love" }));
            Assert.That(model.AlbumList([], [], "love"), Is.EqualTo(new[] { "Love Songs", "Techno Love", "Zebra Love" }));
            Assert.That(model.AlbumList([], [], ""), Is.EqualTo(new[] { "Apple", "Love Songs", "Techno Love", "Zebra Love" }));
            // Searched for "zed", then the artist clicked.
            Assert.That(model.AlbumList(["Zed Love"], [], "zed"), Is.EqualTo(new[] { "Zebra Love" }));
            // Searched for "house", then the genre clicked.
            Assert.That(model.ArtistList(["House"], "house"), Is.EqualTo(new[] { "Ann", "Love Inc", "Zed Love" }));
            Assert.That(model.AlbumList([], ["House"], "house"), Is.EqualTo(new[] { "Apple", "Love Songs", "Zebra Love" }));
            Assert.That(model.AlbumList(["Zed Love", "Ann"], [], "love"), Is.EqualTo(new[] { "Love Songs", "Zebra Love" }));
        }

        [Test]
        public async Task ScanKeepsInitialKeyAndBeatsPerMinute()
        {
            TestMp3.Write(Path.Combine(_musicDir, "a.mp3"), "A", album: "Album", initialKey: "10A", beatsPerMinute: 128);
            using var connection = OpenDatabase();
            using var context = CreateContext(connection);
            var model = new MusicLibModel(context, () => CreateContext(connection));

            await model.ScanAsync(_musicDir).ConfigureAwait(false);

            var track = model.GetAlbumTracks("Album").Single();
            Assert.That(track.InitialKey, Is.EqualTo("10A"));
            Assert.That(track.BeatsPerMinute, Is.EqualTo(128));
        }

        // The database has it on the Camelot wheel, like every key lalamachine shows.
        [Test]
        public async Task ScanStoresAKeyInAnotherNotationOnTheCamelotWheel()
        {
            TestMp3.Write(Path.Combine(_musicDir, "a.mp3"), "A", album: "Album", initialKey: "F#m");
            using var connection = OpenDatabase();
            using var context = CreateContext(connection);
            var model = new MusicLibModel(context, () => CreateContext(connection));

            await model.ScanAsync(_musicDir).ConfigureAwait(false);

            Assert.That(context.Tracks!.Single().InitialKey, Is.EqualTo("11A"));
        }

        [Test]
        public async Task ScanOfTrackWithoutInitialKeyAndBeatsPerMinuteLeavesThemEmpty()
        {
            TestMp3.Write(Path.Combine(_musicDir, "a.mp3"), "A", album: "Album");
            using var connection = OpenDatabase();
            using var context = CreateContext(connection);
            var model = new MusicLibModel(context, () => CreateContext(connection));

            await model.ScanAsync(_musicDir).ConfigureAwait(false);

            var track = model.GetAlbumTracks("Album").Single();
            Assert.That(track.InitialKey, Is.Empty);
            Assert.That(track.BeatsPerMinuteString, Is.Empty);
        }

        [Test]
        public void TrackFromBeforeTheColumnsExistedStillLoads()
        {
            using var connection = OpenDatabase();
            using var context = CreateContext(connection);
            context.Tracks!.Add(new Track { Title = "Old", Path = "/nowhere/old.mp3", Length = 100, TrackNumber = 1, DiscNumber = 1, Album = new Album { Name = "Album" } });
            context.SaveChanges();

            var track = new MusicLibModel(context, () => CreateContext(connection)).GetAlbumTracks("Album").Single();

            Assert.That(track.InitialKey, Is.Empty);
            Assert.That(track.BeatsPerMinuteString, Is.Empty);
        }

        [Test]
        public void ReusedScannerDbDoesNotDuplicateTracks()
        {
            TestMp3.Write(Path.Combine(_musicDir, "a.mp3"), "A");
            using var connection = OpenDatabase();
            using var context = CreateContext(connection);
            var scannerDb = new ScannerDb(context);

            new DirectoryProcessor(scannerDb, _musicDir).ProcessDirectory();
            new DirectoryProcessor(scannerDb, _musicDir).ProcessDirectory();

            Assert.That(context.Tracks!.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task RescanReplacesTheLibrary()
        {
            var oldPath = Path.Combine(_musicDir, "old.mp3");
            TestMp3.Write(oldPath, "Old");
            using var connection = OpenDatabase();
            using var context = CreateContext(connection);
            var model = new MusicLibModel(context, () => CreateContext(connection));
            await model.ScanAsync(_musicDir).ConfigureAwait(false);
            Assert.That(model.DisplayLib([], [], [], "")!.Select(x => x!.Title), Is.EqualTo(new[] { "Old" }));

            File.Delete(oldPath);
            TestMp3.Write(Path.Combine(_musicDir, "new.mp3"), "New");
            await model.ScanAsync(_musicDir).ConfigureAwait(false);

            Assert.That(model.DisplayLib([], [], [], "")!.Select(x => x!.Title), Is.EqualTo(new[] { "New" }));
        }

        [Test]
        public async Task UnreadableFileIsReportedAndTheRestIsScanned()
        {
            var brokenPath = Path.Combine(_musicDir, "broken.mp3");
            TestMp3.WriteBroken(brokenPath);
            TestMp3.Write(Path.Combine(_musicDir, "good.mp3"), "Good");
            using var connection = OpenDatabase();
            using var context = CreateContext(connection);
            var model = new MusicLibModel(context, () => CreateContext(connection));

            var skipped = await model.ScanAsync(_musicDir).ConfigureAwait(false);

            Assert.That(skipped, Is.EqualTo(new[] { brokenPath }));
            Assert.That(model.DisplayLib([], [], [], "")!.Select(x => x!.Title), Is.EqualTo(new[] { "Good" }));
        }

        [Test]
        public async Task FailedScanDoesNotLeaveTheLibraryStuckInScanning()
        {
            using var connection = OpenDatabase();
            using var context = CreateContext(connection);
            var model = new MusicLibModel(context, () => CreateContext(connection));

            await Assert.ThrowsAsync<DirectoryNotFoundException>(() => model.ScanAsync(Path.Combine(_musicDir, "not-there"))).ConfigureAwait(false);
            Assert.That(model.Scanning, Is.False);

            TestMp3.Write(Path.Combine(_musicDir, "a.mp3"), "A");
            await model.ScanAsync(_musicDir).ConfigureAwait(false);
            Assert.That(model.DisplayLib([], [], [], "")!.Select(x => x!.Title), Is.EqualTo(new[] { "A" }));
        }
    }
}
