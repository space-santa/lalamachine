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
        public async Task RescanDoesNotDuplicateTracks()
        {
            TestMp3.Write(Path.Combine(_musicDir, "a.mp3"), "A", "Artist", "Album");
            TestMp3.Write(Path.Combine(_musicDir, "b.mp3"), "B", "Artist", "Album");
            using var connection = OpenDatabase();
            using var context = CreateContext(connection);
            var model = new MusicLibModel(context, () => CreateContext(connection));

            await model.ScanAsync(_musicDir).ConfigureAwait(false);
            await model.ScanAsync(_musicDir).ConfigureAwait(false);

            Assert.That(model.DisplayLib("", "", "", "")!.Select(x => x!.Title).Order(), Is.EqualTo(new[] { "A", "B" }));
            Assert.That(model.ArtistList("", ""), Is.EqualTo(new[] { "Artist" }));
            Assert.That(model.AlbumList("", "", ""), Is.EqualTo(new[] { "Album" }));
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
            Assert.That(model.DisplayLib("", "", "", "")!.Select(x => x!.Title), Is.EqualTo(new[] { "Old" }));

            File.Delete(oldPath);
            TestMp3.Write(Path.Combine(_musicDir, "new.mp3"), "New");
            await model.ScanAsync(_musicDir).ConfigureAwait(false);

            Assert.That(model.DisplayLib("", "", "", "")!.Select(x => x!.Title), Is.EqualTo(new[] { "New" }));
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
            Assert.That(model.DisplayLib("", "", "", "")!.Select(x => x!.Title), Is.EqualTo(new[] { "Good" }));
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
            Assert.That(model.DisplayLib("", "", "", "")!.Select(x => x!.Title), Is.EqualTo(new[] { "A" }));
        }
    }
}
