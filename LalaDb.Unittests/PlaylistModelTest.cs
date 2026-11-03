using System.Linq;
using LalaDb.Data;
using LalaDb.Model;
using LibLala.DomainPrimitives;
using LibLala.LibLalaTagReader;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace LalaDb.Unittests
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "The teardown disposes them.")]
    public class PlaylistModelTest
    {
        private SqliteConnection _connection = null!;
        private LalaContext _context = null!;
        private PlaylistModel _model = null!;

        [SetUp]
        public void SetUp()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();
            var options = new DbContextOptionsBuilder<LalaContext>()
                .UseLazyLoadingProxies()
                .UseSqlite(_connection)
                .Options;
            _context = new LalaContext(options);
            _context.Database.Migrate();
            foreach (var title in new[] { "a", "b", "c" })
            {
                _context.Add(new Track { Title = title, Path = PathOf(title), Length = 100, Album = new Album { Name = title }, TrackNumber = 1, DiscNumber = 1 });
            }
            _context.SaveChanges();
            _model = new PlaylistModel(_context);
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

        private string[] Titles(string playlist)
        {
            return _model.GetPlaylistTracks(playlist).Select(x => x.Title).ToArray();
        }

        private void CreateWith(string playlist, params string[] titles)
        {
            _model.CreatePlaylist(playlist);
            foreach (var title in titles)
            {
                _model.AddTrack(playlist, PathOf(title));
            }
        }

        [Test]
        public void PlaylistNames_LeavesOutTheQueue()
        {
            _model.CreatePlaylist("list");
            _model.SavePlaylist(LibLala.Constants.MISCPLAYLISTNAME, []);

            Assert.That(_model.PlaylistNames(), Is.EqualTo(new[] { "list" }));
        }

        [Test]
        public void AddTrack_GoesToTheEnd()
        {
            CreateWith("list", "b", "a");

            Assert.That(Titles("list"), Is.EqualTo(new[] { "b", "a" }));
        }

        [Test]
        public void AddTrack_AlreadyThere_DoesNotAddItAgain()
        {
            CreateWith("list", "a", "a");

            Assert.That(Titles("list"), Is.EqualTo(new[] { "a" }));
        }

        [Test]
        public void AddTrack_NoSuchPlaylist_DoesNothing()
        {
            _model.AddTrack("not there", PathOf("a"));

            Assert.That(_model.PlaylistNames(), Is.Empty);
            Assert.That(_context.PlaylistTracks!.Count(), Is.EqualTo(0));
        }

        [Test]
        public void AddTrack_AfterRemovingOne_DoesNotReuseItsPlace()
        {
            CreateWith("list", "a", "b");
            _model.RemoveTrack("list", PathOf("a"));

            _model.AddTrack("list", PathOf("c"));

            Assert.That(Titles("list"), Is.EqualTo(new[] { "b", "c" }));
        }

        [Test]
        public void RemoveTrack_OnlyFromThatPlaylist()
        {
            CreateWith("one", "a", "b");
            CreateWith("two", "a");

            _model.RemoveTrack("one", PathOf("a"));

            Assert.That(Titles("one"), Is.EqualTo(new[] { "b" }));
            Assert.That(Titles("two"), Is.EqualTo(new[] { "a" }));
        }

        [Test]
        public void RenamePlaylist_KeepsTheTracks()
        {
            CreateWith("old", "a", "b");

            _model.RenamePlaylist("old", "new");

            Assert.That(_model.PlaylistNames(), Is.EqualTo(new[] { "new" }));
            Assert.That(Titles("new"), Is.EqualTo(new[] { "a", "b" }));
        }

        [Test]
        public void DeletePlaylist_RemovesItAndItsTracksOnly()
        {
            CreateWith("one", "a");
            CreateWith("two", "a");

            _model.DeletePlaylist("one");

            Assert.That(_model.PlaylistNames(), Is.EqualTo(new[] { "two" }));
            Assert.That(_context.PlaylistTracks!.Count(), Is.EqualTo(1));
            Assert.That(Titles("two"), Is.EqualTo(new[] { "a" }));
        }

        [Test]
        public void DeletePlaylist_NoSuchPlaylist_DoesNothing()
        {
            CreateWith("one", "a");

            _model.DeletePlaylist("not there");

            Assert.That(_model.PlaylistNames(), Is.EqualTo(new[] { "one" }));
        }

        [Test]
        public void SaveOrder_ChangesTheOrder()
        {
            CreateWith("list", "a", "b", "c");

            _model.SaveOrder("list", new[] { PathOf("c"), PathOf("a"), PathOf("b") });

            Assert.That(Titles("list"), Is.EqualTo(new[] { "c", "a", "b" }));
        }

        // A scan without the drive takes these out of the library, but they are back after the next one.
        [Test]
        public void SaveOrder_TrackNotInTheLibrary_StaysInThePlaylistAtTheEnd()
        {
            CreateWith("list", "a");
            _model.AddTrack("list", "/unplugged/d.mp3");
            _model.AddTrack("list", PathOf("b"));

            _model.SaveOrder("list", new[] { PathOf("b"), PathOf("a") });

            Assert.That(Titles("list"), Is.EqualTo(new[] { "b", "a" }));
            Assert.That(_model.TrackPaths("list"), Does.Contain("/unplugged/d.mp3"));
            Assert.That(_context.PlaylistTracks!.OrderBy(x => x.Order).Last().TrackPath, Is.EqualTo("/unplugged/d.mp3"));
        }

        [Test]
        public void PlaylistsContaining_ListsThePlaylistsWithTheTrack()
        {
            CreateWith("one", "a");
            CreateWith("two", "b");
            CreateWith("three", "a", "b");

            Assert.That(_model.PlaylistsContaining(PathOf("a")), Is.EquivalentTo(new[] { "one", "three" }));
            Assert.That(_model.PlaylistsContaining(PathOf("c")), Is.Empty);
        }

        [Test]
        public void TrackPaths_AreThePathsOfThePlaylist()
        {
            CreateWith("one", "a", "b");
            CreateWith("two", "c");

            Assert.That(_model.TrackPaths("one"), Is.EquivalentTo(new[] { PathOf("a"), PathOf("b") }));
        }

        // The queue can have the same track more than once.
        [Test]
        public void GetPlaylistTracks_SameTrackTwice_ListsItTwice()
        {
            var tracks = new[] { "a", "b", "a" }
                .Select(title => new LibLalaTagsBuilder(title, new UncheckedTrackPath(PathOf(title)), 100).Build())
                .ToList();

            _model.SavePlaylist(LibLala.Constants.MISCPLAYLISTNAME, tracks);

            Assert.That(Titles(LibLala.Constants.MISCPLAYLISTNAME), Is.EqualTo(new[] { "a", "b", "a" }));
        }

        [Test]
        public void GetPlaylistTracks_BrokenRow_SkipsItAndLoadsTheRest()
        {
            CreateWith("list", "a", "b");
            _context.Tracks!.Single(x => x.Title == "a").Title = null;
            _context.SaveChanges();

            Assert.That(Titles("list"), Is.EqualTo(new[] { "b" }));
        }
    }
}
