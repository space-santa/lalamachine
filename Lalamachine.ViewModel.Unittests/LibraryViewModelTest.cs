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

        // A genre that the filters under test leave out, to see that several genres aren't the same as no filter.
        private static void AddGenre(SqliteConnection connection, string name)
        {
            using var context = CreateContext(connection);
            var genre = new Genre { Name = name };
            var artist = new Artist { Name = $"{name}Artist" };
            context.AddRange(genre, artist);
            context.SaveChanges();
            AddTrack(context, $"{name}Track", $"{name}Album", artist, genre);
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

            Assert.That(viewModel.ArtistFilter, Is.EqualTo(new[] { "Alice" }));
            Assert.That(viewModel.AlbumFilter, Is.EqualTo(new[] { "AliceAlbum1" }));
            Assert.That(Titles(viewModel), Is.EqualTo(new[] { "a1" }));
        }

        [Test]
        public void SetGenreFilter_SeveralGenres_ListsWhatIsInAnyOfThem()
        {
            using var connection = OpenDatabase();
            AddGenre(connection, "Soul");
            using var context = CreateContext(connection);
            var viewModel = CreateViewModel(connection, context);

            viewModel.SetGenreFilterCommand.Execute(new[] { "Rock", "Jazz" });

            Assert.That(Titles(viewModel), Is.EqualTo(new[] { "a1", "a2", "b1", "b2" }));
            Assert.That(viewModel.ArtistList, Does.Contain("Alice").And.Contain("Bob"));
            Assert.That(viewModel.AlbumList, Does.Contain("AliceAlbum1").And.Contain("BobAlbum2"));
        }

        [Test]
        public void SetGenreFilter_TrackInBothGenres_IsListedOnce()
        {
            using var connection = OpenDatabase();
            using var context = CreateContext(connection);
            context.Add(new GenreTrack { GenreId = context.Genres!.Single(x => x.Name == "Jazz").GenreId, TrackId = context.Tracks!.Single(x => x.Title == "a1").TrackId });
            context.SaveChanges();
            var viewModel = CreateViewModel(connection, context);

            viewModel.SetGenreFilterCommand.Execute(new[] { "Rock", "Jazz" });

            Assert.That(Titles(viewModel), Is.EqualTo(new[] { "a1", "a2", "b1", "b2" }));
        }

        [Test]
        public void SetArtistFilter_SeveralArtists_ListsWhatIsByAnyOfThem()
        {
            using var connection = OpenDatabase();
            using var context = CreateContext(connection);
            var viewModel = CreateViewModel(connection, context);

            viewModel.SetArtistFilterCommand.Execute(new[] { "Alice", "Bob" });

            Assert.That(Titles(viewModel), Is.EqualTo(new[] { "a1", "a2", "b1", "b2" }));
            Assert.That(viewModel.AlbumList, Does.Contain("AliceAlbum1").And.Contain("BobAlbum2"));
        }

        [Test]
        public void SetArtistFilter_TrackByBothArtists_IsListedOnce()
        {
            using var connection = OpenDatabase();
            using var context = CreateContext(connection);
            context.Add(new ArtistTrack { ArtistId = context.Artists!.Single(x => x.Name == "Bob").ArtistId, TrackId = context.Tracks!.Single(x => x.Title == "a1").TrackId });
            context.SaveChanges();
            var viewModel = CreateViewModel(connection, context);

            viewModel.SetArtistFilterCommand.Execute(new[] { "Alice", "Bob" });

            Assert.That(Titles(viewModel), Is.EqualTo(new[] { "a1", "a2", "b1", "b2" }));
        }

        [Test]
        public void SetAlbumFilter_SeveralAlbums_ListsTheTracksOfAll()
        {
            using var connection = OpenDatabase();
            using var context = CreateContext(connection);
            var viewModel = CreateViewModel(connection, context);

            viewModel.SetAlbumFilterCommand.Execute(new[] { "AliceAlbum1", "BobAlbum2" });

            Assert.That(Titles(viewModel), Is.EqualTo(new[] { "a1", "b2" }));
        }

        // A list doesn't have to give back its selected items in the order it got them.
        [Test]
        public void SetFilter_SameItemsInAnotherOrder_DoesNotNotify()
        {
            using var connection = OpenDatabase();
            using var context = CreateContext(connection);
            var viewModel = CreateViewModel(connection, context);
            viewModel.SetGenreFilterCommand.Execute(new[] { "Rock", "Jazz" });
            var notifications = RecordNotifications(viewModel);

            viewModel.SetGenreFilterCommand.Execute(new[] { "Jazz", "Rock" });

            Assert.That(notifications, Is.Empty);
        }

        [Test]
        public void SetFilter_AllAmongTheSelection_MeansNoFilter()
        {
            using var connection = OpenDatabase();
            using var context = CreateContext(connection);
            var viewModel = CreateViewModel(connection, context);
            viewModel.SetGenreFilterCommand.Execute("Rock");

            viewModel.SetGenreFilterCommand.Execute(new[] { LibLala.Constants.ALL, "Rock" });

            Assert.That(viewModel.GenreFilter, Is.Empty);
            Assert.That(Titles(viewModel), Is.EqualTo(new[] { "a1", "a2", "b1", "b2" }));
        }

        [Test]
        public void SetFilter_NothingSelected_MeansNoFilter()
        {
            using var connection = OpenDatabase();
            using var context = CreateContext(connection);
            var viewModel = CreateViewModel(connection, context);
            viewModel.SetGenreFilterCommand.Execute("Rock");

            viewModel.SetGenreFilterCommand.Execute(System.Array.Empty<string>());

            Assert.That(viewModel.GenreFilter, Is.Empty);
        }

        [Test]
        public void SetGenreFilter_OneOfTheArtistsNotInGenre_KeepsTheOther()
        {
            using var connection = OpenDatabase();
            using var context = CreateContext(connection);
            var viewModel = CreateViewModel(connection, context);
            viewModel.SetArtistFilterCommand.Execute(new[] { "Alice", "Bob" });
            viewModel.SetAlbumFilterCommand.Execute(new[] { "AliceAlbum1", "BobAlbum1" });

            viewModel.SetGenreFilterCommand.Execute("Jazz");

            Assert.That(viewModel.ArtistFilter, Is.EqualTo(new[] { "Bob" }));
            Assert.That(viewModel.AlbumFilter, Is.EqualTo(new[] { "BobAlbum1" }));
            Assert.That(Titles(viewModel), Is.EqualTo(new[] { "b1" }));
        }

        [Test]
        public void AddTracksFromAlbum_SeveralAlbums_AddsTheTracksOfAll()
        {
            using var connection = OpenDatabase();
            using var context = CreateContext(connection);
            var viewModel = CreateViewModel(connection, context);
            AddTracksToPlaylistEventArgs? args = null;
            viewModel.AddTracksToPlaylistEvent += (o, e) => args = e;

            viewModel.AddTracksFromAlbumCommand.Execute(new[] { "AliceAlbum1", "BobAlbum2" });

            Assert.That(args!.Tracks.Select(track => track.Title), Is.EqualTo(new[] { "a1", "b2" }));
            Assert.That(args.NewPlaylist, Is.False);
        }

        [Test]
        public void CreateNewPlaylistFromAlbum_SeveralAlbums_AddsTheTracksOfAll()
        {
            using var connection = OpenDatabase();
            using var context = CreateContext(connection);
            var viewModel = CreateViewModel(connection, context);
            AddTracksToPlaylistEventArgs? args = null;
            viewModel.AddTracksToPlaylistEvent += (o, e) => args = e;

            viewModel.CreateNewPlaylistFromAlbumCommand.Execute(new[] { "AliceAlbum1", "BobAlbum2" });

            Assert.That(args!.Tracks.Select(track => track.Title), Is.EqualTo(new[] { "a1", "b2" }));
            Assert.That(args.NewPlaylist, Is.True);
        }

        private static string[] PlaylistAfterCreatingOneFrom(string[] selectedAlbums)
        {
            using var connection = OpenDatabase();
            using var context = CreateContext(connection);
            var viewModel = CreateViewModel(connection, context);
            var playlist = new PlaylistBaseViewModel("MAIN", Substitute.For<IDialogService>());
            viewModel.AddTracksToPlaylistEvent += playlist.AddTracksToPlaylistHandler;
            viewModel.AddTracksFromAlbumCommand.Execute(new[] { "AliceAlbum1" });

            viewModel.CreateNewPlaylistFromAlbumCommand.Execute(selectedAlbums);

            return playlist.Playlist.Select(track => track.Title).ToArray();
        }

        // A list has nothing selected after a filter change.
        [Test]
        public void CreateNewPlaylistFromAlbum_NothingSelected_KeepsThePlaylist()
        {
            Assert.That(PlaylistAfterCreatingOneFrom(System.Array.Empty<string>()), Is.EqualTo(new[] { "a1" }));
        }

        [Test]
        public void CreateNewPlaylistFromAlbum_AllSelected_KeepsThePlaylist()
        {
            Assert.That(PlaylistAfterCreatingOneFrom(new[] { LibLala.Constants.ALL }), Is.EqualTo(new[] { "a1" }));
        }

        [Test]
        public void CreateNewPlaylistFromAlbum_AlbumSelected_ReplacesThePlaylist()
        {
            Assert.That(PlaylistAfterCreatingOneFrom(new[] { "BobAlbum1" }), Is.EqualTo(new[] { "b1" }));
        }

        [TestCase(new[] { LibLala.Constants.ALL, "Jazz" }, new[] { "Jazz" }, new[] { "Jazz" })]
        [TestCase(new[] { LibLala.Constants.ALL, "Jazz" }, new[] { LibLala.Constants.ALL }, new[] { LibLala.Constants.ALL })]
        [TestCase(new[] { LibLala.Constants.ALL, "Jazz", "Rock" }, new[] { LibLala.Constants.ALL, "Rock" }, new[] { "Jazz", "Rock" })]
        [TestCase(new[] { LibLala.Constants.ALL, "Jazz", "Rock" }, new[] { "Jazz", "Rock" }, new[] { "Jazz", "Rock" })]
        [TestCase(new[] { "Jazz", "Rock" }, new[] { "Rock" }, new[] { "Jazz", "Rock" })]
        [TestCase(new[] { LibLala.Constants.ALL }, new[] { LibLala.Constants.ALL }, new[] { LibLala.Constants.ALL })]
        [TestCase(new string[0], new string[0], new string[0])]
        public void KeepAllOnItsOwn_ChangesTheSelection(string[] selected, string[] added, string[] expected)
        {
            var selection = new List<string>(selected);

            LibraryViewModel.KeepAllOnItsOwn(selection, added);

            Assert.That(selection, Is.EqualTo(expected));
        }

        [TestCase(new[] { "Rock" }, "Jazz", new[] { "Rock", "Jazz" })]
        [TestCase(new string[0], "Jazz", new[] { "Jazz" })]
        [TestCase(new[] { LibLala.Constants.ALL }, "Jazz", new[] { "Jazz" })]
        [TestCase(new[] { "Rock", "Jazz" }, "Rock", new[] { "Jazz" })]
        [TestCase(new[] { "Rock" }, "Rock", new[] { LibLala.Constants.ALL })]
        [TestCase(new[] { "Rock", "Jazz" }, LibLala.Constants.ALL, new[] { LibLala.Constants.ALL })]
        [TestCase(new[] { LibLala.Constants.ALL }, LibLala.Constants.ALL, new[] { LibLala.Constants.ALL })]
        public void ToggleFilterItem_ChangesTheSelection(string[] selected, string item, string[] expected)
        {
            var selection = new List<string>(selected);

            LibraryViewModel.ToggleFilterItem(selection, item);

            Assert.That(selection, Is.EqualTo(expected));
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
        public void ClearSearch_ClearsTheKeyFilterToo()
        {
            using var connection = OpenDatabase();
            using var context = CreateContext(connection);
            context.Tracks!.Single(x => x.Title == "a1").InitialKey = "8A";
            context.SaveChanges();
            var viewModel = CreateViewModel(connection, context);
            var playlist = new LibraryPlaylistViewModel(Substitute.For<IDialogService>());
            viewModel.DisplayLibChanged += playlist.DisplayChangedHandler;
            viewModel.FiltersCleared += playlist.ClearKeyFilterHandler;
            viewModel.SetGenreFilterCommand.Execute("Rock");
            playlist.KeyFilterOptions.Single(x => x.Key == "8A").IsSelected = true;
            Assert.That(playlist.Playlist.Select(track => track.Title), Is.EqualTo(new[] { "a1" }));

            viewModel.ClearSearchCommand.Execute(null);

            Assert.That(playlist.Playlist.Select(track => track.Title).OrderBy(title => title), Is.EqualTo(new[] { "a1", "a2", "b1", "b2" }));
            Assert.That(playlist.IsKeyFilterActive, Is.False);
            Assert.That(playlist.KeyFilterOptions.Any(x => x.IsSelected), Is.False);
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

        // The search box reports being emptied after clearing emptied it.
        [Test]
        public void ClearSearch_TwiceInARow_RebuildsOnce()
        {
            using var connection = OpenDatabase();
            using var context = CreateContext(connection);
            var viewModel = CreateViewModel(connection, context);
            viewModel.SearchString = "Rock";
            var rebuilds = 0;
            viewModel.DisplayLibChanged += (o, e) => ++rebuilds;

            viewModel.ClearSearchCommand.Execute(null);
            viewModel.ClearSearchCommand.Execute(null);

            Assert.That(rebuilds, Is.EqualTo(1));
        }

        [Test]
        public void ClearSearch_OnlyKeyFilterSet_StillClearsIt()
        {
            using var connection = OpenDatabase();
            using var context = CreateContext(connection);
            context.Tracks!.Single(x => x.Title == "a1").InitialKey = "8A";
            context.SaveChanges();
            var viewModel = CreateViewModel(connection, context);
            var playlist = new LibraryPlaylistViewModel(Substitute.For<IDialogService>());
            viewModel.DisplayLibChanged += playlist.DisplayChangedHandler;
            viewModel.FiltersCleared += playlist.ClearKeyFilterHandler;
            viewModel.UpdateListsCommand.Execute(null);
            playlist.KeyFilterOptions.Single(x => x.Key == "8A").IsSelected = true;

            viewModel.ClearSearchCommand.Execute(null);

            Assert.That(playlist.IsKeyFilterActive, Is.False);
            Assert.That(playlist.Playlist.Select(track => track.Title).OrderBy(title => title), Is.EqualTo(new[] { "a1", "a2", "b1", "b2" }));
        }
    }
}
