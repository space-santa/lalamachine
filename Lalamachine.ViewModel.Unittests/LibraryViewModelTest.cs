using System.Collections.Generic;
using System.Linq;
using LalaDb.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;

namespace Lalamachine.ViewModel.Unittests
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "<Pending>")]
    public class LibraryViewModelTest
    {
        private static SqliteConnection OpenDatabase()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();
            using var context = CreateContext(connection);
            context.Database.Migrate();

            var rock = new Genre { Name = "Rock" };
            var jazz = new Genre { Name = "Jazz" };
            var alice = new Artist { Name = "Alice" };
            var bob = new Artist { Name = "Bob" };
            context.AddRange(rock, jazz, alice, bob);
            context.SaveChanges();

            AddTrack(context, "a1", "AliceAlbum1", alice, rock);
            AddTrack(context, "a2", "AliceAlbum2", alice, rock);
            AddTrack(context, "b1", "BobAlbum1", bob, jazz);
            AddTrack(context, "b2", "BobAlbum2", bob, jazz);
            return connection;
        }

        private static void AddTrack(LalaContext context, string title, string albumName, Artist artist, Genre genre)
        {
            var album = new Album { Name = albumName };
            context.Add(album);
            context.SaveChanges();

            var track = new Track { Title = title, Path = $"/nowhere/{title}.mp3", Length = 100, AlbumId = album.AlbumId, TrackNumber = 1, DiscNumber = 1, Year = 2000, Comment = "" };
            context.Add(track);
            context.SaveChanges();

            context.Add(new ArtistTrack { ArtistId = artist.ArtistId, TrackId = track.TrackId });
            context.Add(new GenreTrack { GenreId = genre.GenreId, TrackId = track.TrackId });
            context.SaveChanges();
        }

        private static LalaContext CreateContext(SqliteConnection connection)
        {
            var options = new DbContextOptionsBuilder<LalaContext>()
                .UseLazyLoadingProxies()
                .UseSqlite(connection)
                .Options;
            return new LalaContext(options);
        }

        private static LibraryViewModel CreateViewModel(SqliteConnection connection, LalaContext context)
        {
            return new LibraryViewModel(context, () => CreateContext(connection), Substitute.For<IDialogService>());
        }

        private static List<string> RecordNotifications(LibraryViewModel viewModel)
        {
            var notifications = new List<string>();
            viewModel.PropertyChanged += (o, e) => notifications.Add(e.PropertyName ?? "");
            return notifications;
        }

        private static string[] Titles(LibraryViewModel viewModel)
        {
            return viewModel.DisplayLib.Select(track => track.Title).OrderBy(title => title).ToArray();
        }

        // The list control selects its item again when it gets replaced, which sets the same filter again.
        // If that replaced the lists again, it would never end.
        [Test]
        public void SetGenreFilter_SameValue_DoesNotNotify()
        {
            using var connection = OpenDatabase();
            using var context = CreateContext(connection);
            var viewModel = CreateViewModel(connection, context);
            viewModel.SetGenreFilterCommand.Execute("Rock");
            var notifications = RecordNotifications(viewModel);

            viewModel.SetGenreFilterCommand.Execute("Rock");

            Assert.That(notifications, Is.Empty);
        }

        [Test]
        public void SetArtistFilter_SameValue_DoesNotNotify()
        {
            using var connection = OpenDatabase();
            using var context = CreateContext(connection);
            var viewModel = CreateViewModel(connection, context);
            viewModel.SetArtistFilterCommand.Execute("Alice");
            var notifications = RecordNotifications(viewModel);

            viewModel.SetArtistFilterCommand.Execute("Alice");

            Assert.That(notifications, Is.Empty);
        }

        [Test]
        public void SetAlbumFilter_SameValue_DoesNotNotify()
        {
            using var connection = OpenDatabase();
            using var context = CreateContext(connection);
            var viewModel = CreateViewModel(connection, context);
            viewModel.SetAlbumFilterCommand.Execute("AliceAlbum1");
            var notifications = RecordNotifications(viewModel);

            viewModel.SetAlbumFilterCommand.Execute("AliceAlbum1");

            Assert.That(notifications, Is.Empty);
        }

        [Test]
        public void SetFilter_All_WhenNotFiltered_DoesNotNotify()
        {
            using var connection = OpenDatabase();
            using var context = CreateContext(connection);
            var viewModel = CreateViewModel(connection, context);
            var notifications = RecordNotifications(viewModel);

            viewModel.SetGenreFilterCommand.Execute(LibLala.Constants.ALL);

            Assert.That(notifications, Is.Empty);
        }

        [Test]
        public void SetGenreFilter_ReplacesTheListsBelowItOnly()
        {
            using var connection = OpenDatabase();
            using var context = CreateContext(connection);
            var viewModel = CreateViewModel(connection, context);
            var notifications = RecordNotifications(viewModel);

            viewModel.SetGenreFilterCommand.Execute("Rock");

            Assert.That(notifications, Does.Not.Contain(nameof(viewModel.GenreList)));
            Assert.That(notifications, Does.Contain(nameof(viewModel.ArtistList)));
            Assert.That(notifications, Does.Contain(nameof(viewModel.AlbumList)));
            Assert.That(Titles(viewModel), Is.EqualTo(new[] { "a1", "a2" }));
        }

        [Test]
        public void SetArtistFilter_ReplacesTheListsBelowItOnly()
        {
            using var connection = OpenDatabase();
            using var context = CreateContext(connection);
            var viewModel = CreateViewModel(connection, context);
            var notifications = RecordNotifications(viewModel);

            viewModel.SetArtistFilterCommand.Execute("Bob");

            Assert.That(notifications, Does.Not.Contain(nameof(viewModel.GenreList)));
            Assert.That(notifications, Does.Not.Contain(nameof(viewModel.ArtistList)));
            Assert.That(notifications, Does.Contain(nameof(viewModel.AlbumList)));
            Assert.That(Titles(viewModel), Is.EqualTo(new[] { "b1", "b2" }));
        }

        [Test]
        public void SetAlbumFilter_ReplacesNoList()
        {
            using var connection = OpenDatabase();
            using var context = CreateContext(connection);
            var viewModel = CreateViewModel(connection, context);
            var notifications = RecordNotifications(viewModel);

            viewModel.SetAlbumFilterCommand.Execute("BobAlbum1");

            Assert.That(notifications, Does.Not.Contain(nameof(viewModel.GenreList)));
            Assert.That(notifications, Does.Not.Contain(nameof(viewModel.ArtistList)));
            Assert.That(notifications, Does.Not.Contain(nameof(viewModel.AlbumList)));
            Assert.That(Titles(viewModel), Is.EqualTo(new[] { "b1" }));
        }

        [Test]
        public void SetArtistFilter_AlbumOfAnotherArtist_ClearsAlbumFilter()
        {
            using var connection = OpenDatabase();
            using var context = CreateContext(connection);
            var viewModel = CreateViewModel(connection, context);
            viewModel.SetAlbumFilterCommand.Execute("AliceAlbum1");

            viewModel.SetArtistFilterCommand.Execute("Bob");

            Assert.That(viewModel.AlbumFilter, Is.Empty);
            Assert.That(Titles(viewModel), Is.EqualTo(new[] { "b1", "b2" }));
        }

        [Test]
        public void SetGenreFilter_ArtistAndAlbumNotInGenre_ClearsBoth()
        {
            using var connection = OpenDatabase();
            using var context = CreateContext(connection);
            var viewModel = CreateViewModel(connection, context);
            viewModel.SetArtistFilterCommand.Execute("Alice");
            viewModel.SetAlbumFilterCommand.Execute("AliceAlbum1");

            viewModel.SetGenreFilterCommand.Execute("Jazz");

            Assert.That(viewModel.ArtistFilter, Is.Empty);
            Assert.That(viewModel.AlbumFilter, Is.Empty);
            Assert.That(Titles(viewModel), Is.EqualTo(new[] { "b1", "b2" }));
        }

        [Test]
        public void SetGenreFilter_ArtistAndAlbumInGenre_KeepsBoth()
        {
            using var connection = OpenDatabase();
            using var context = CreateContext(connection);
            var viewModel = CreateViewModel(connection, context);
            viewModel.SetArtistFilterCommand.Execute("Alice");
            viewModel.SetAlbumFilterCommand.Execute("AliceAlbum1");

            viewModel.SetGenreFilterCommand.Execute("Rock");

            Assert.That(viewModel.ArtistFilter, Is.EqualTo("Alice"));
            Assert.That(viewModel.AlbumFilter, Is.EqualTo("AliceAlbum1"));
            Assert.That(Titles(viewModel), Is.EqualTo(new[] { "a1" }));
        }

        [Test]
        public void SearchString_GenreNotFound_ClearsGenreFilter()
        {
            using var connection = OpenDatabase();
            using var context = CreateContext(connection);
            var viewModel = CreateViewModel(connection, context);
            viewModel.SetGenreFilterCommand.Execute("Rock");

            viewModel.SearchString = "Jazz";

            Assert.That(viewModel.GenreFilter, Is.Empty);
        }

        [Test]
        public void ClearSearch_ClearsAllFilters()
        {
            using var connection = OpenDatabase();
            using var context = CreateContext(connection);
            var viewModel = CreateViewModel(connection, context);
            viewModel.SetGenreFilterCommand.Execute("Rock");
            viewModel.SetArtistFilterCommand.Execute("Alice");
            viewModel.SetAlbumFilterCommand.Execute("AliceAlbum1");
            viewModel.SearchString = "a";

            viewModel.ClearSearchCommand.Execute(null);

            Assert.That(viewModel.GenreFilter, Is.Empty);
            Assert.That(viewModel.ArtistFilter, Is.Empty);
            Assert.That(viewModel.AlbumFilter, Is.Empty);
            Assert.That(viewModel.SearchString, Is.Empty);
            Assert.That(Titles(viewModel), Is.EqualTo(new[] { "a1", "a2", "b1", "b2" }));
        }

        [Test]
        public void ClearSearch_NoSearchString_StillUpdatesTheLists()
        {
            using var connection = OpenDatabase();
            using var context = CreateContext(connection);
            var viewModel = CreateViewModel(connection, context);
            viewModel.SetGenreFilterCommand.Execute("Rock");
            var notifications = RecordNotifications(viewModel);

            viewModel.ClearSearchCommand.Execute(null);

            Assert.That(notifications, Does.Contain(nameof(viewModel.ArtistList)));
            Assert.That(viewModel.ArtistList, Does.Contain("Bob"));
        }
    }
}
