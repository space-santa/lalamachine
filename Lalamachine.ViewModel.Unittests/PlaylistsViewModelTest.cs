using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using LalaDb.Data;
using LalaDb.Model;
using LibLala.DomainPrimitives;
using LibLala.LibLalaTagReader;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;

namespace Lalamachine.ViewModel.Unittests
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "The teardown disposes them.")]
    public class PlaylistsViewModelTest
    {
        private const string Liked = LibLala.Constants.LIKEDPLAYLISTNAME;

        private SqliteConnection _connection = null!;
        private LalaContext _context = null!;
        private IDialogService _dialogs = null!;

        [SetUp]
        public void SetUp()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();
            _context = CreateContext();
            _context.Database.Migrate();
            foreach (var title in new[] { "a", "b", "c" })
            {
                _context.Add(new Track { Title = title, Path = PathOf(title), Length = 100, Album = new Album { Name = title }, TrackNumber = 1, DiscNumber = 1 });
            }
            _context.SaveChanges();
            _dialogs = Substitute.For<IDialogService>();
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
            _connection.Dispose();
        }

        private LalaContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<LalaContext>()
                .UseLazyLoadingProxies()
                .UseSqlite(_connection)
                .Options;
            return new LalaContext(options);
        }

        private static string PathOf(string title)
        {
            return $"/nowhere/{title}.mp3";
        }

        private static LibLalaTags Tags(string title)
        {
            return new LibLalaTagsBuilder(title, new UncheckedTrackPath(PathOf(title)), 100).Build();
        }

        // Each with the given tracks in the database.
        private PlaylistsViewModel ViewModelWith(params (string Name, string[] Titles)[] playlists)
        {
            var model = new PlaylistModel(_context);
            foreach (var (name, titles) in playlists)
            {
                model.CreatePlaylist(name);
                model.AddTracks(name, titles.Select(PathOf));
            }
            return new PlaylistsViewModel(_context, _dialogs);
        }

        private static string[] Names(PlaylistsViewModel viewModel)
        {
            return viewModel.Playlists.Select(x => x.Name).ToArray();
        }

        private static NamedPlaylistViewModel Select(PlaylistsViewModel viewModel, string name)
        {
            viewModel.SelectedPlaylist = viewModel.Playlists.Single(x => x.Name == name);
            return viewModel.SelectedPlaylist;
        }

        private static string[] Titles(NamedPlaylistViewModel playlist)
        {
            return playlist.Playlist.Select(x => x.Title).ToArray();
        }

        private string[] SavedTitles(string name)
        {
            // A new context, so that this is what really is in the database.
            using var context = CreateContext();
            return new PlaylistModel(context).GetPlaylistTracks(name).Select(x => x.Title).ToArray();
        }

        [Test]
        public void PushPlaylist_SendsTheSelectedPlaylistInItsSavedOrder()
        {
            ViewModelWith(("Mix", ["c", "a", "b"]), ("Other", ["a"]));
            var pusher = Substitute.For<IPlaylistPusher>();
            pusher.PushAsync(Arg.Any<string>(), Arg.Any<IReadOnlyList<string>>()).Returns(Task.CompletedTask);
            var viewModel = new PlaylistsViewModel(_context, _dialogs, pusher);
            var playlist = Select(viewModel, "Mix");
            playlist.MoveTrack(playlist.Playlist[0], 2);
            playlist.SaveOrderCommand.Execute(null);

            viewModel.PushPlaylistCommand.Execute(null);

            pusher.Received(1).PushAsync("Mix", Arg.Is<IReadOnlyList<string>>(x => x.SequenceEqual(new[] { PathOf("a"), PathOf("b"), PathOf("c") })));
        }

        [Test]
        public void PushPlaylist_NotWithANewOrderThatIsntSaved()
        {
            ViewModelWith(("Mix", ["a", "b"]));
            var pusher = Substitute.For<IPlaylistPusher>();
            var viewModel = new PlaylistsViewModel(_context, _dialogs, pusher);
            var playlist = Select(viewModel, "Mix");
            playlist.MoveTrack(playlist.Playlist[0], 1);

            viewModel.PushPlaylistCommand.Execute(null);

            pusher.DidNotReceive().PushAsync(Arg.Any<string>(), Arg.Any<IReadOnlyList<string>>());
            Assert.That(viewModel.IsPushing, Is.False);
        }

        [Test]
        public void PushPlaylist_WaitsForTheOneThatIsRunning()
        {
            ViewModelWith(("Mix", ["a"]));
            var pusher = Substitute.For<IPlaylistPusher>();
            var running = new TaskCompletionSource();
            pusher.PushAsync(Arg.Any<string>(), Arg.Any<IReadOnlyList<string>>()).Returns(running.Task);
            var viewModel = new PlaylistsViewModel(_context, _dialogs, pusher);
            Select(viewModel, "Mix");

            viewModel.PushPlaylistCommand.Execute(null);
            Assert.That(viewModel.IsPushing, Is.True);
            viewModel.PushPlaylistCommand.Execute(null);

            pusher.Received(1).PushAsync(Arg.Any<string>(), Arg.Any<IReadOnlyList<string>>());
            running.SetResult();
            Assert.That(viewModel.IsPushing, Is.False);
        }

        [Test]
        public void PushPlaylist_DoesNothingWithoutAPusher()
        {
            var viewModel = ViewModelWith(("Mix", ["a"]));
            Select(viewModel, "Mix");

            viewModel.PushPlaylistCommand.Execute(null);

            Assert.That(viewModel.IsPushing, Is.False);
        }

        [Test]
        public void PullPlaylist_ReplacesTheTracksAndShowsThePlaylist()
        {
            ViewModelWith(("Mix", ["a", "b"]));
            var puller = Substitute.For<IPlaylistPuller>();
            puller.PullAsync(Arg.Any<ILocalPlaylists>()).Returns(x =>
            {
                x.Arg<ILocalPlaylists>().Replace("Mix", [PathOf("c"), PathOf("a")]);
                return Task.CompletedTask;
            });
            var viewModel = new PlaylistsViewModel(_context, _dialogs, null, puller);
            var playlist = Select(viewModel, "Mix");
            Select(viewModel, Liked);

            viewModel.PullPlaylistCommand.Execute(null);

            Assert.That(viewModel.SelectedPlaylist, Is.SameAs(playlist));
            Assert.That(Titles(playlist), Is.EqualTo(new[] { "c", "a" }));
            Assert.That(playlist.HasUnsavedOrder, Is.False);
            Assert.That(SavedTitles("Mix"), Is.EqualTo(new[] { "c", "a" }));
            Assert.That(viewModel.IsPulling, Is.False);
        }

        [Test]
        public void PullPlaylist_KeepsTheNameHereWhenItOnlyDiffersInCase()
        {
            var viewModel = ViewModelWith(("Mix", ["a"]));
            ILocalPlaylists playlists = viewModel;

            playlists.Replace("MIX", [PathOf("b")]);

            Assert.That(Names(viewModel), Is.EqualTo(new[] { Liked, "Mix" }));
            Assert.That(SavedTitles("Mix"), Is.EqualTo(new[] { "b" }));
        }

        [Test]
        public void PullPlaylist_CreatesANewOneInItsPlace()
        {
            var viewModel = ViewModelWith(("Aaa", ["a"]), ("Ccc", ["c"]));
            ILocalPlaylists playlists = viewModel;

            playlists.Replace("Bbb", [PathOf("b"), PathOf("a")]);

            Assert.That(Names(viewModel), Is.EqualTo(new[] { Liked, "Aaa", "Bbb", "Ccc" }));
            Assert.That(viewModel.SelectedPlaylist!.Name, Is.EqualTo("Bbb"));
            Assert.That(Titles(viewModel.SelectedPlaylist), Is.EqualTo(new[] { "b", "a" }));
            Assert.That(SavedTitles("Bbb"), Is.EqualTo(new[] { "b", "a" }));
        }

        [Test]
        public void PullPlaylist_FindsThePlaylistHereIgnoringCase()
        {
            var viewModel = ViewModelWith(("Mix", ["a", "b"]));
            new PlaylistModel(_context).AddTracks("Mix", ["/nowhere/gone.mp3"]);
            ILocalPlaylists playlists = viewModel;

            // Also the track the library doesn't have, it gets replaced as well.
            Assert.That(playlists.Find("mix"), Is.EqualTo(new LocalPlaylist("Mix", 3, false)));
            Assert.That(playlists.Find("Other"), Is.Null);
        }

        [Test]
        public void PullPlaylist_FindSaysWhenThereIsANewOrderThatIsntSaved()
        {
            var viewModel = ViewModelWith(("Mix", ["a", "b"]));
            var playlist = Select(viewModel, "Mix");
            playlist.MoveTrack(playlist.Playlist[0], 1);

            Assert.That(((ILocalPlaylists)viewModel).Find("Mix")!.HasUnsavedOrder, Is.True);
        }

        [Test]
        public void PullPlaylist_IntoLikedChangesTheHearts()
        {
            var viewModel = ViewModelWith((Liked, ["a", "b"]), ("Mix", ["a", "b", "c"]));
            var mix = Select(viewModel, "Mix");
            var changes = new List<(string[] Paths, bool IsLiked)>();
            viewModel.LikedChanged += (o, e) => changes.Add((e.Paths.ToArray(), e.IsLiked));

            ((ILocalPlaylists)viewModel).Replace(Liked, [PathOf("c"), PathOf("b")]);

            Assert.That(viewModel.IsLiked(PathOf("a")), Is.False);
            Assert.That(viewModel.IsLiked(PathOf("b")), Is.True);
            Assert.That(viewModel.IsLiked(PathOf("c")), Is.True);
            Assert.That(changes, Has.Count.EqualTo(2));
            Assert.That(changes[0].Paths, Is.EqualTo(new[] { PathOf("a") }));
            Assert.That(changes[0].IsLiked, Is.False);
            Assert.That(changes[1].Paths, Is.EqualTo(new[] { PathOf("c") }));
            Assert.That(changes[1].IsLiked, Is.True);
            Assert.That(mix.Playlist.Select(x => x.IsLiked), Is.EqualTo(new[] { false, true, true }));
            Assert.That(viewModel.SelectedPlaylist!.Playlist.Select(x => x.IsLiked), Is.All.True);
        }

        [Test]
        public void PullPlaylist_IntoAnotherPlaylistLeavesTheHearts()
        {
            var viewModel = ViewModelWith((Liked, ["a"]), ("Mix", ["a"]));
            var likedChanges = 0;
            viewModel.LikedChanged += (o, e) => ++likedChanges;

            ((ILocalPlaylists)viewModel).Replace("Mix", [PathOf("b")]);

            Assert.That(viewModel.IsLiked(PathOf("a")), Is.True);
            Assert.That(likedChanges, Is.EqualTo(0));
        }

        [Test]
        public void PullPlaylist_WaitsForTheOneThatIsRunning()
        {
            ViewModelWith();
            var puller = Substitute.For<IPlaylistPuller>();
            var running = new TaskCompletionSource();
            puller.PullAsync(Arg.Any<ILocalPlaylists>()).Returns(running.Task);
            var viewModel = new PlaylistsViewModel(_context, _dialogs, null, puller);

            viewModel.PullPlaylistCommand.Execute(null);
            Assert.That(viewModel.IsPulling, Is.True);
            viewModel.PullPlaylistCommand.Execute(null);

            puller.Received(1).PullAsync(Arg.Any<ILocalPlaylists>());
            running.SetResult();
            Assert.That(viewModel.IsPulling, Is.False);
        }

        [Test]
        public void PullPlaylist_DoesNothingWithoutAPuller()
        {
            var viewModel = ViewModelWith();

            viewModel.PullPlaylistCommand.Execute(null);

            Assert.That(viewModel.IsPulling, Is.False);
        }

        private void NameTheNextPlaylist(string name)
        {
            _dialogs.AskForTextAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(Task.FromResult<string?>(name));
        }

        [Test]
        public void Liked_IsCreatedOnce_AndAlwaysFirst()
        {
            var viewModel = ViewModelWith(("Aaa", []), ("zzz", []), ("Mmm", []));
            Assert.That(Names(viewModel), Is.EqualTo(new[] { Liked, "Aaa", "Mmm", "zzz" }));

            var again = new PlaylistsViewModel(_context, _dialogs);
            Assert.That(Names(again), Is.EqualTo(new[] { Liked, "Aaa", "Mmm", "zzz" }));
        }

        [Test]
        public void Liked_IsSelectedToStartWith()
        {
            var viewModel = ViewModelWith(("Liked", ["a"]));

            Assert.That(viewModel.SelectedPlaylist!.Name, Is.EqualTo(Liked));
            Assert.That(Titles(viewModel.SelectedPlaylist), Is.EqualTo(new[] { "a" }));
        }

        [Test]
        public void TheQueueIsNotListed()
        {
            new PlaylistModel(_context).SavePlaylist(LibLala.Constants.MISCPLAYLISTNAME, [Tags("a")]);

            var viewModel = new PlaylistsViewModel(_context, _dialogs);

            Assert.That(Names(viewModel), Is.EqualTo(new[] { Liked }));
            Assert.That(viewModel.ChoicesFor([Tags("a")]).Choices.Select(x => x.Name), Is.EqualTo(new[] { Liked }));
        }

        [Test]
        public void AddPlaylist_ListsItInOrderAndSelectsIt()
        {
            var viewModel = ViewModelWith(("Aaa", []), ("Ccc", []));
            NameTheNextPlaylist("  Bbb ");

            viewModel.AddPlaylistCommand.Execute(null);

            Assert.That(Names(viewModel), Is.EqualTo(new[] { Liked, "Aaa", "Bbb", "Ccc" }));
            Assert.That(viewModel.SelectedPlaylist!.Name, Is.EqualTo("Bbb"));
            Assert.That(new PlaylistModel(_context).PlaylistNames(), Does.Contain("Bbb"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void AddPlaylist_NoName_DoesNothing(string? name)
        {
            var viewModel = ViewModelWith();
            _dialogs.AskForTextAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(Task.FromResult(name));

            viewModel.AddPlaylistCommand.Execute(null);

            Assert.That(Names(viewModel), Is.EqualTo(new[] { Liked }));
            _dialogs.DidNotReceive().ShowMessageAsync(Arg.Any<string>());
        }

        [TestCase("Chill")]
        [TestCase("CHILL")]
        [TestCase("liked")]
        [TestCase(LibLala.Constants.MISCPLAYLISTNAME)]
        public void AddPlaylist_NameTaken_SaysSoAndDoesNothing(string name)
        {
            var viewModel = ViewModelWith(("Chill", []));
            NameTheNextPlaylist(name);

            viewModel.AddPlaylistCommand.Execute(null);

            Assert.That(Names(viewModel), Is.EqualTo(new[] { Liked, "Chill" }));
            _dialogs.Received(1).ShowMessageAsync(Arg.Is<string>(x => x.Contains(name, System.StringComparison.Ordinal)));
        }

        [Test]
        public void RenamePlaylist_KeepsTheTracksAndTheOrderOfTheList()
        {
            var viewModel = ViewModelWith(("Aaa", ["a", "b"]), ("Mmm", []));
            Select(viewModel, "Aaa");
            NameTheNextPlaylist("Zzz");

            viewModel.RenamePlaylistCommand.Execute(null);

            Assert.That(Names(viewModel), Is.EqualTo(new[] { Liked, "Mmm", "Zzz" }));
            Assert.That(viewModel.SelectedPlaylist!.Name, Is.EqualTo("Zzz"));
            Assert.That(Titles(viewModel.SelectedPlaylist), Is.EqualTo(new[] { "a", "b" }));
            Assert.That(SavedTitles("Zzz"), Is.EqualTo(new[] { "a", "b" }));
        }

        [Test]
        public void RenamePlaylist_OnlyTheCase_IsFine()
        {
            var viewModel = ViewModelWith(("chill", []));
            Select(viewModel, "chill");
            NameTheNextPlaylist("Chill");

            viewModel.RenamePlaylistCommand.Execute(null);

            Assert.That(Names(viewModel), Is.EqualTo(new[] { Liked, "Chill" }));
            _dialogs.DidNotReceive().ShowMessageAsync(Arg.Any<string>());
        }

        [Test]
        public void RenamePlaylist_NameTaken_KeepsTheName()
        {
            var viewModel = ViewModelWith(("Aaa", []), ("Bbb", []));
            Select(viewModel, "Aaa");
            NameTheNextPlaylist("bbb");

            viewModel.RenamePlaylistCommand.Execute(null);

            Assert.That(Names(viewModel), Is.EqualTo(new[] { Liked, "Aaa", "Bbb" }));
        }

        [Test]
        public void Liked_CanNotBeRenamedOrDeleted()
        {
            var viewModel = ViewModelWith();
            NameTheNextPlaylist("Other");
            _dialogs.ConfirmAsync(Arg.Any<string>()).Returns(Task.FromResult(true));

            viewModel.RenamePlaylistCommand.Execute(null);
            viewModel.DeletePlaylistCommand.Execute(null);

            Assert.That(viewModel.SelectedPlaylist!.CanRenameAndDelete, Is.False);
            Assert.That(Names(viewModel), Is.EqualTo(new[] { Liked }));
        }

        [Test]
        public void DeletePlaylist_Confirmed_IsGoneAndSelectsTheNext()
        {
            var viewModel = ViewModelWith(("Aaa", ["a"]), ("Bbb", []));
            Select(viewModel, "Aaa");
            _dialogs.ConfirmAsync(Arg.Any<string>()).Returns(Task.FromResult(true));

            viewModel.DeletePlaylistCommand.Execute(null);

            Assert.That(Names(viewModel), Is.EqualTo(new[] { Liked, "Bbb" }));
            Assert.That(viewModel.SelectedPlaylist!.Name, Is.EqualTo("Bbb"));
            Assert.That(new PlaylistModel(_context).PlaylistNames(), Does.Not.Contain("Aaa"));
        }

        [Test]
        public void DeletePlaylist_TheLastInTheList_SelectsTheOneBefore()
        {
            var viewModel = ViewModelWith(("Aaa", []));
            Select(viewModel, "Aaa");
            _dialogs.ConfirmAsync(Arg.Any<string>()).Returns(Task.FromResult(true));

            viewModel.DeletePlaylistCommand.Execute(null);

            Assert.That(viewModel.SelectedPlaylist!.Name, Is.EqualTo(Liked));
        }

        [Test]
        public void DeletePlaylist_NotConfirmed_KeepsIt()
        {
            var viewModel = ViewModelWith(("Aaa", []));
            Select(viewModel, "Aaa");

            viewModel.DeletePlaylistCommand.Execute(null);

            Assert.That(Names(viewModel), Is.EqualTo(new[] { Liked, "Aaa" }));
        }

        [Test]
        public void DeletePlaylist_WhilePlaying_NextPlaysNothing()
        {
            var viewModel = ViewModelWith(("Aaa", ["a", "b"]), ("Bbb", ["c"]));
            var playlist = Select(viewModel, "Aaa");
            playlist.PlayTrackCommand.Execute(playlist.Playlist[0]);
            _dialogs.ConfirmAsync(Arg.Any<string>()).Returns(Task.FromResult(true));
            viewModel.DeletePlaylistCommand.Execute(null);
            var wasCalled = false;
            viewModel.PlayTrackEvent += (o, e) => wasCalled = true;

            viewModel.PlayNextTrackHandler(null, System.EventArgs.Empty);

            Assert.That(wasCalled, Is.False);
        }

        // Stopping once is fine, the buttons staying dead until a track gets double-clicked isn't.
        [Test]
        public void DeletePlaylist_WhilePlaying_AfterThatNextPlaysTheSelectedPlaylist()
        {
            var viewModel = ViewModelWith(("Aaa", ["a", "b"]), ("Bbb", ["c"]));
            var playlist = Select(viewModel, "Aaa");
            playlist.PlayTrackCommand.Execute(playlist.Playlist[0]);
            _dialogs.ConfirmAsync(Arg.Any<string>()).Returns(Task.FromResult(true));
            viewModel.DeletePlaylistCommand.Execute(null);
            var playedPath = "";
            viewModel.PlayTrackEvent += (o, e) => playedPath = e.Path;

            viewModel.PlayNextTrackHandler(null, System.EventArgs.Empty);
            Assert.That(playedPath, Is.Empty);

            viewModel.PlayNextTrackHandler(null, System.EventArgs.Empty);
            Assert.That(playedPath, Is.EqualTo(PathOf("c")));
        }

        // Asked for a track to mix in before the end, which is no next yet.
        [Test]
        public void DeletePlaylist_WhilePlaying_NothingIsNextToMixIn_AndNextAfterThatStillStops()
        {
            var viewModel = ViewModelWith(("Aaa", ["a", "b"]), ("Bbb", ["c"]));
            var playlist = Select(viewModel, "Aaa");
            playlist.PlayTrackCommand.Execute(playlist.Playlist[0]);
            _dialogs.ConfirmAsync(Arg.Any<string>()).Returns(Task.FromResult(true));
            viewModel.DeletePlaylistCommand.Execute(null);
            var playedPath = "";
            viewModel.PlayTrackEvent += (o, e) => playedPath = e.Path;
            var next = new NextTrackEventArgs();

            viewModel.PeekNextTrackHandler(null, next);
            Assert.That(next.Track, Is.Null);

            viewModel.PlayNextTrackHandler(null, System.EventArgs.Empty);
            Assert.That(playedPath, Is.Empty);
        }

        [Test]
        public void DeletePlaylist_WhilePlaying_PreviousAfterThatPlaysTheSelectedPlaylist()
        {
            var viewModel = ViewModelWith(("Aaa", ["a", "b"]), ("Bbb", ["c"]));
            var playlist = Select(viewModel, "Aaa");
            playlist.PlayTrackCommand.Execute(playlist.Playlist[0]);
            _dialogs.ConfirmAsync(Arg.Any<string>()).Returns(Task.FromResult(true));
            viewModel.DeletePlaylistCommand.Execute(null);
            var playedPath = "";
            viewModel.PlayTrackEvent += (o, e) => playedPath = e.Path;

            viewModel.PlayLastTrackHandler(null, System.EventArgs.Empty);
            Assert.That(playedPath, Is.Empty);

            viewModel.PlayLastTrackHandler(null, System.EventArgs.Empty);
            Assert.That(playedPath, Is.EqualTo(PathOf("c")));
        }

        // The one stop is for the deleted playlist. A track played from another one just goes on.
        [Test]
        public void DeletePlaylist_WhilePlaying_ThenPlayingAnother_NextGoesOnFromThatOne()
        {
            var viewModel = ViewModelWith(("Aaa", ["a"]), ("Bbb", ["b", "c"]));
            var playlist = Select(viewModel, "Aaa");
            playlist.PlayTrackCommand.Execute(playlist.Playlist[0]);
            _dialogs.ConfirmAsync(Arg.Any<string>()).Returns(Task.FromResult(true));
            viewModel.DeletePlaylistCommand.Execute(null);
            var other = viewModel.SelectedPlaylist!;
            other.PlayTrackCommand.Execute(other.Playlist[0]);
            var playedPath = "";
            viewModel.PlayTrackEvent += (o, e) => playedPath = e.Path;

            viewModel.PlayNextTrackHandler(null, System.EventArgs.Empty);

            Assert.That(playedPath, Is.EqualTo(PathOf("c")));
        }

        [Test]
        public void PlayingPlaylistName_AfterARestart_NextPlaysThatPlaylist()
        {
            var viewModel = ViewModelWith(("Aaa", ["a"]), ("Bbb", ["b", "c"]));
            var playlist = Select(viewModel, "Bbb");
            playlist.PlayTrackCommand.Execute(playlist.Playlist[1]);
            var name = viewModel.PlayingPlaylistName;

            var again = new PlaylistsViewModel(_context, _dialogs) { PlayingPlaylistName = name };
            var playedPath = "";
            again.PlayTrackEvent += (o, e) => playedPath = e.Path;
            again.PlayNextTrackHandler(null, System.EventArgs.Empty);

            Assert.That(name, Is.EqualTo("Bbb"));
            Assert.That(playedPath, Is.EqualTo(PathOf("b")));
            Assert.That(again.SelectedPlaylist!.Name, Is.EqualTo(Liked));
        }

        [Test]
        public void PlayingPlaylistName_NoSuchPlaylist_NextPlaysTheSelectedOne()
        {
            var viewModel = ViewModelWith(("Aaa", ["a"]));
            Select(viewModel, "Aaa");
            viewModel.PlayingPlaylistName = "deleted since";
            var playedPath = "";
            viewModel.PlayTrackEvent += (o, e) => playedPath = e.Path;

            viewModel.PlayNextTrackHandler(null, System.EventArgs.Empty);

            Assert.That(playedPath, Is.EqualTo(PathOf("a")));
        }

        [Test]
        public void PlayingPlaylistName_FollowsARename()
        {
            var viewModel = ViewModelWith(("Aaa", ["a"]));
            var playlist = Select(viewModel, "Aaa");
            playlist.PlayTrackCommand.Execute(playlist.Playlist[0]);
            NameTheNextPlaylist("Zzz");

            viewModel.RenamePlaylistCommand.Execute(null);

            Assert.That(viewModel.PlayingPlaylistName, Is.EqualTo("Zzz"));
        }

        [Test]
        public void PlaylistsWithUnsavedOrder_AreTheOnesWithANewOrder()
        {
            var viewModel = ViewModelWith(("Aaa", ["a", "b"]), ("Bbb", ["a", "b"]), ("Ccc", ["a", "b"]));
            Assert.That(viewModel.PlaylistsWithUnsavedOrder, Is.Empty);

            var first = Select(viewModel, "Aaa");
            first.MoveTrack(first.Playlist[0], 1);
            Select(viewModel, "Bbb");
            var third = Select(viewModel, "Ccc");
            third.MoveTrack(third.Playlist[0], 1);

            Assert.That(viewModel.PlaylistsWithUnsavedOrder, Is.EqualTo(new[] { "Aaa", "Ccc" }));

            first.SaveOrderCommand.Execute(null);
            third.DiscardOrderCommand.Execute(null);

            Assert.That(viewModel.PlaylistsWithUnsavedOrder, Is.Empty);
        }

        [Test]
        public void ToggleLiked_DatabaseFails_ChangesNothing()
        {
            var viewModel = ViewModelWith();
            _context.Dispose();

            Assert.Throws<System.ObjectDisposedException>(() => viewModel.ToggleLiked(Tags("a")));

            Assert.That(viewModel.IsLiked(PathOf("a")), Is.False);
        }

        [Test]
        public void TickingAChoice_DatabaseFails_StaysUnticked()
        {
            var viewModel = ViewModelWith();
            var choice = viewModel.ChoicesFor([Tags("a")]).Choices.Single(x => x.Name == Liked);
            _context.Dispose();

            Assert.Throws<System.ObjectDisposedException>(() => choice.ContainsTracks = true);

            Assert.That(choice.ContainsTracks, Is.False);
            Assert.That(viewModel.IsLiked(PathOf("a")), Is.False);
        }

        [Test]
        public void ToggleLiked_AddsAndRemovesIt_AndTellsEveryone()
        {
            var viewModel = ViewModelWith();
            LikedChangedEventArgs? args = null;
            viewModel.LikedChanged += (o, e) => args = e;

            viewModel.ToggleLiked(Tags("a"));

            Assert.That(viewModel.IsLiked(PathOf("a")), Is.True);
            Assert.That(args!.Paths, Is.EqualTo(new[] { PathOf("a") }));
            Assert.That(args.IsLiked, Is.True);
            Assert.That(Titles(viewModel.SelectedPlaylist!), Is.EqualTo(new[] { "a" }));
            Assert.That(viewModel.SelectedPlaylist!.Playlist[0].IsLiked, Is.True);
            Assert.That(SavedTitles(Liked), Is.EqualTo(new[] { "a" }));

            viewModel.ToggleLiked(Tags("a"));

            Assert.That(viewModel.IsLiked(PathOf("a")), Is.False);
            Assert.That(args.IsLiked, Is.False);
            Assert.That(Titles(viewModel.SelectedPlaylist!), Is.Empty);
            Assert.That(SavedTitles(Liked), Is.Empty);
        }

        [Test]
        public void Liked_IsThereAfterARestart()
        {
            new PlaylistsViewModel(_context, _dialogs).ToggleLiked(Tags("a"));

            var again = new PlaylistsViewModel(_context, _dialogs);

            Assert.That(again.IsLiked(PathOf("a")), Is.True);
            Assert.That(again.IsLiked(PathOf("b")), Is.False);
        }

        [Test]
        public void HeartInAnotherPlaylist_ChangesWithTheLike()
        {
            var viewModel = ViewModelWith(("Aaa", ["a"]));
            var playlist = Select(viewModel, "Aaa");

            playlist.ToggleLikedCommand.Execute(playlist.Playlist[0]);

            Assert.That(playlist.Playlist[0].IsLiked, Is.True);
            Assert.That(Titles(playlist), Is.EqualTo(new[] { "a" }));
            Assert.That(SavedTitles(Liked), Is.EqualTo(new[] { "a" }));
        }

        [Test]
        public void ChoicesFor_MarksThePlaylistsWithTheTrack()
        {
            var viewModel = ViewModelWith(("Aaa", ["a"]), ("Bbb", ["b"]), ("Ccc", ["a", "b"]));

            var choices = viewModel.ChoicesFor([Tags("a")]).Choices;

            Assert.That(choices.Select(x => x.Name), Is.EqualTo(new[] { Liked, "Aaa", "Bbb", "Ccc" }));
            Assert.That(choices.Select(x => x.ContainsTracks), Is.EqualTo(new[] { false, true, false, true }));
        }

        [Test]
        public void TickingAChoice_AddsTheTrackToTheEnd()
        {
            var viewModel = ViewModelWith(("Aaa", ["b"]));
            var playlist = Select(viewModel, "Aaa");

            viewModel.ChoicesFor([Tags("a")]).Choices.Single(x => x.Name == "Aaa").ContainsTracks = true;

            Assert.That(Titles(playlist), Is.EqualTo(new[] { "b", "a" }));
            Assert.That(playlist.HasUnsavedOrder, Is.False);
            Assert.That(SavedTitles("Aaa"), Is.EqualTo(new[] { "b", "a" }));
        }

        [Test]
        public void TickingAChoice_PlaylistNotLoadedYet_HasTheTrackWhenItIs()
        {
            var viewModel = ViewModelWith(("Aaa", ["b"]));

            viewModel.ChoicesFor([Tags("a")]).Choices.Single(x => x.Name == "Aaa").ContainsTracks = true;

            Assert.That(Titles(Select(viewModel, "Aaa")), Is.EqualTo(new[] { "b", "a" }));
        }

        [Test]
        public void UntickingAChoice_RemovesTheTrack()
        {
            var viewModel = ViewModelWith(("Aaa", ["a", "b"]));
            var playlist = Select(viewModel, "Aaa");

            viewModel.ChoicesFor([Tags("a")]).Choices.Single(x => x.Name == "Aaa").ContainsTracks = false;

            Assert.That(Titles(playlist), Is.EqualTo(new[] { "b" }));
            Assert.That(playlist.HasUnsavedOrder, Is.False);
            Assert.That(SavedTitles("Aaa"), Is.EqualTo(new[] { "b" }));
        }

        [Test]
        public void TickingLiked_IsTheSameAsTheHeart()
        {
            var viewModel = ViewModelWith();
            var likedChanges = 0;
            viewModel.LikedChanged += (o, e) => ++likedChanges;

            viewModel.ChoicesFor([Tags("a")]).Choices.Single(x => x.Name == Liked).ContainsTracks = true;

            Assert.That(viewModel.IsLiked(PathOf("a")), Is.True);
            Assert.That(likedChanges, Is.EqualTo(1));
        }

        [Test]
        public void NewPlaylistFromTheChoices_HasTheTrackAndIsTicked()
        {
            var viewModel = ViewModelWith(("Aaa", []), ("Ccc", []));
            var choices = viewModel.ChoicesFor([Tags("a")]);
            NameTheNextPlaylist("Bbb");

            choices.NewPlaylistCommand.Execute(null);

            Assert.That(choices.Choices.Select(x => x.Name), Is.EqualTo(new[] { Liked, "Aaa", "Bbb", "Ccc" }));
            Assert.That(choices.Choices.Single(x => x.Name == "Bbb").ContainsTracks, Is.True);
            Assert.That(SavedTitles("Bbb"), Is.EqualTo(new[] { "a" }));
            Assert.That(viewModel.SelectedPlaylist!.Name, Is.EqualTo(Liked));
        }

        private QueueViewModel QueueWith(PlaylistsViewModel viewModel, params string[] titles)
        {
            var queue = new QueueViewModel(_context, _dialogs, viewModel);
            foreach (var title in titles)
            {
                queue.AddTrack(Tags(title));
            }
            return queue;
        }

        [Test]
        public void SaveQueueAsPlaylist_HasTheTracksInTheOrderOfTheQueue_AndIsSelected()
        {
            var viewModel = ViewModelWith(("Aaa", []), ("Ccc", []));
            var queue = QueueWith(viewModel, "c", "a", "b");
            NameTheNextPlaylist("Bbb");

            queue.SaveAsPlaylistCommand.Execute(null);

            Assert.That(Names(viewModel), Is.EqualTo(new[] { Liked, "Aaa", "Bbb", "Ccc" }));
            Assert.That(viewModel.SelectedPlaylist!.Name, Is.EqualTo("Bbb"));
            Assert.That(Titles(viewModel.SelectedPlaylist), Is.EqualTo(new[] { "c", "a", "b" }));
            Assert.That(viewModel.SelectedPlaylist.HasUnsavedOrder, Is.False);
            Assert.That(SavedTitles("Bbb"), Is.EqualTo(new[] { "c", "a", "b" }));
        }

        [Test]
        public void SaveQueueAsPlaylist_KeepsTheQueue()
        {
            var viewModel = ViewModelWith();
            var queue = QueueWith(viewModel, "a", "b");
            NameTheNextPlaylist("Bbb");

            queue.SaveAsPlaylistCommand.Execute(null);

            Assert.That(queue.Playlist.Select(x => x.Title), Is.EqualTo(new[] { "a", "b" }));
        }

        [Test]
        public void SaveQueueAsPlaylist_SameTrackTwice_IsInThePlaylistOnce()
        {
            var viewModel = ViewModelWith();
            var queue = QueueWith(viewModel, "a", "b", "a");
            NameTheNextPlaylist("Bbb");

            queue.SaveAsPlaylistCommand.Execute(null);

            Assert.That(SavedTitles("Bbb"), Is.EqualTo(new[] { "a", "b" }));
        }

        [Test]
        public void SaveQueueAsPlaylist_LikedTracksKeepTheirHearts()
        {
            var viewModel = ViewModelWith(("Liked", ["a"]));
            var queue = QueueWith(viewModel, "a", "b");
            NameTheNextPlaylist("Bbb");

            queue.SaveAsPlaylistCommand.Execute(null);

            Assert.That(viewModel.SelectedPlaylist!.Playlist.Select(x => x.IsLiked), Is.EqualTo(new[] { true, false }));
            Assert.That(viewModel.IsLiked(PathOf("b")), Is.False);
        }

        [Test]
        public void SaveQueueAsPlaylist_Cancelled_DoesNothing()
        {
            var viewModel = ViewModelWith();
            var queue = QueueWith(viewModel, "a");

            queue.SaveAsPlaylistCommand.Execute(null);

            Assert.That(Names(viewModel), Is.EqualTo(new[] { Liked }));
            Assert.That(viewModel.SelectedPlaylist!.Name, Is.EqualTo(Liked));
        }

        [TestCase("Aaa")]
        [TestCase("aaa")]
        [TestCase("Liked")]
        public void SaveQueueAsPlaylist_NameTaken_LeavesThatPlaylistAlone(string name)
        {
            var viewModel = ViewModelWith(("Aaa", ["c"]), ("Liked", ["c"]));
            var queue = QueueWith(viewModel, "a", "b");
            NameTheNextPlaylist(name);

            queue.SaveAsPlaylistCommand.Execute(null);

            Assert.That(Names(viewModel), Is.EqualTo(new[] { Liked, "Aaa" }));
            Assert.That(SavedTitles("Aaa"), Is.EqualTo(new[] { "c" }));
            Assert.That(SavedTitles(Liked), Is.EqualTo(new[] { "c" }));
            _dialogs.Received(1).ShowMessageAsync(Arg.Any<string>());
        }

        private static PlaylistTags OutsideTheLibrary(string title)
        {
            return new PlaylistTags(Tags(title)) { IsOutsideLibrary = true };
        }

        [Test]
        public void SaveQueueAsPlaylist_WithATrackOutsideTheLibrary_DoesNotEvenAsk()
        {
            var viewModel = ViewModelWith();
            var queue = QueueWith(viewModel, "a");
            queue.AddTrack(OutsideTheLibrary("b"));

            queue.SaveAsPlaylistCommand.Execute(null);

            _dialogs.DidNotReceive().AskForTextAsync(Arg.Any<string>(), Arg.Any<string>());
            Assert.That(Names(viewModel), Is.EqualTo(new[] { Liked }));
        }

        [Test]
        public void CanSaveAsPlaylist_NeedsTracksAndNoneOutsideTheLibrary()
        {
            var queue = QueueWith(ViewModelWith());
            Assert.That(queue.CanSaveAsPlaylist, Is.False);

            queue.AddTrack(Tags("a"));
            Assert.That(queue.CanSaveAsPlaylist, Is.True);

            queue.AddTrack(OutsideTheLibrary("b"));
            Assert.That(queue.CanSaveAsPlaylist, Is.False);

            queue.RemoveTrackCommand.Execute(queue.Playlist[1]);
            Assert.That(queue.CanSaveAsPlaylist, Is.True);
        }

        [Test]
        public void CanSaveAsPlaylist_TellsTheViewWhenItChanges()
        {
            var queue = QueueWith(ViewModelWith());
            var notifications = new System.Collections.Generic.List<string?>();
            queue.PropertyChanged += (o, e) => notifications.Add(e.PropertyName);

            queue.AddTrack(OutsideTheLibrary("a"));

            Assert.That(notifications, Does.Contain(nameof(queue.CanSaveAsPlaylist)));
        }

        [Test]
        public void TrackOutsideTheLibrary_CanNotBeLiked()
        {
            var viewModel = ViewModelWith();
            var queue = QueueWith(viewModel);
            queue.AddTrack(OutsideTheLibrary("a"));

            queue.ToggleLikedCommand.Execute(queue.Playlist[0]);

            Assert.That(viewModel.IsLiked(PathOf("a")), Is.False);
            Assert.That(new PlaylistModel(_context).TrackPaths(Liked), Is.Empty);
        }

        [Test]
        public void TrackOutsideTheLibrary_StillPlaysInTheQueue()
        {
            var queue = QueueWith(ViewModelWith());
            queue.AddTrack(OutsideTheLibrary("a"));
            PlayTrackEventArgs? args = null;
            queue.PlayTrackEvent += (o, e) => args = e;

            queue.PlayTrackCommand.Execute(queue.Playlist[0]);

            Assert.That(args!.Path, Is.EqualTo(PathOf("a")));
        }

        [Test]
        public void ChoicesFor_SeveralTracks_TicksOnlyThePlaylistsWithAllOfThem()
        {
            var viewModel = ViewModelWith(("Aaa", ["a"]), ("Bbb", ["b"]), ("Ccc", ["a", "b"]));

            var choices = viewModel.ChoicesFor([Tags("a"), Tags("b")]).Choices;

            Assert.That(choices.Select(x => x.Name), Is.EqualTo(new[] { Liked, "Aaa", "Bbb", "Ccc" }));
            Assert.That(choices.Select(x => x.ContainsTracks), Is.EqualTo(new[] { false, false, false, true }));
        }

        [Test]
        public void TickingAChoice_SeveralTracks_AddsOnlyTheOnesItDoesNotHave()
        {
            var viewModel = ViewModelWith(("Aaa", ["b"]));
            var playlist = Select(viewModel, "Aaa");
            var choice = viewModel.ChoicesFor([Tags("a"), Tags("b"), Tags("c")]).Choices.Single(x => x.Name == "Aaa");

            choice.ContainsTracks = true;

            Assert.That(choice.ContainsTracks, Is.True);
            Assert.That(Titles(playlist), Is.EqualTo(new[] { "b", "a", "c" }));
            Assert.That(playlist.HasUnsavedOrder, Is.False);
            Assert.That(SavedTitles("Aaa"), Is.EqualTo(new[] { "b", "a", "c" }));
        }

        [Test]
        public void TickingAChoice_SeveralTracks_PlaylistNotLoadedYet_HasThemWhenItIs()
        {
            var viewModel = ViewModelWith(("Aaa", ["b"]));

            viewModel.ChoicesFor([Tags("a"), Tags("b")]).Choices.Single(x => x.Name == "Aaa").ContainsTracks = true;

            Assert.That(Titles(Select(viewModel, "Aaa")), Is.EqualTo(new[] { "b", "a" }));
        }

        [Test]
        public void UntickingAChoice_SeveralTracks_RemovesThemAll()
        {
            var viewModel = ViewModelWith(("Aaa", ["a", "b", "c"]));
            var playlist = Select(viewModel, "Aaa");

            viewModel.ChoicesFor([Tags("a"), Tags("c")]).Choices.Single(x => x.Name == "Aaa").ContainsTracks = false;

            Assert.That(Titles(playlist), Is.EqualTo(new[] { "b" }));
            Assert.That(SavedTitles("Aaa"), Is.EqualTo(new[] { "b" }));
        }

        [Test]
        public void UntickingAChoice_SeveralTracksWithThePlayingOne_NextGoesOnFromItsPlace()
        {
            var viewModel = ViewModelWith(("Aaa", ["a", "b", "c"]));
            var playlist = Select(viewModel, "Aaa");
            playlist.PlayTrackCommand.Execute(playlist.Playlist[1]);
            var playedPath = "";
            viewModel.PlayTrackEvent += (o, e) => playedPath = e.Path;

            viewModel.ChoicesFor([Tags("a"), Tags("b")]).Choices.Single(x => x.Name == "Aaa").ContainsTracks = false;
            viewModel.PlayNextTrackHandler(null, System.EventArgs.Empty);

            Assert.That(playedPath, Is.EqualTo(PathOf("c")));
        }

        [Test]
        public void TickingLiked_SeveralTracks_LikesThemAllInOneGo()
        {
            var viewModel = ViewModelWith(("Aaa", ["a", "b"]));
            var playlist = Select(viewModel, "Aaa");
            var likedChanges = 0;
            viewModel.LikedChanged += (o, e) => ++likedChanges;

            viewModel.ChoicesFor([Tags("a"), Tags("b")]).Choices.Single(x => x.Name == Liked).ContainsTracks = true;

            Assert.That(likedChanges, Is.EqualTo(1));
            Assert.That(playlist.Playlist.Select(x => x.IsLiked), Is.All.True);
            Assert.That(SavedTitles(Liked), Is.EqualTo(new[] { "a", "b" }));
        }

        [Test]
        public void NewPlaylistFromTheChoices_SeveralTracks_HasThemAllAndIsTicked()
        {
            var viewModel = ViewModelWith();
            var choices = viewModel.ChoicesFor([Tags("a"), Tags("b")]);
            NameTheNextPlaylist("Bbb");

            choices.NewPlaylistCommand.Execute(null);

            Assert.That(choices.Choices.Single(x => x.Name == "Bbb").ContainsTracks, Is.True);
            Assert.That(SavedTitles("Bbb"), Is.EqualTo(new[] { "a", "b" }));
        }

        [Test]
        public void SaveQueueAsPlaylist_EmptyQueue_DoesNotEvenAsk()
        {
            var viewModel = ViewModelWith();
            var queue = QueueWith(viewModel);

            queue.SaveAsPlaylistCommand.Execute(null);

            _dialogs.DidNotReceive().AskForTextAsync(Arg.Any<string>(), Arg.Any<string>());
        }

        [Test]
        public void HasTracks_TellsTheViewWhenItChanges()
        {
            var queue = QueueWith(ViewModelWith());
            var notifications = new System.Collections.Generic.List<string?>();
            queue.PropertyChanged += (o, e) => notifications.Add(e.PropertyName);

            queue.AddTrack(Tags("a"));
            Assert.That(notifications, Does.Contain(nameof(queue.HasTracks)));

            notifications.Clear();
            queue.RemoveAllTracksCommand.Execute(null);
            Assert.That(notifications, Does.Contain(nameof(queue.HasTracks)));
            Assert.That(queue.HasTracks, Is.False);
        }

        [Test]
        public void RemoveTrack_RemovesItFromThePlaylistForGood()
        {
            var viewModel = ViewModelWith(("Aaa", ["a", "b"]));
            var playlist = Select(viewModel, "Aaa");

            playlist.RemoveTrackCommand.Execute(playlist.Playlist[0]);

            Assert.That(Titles(playlist), Is.EqualTo(new[] { "b" }));
            Assert.That(SavedTitles("Aaa"), Is.EqualTo(new[] { "b" }));
        }

        [Test]
        public void RemoveTrack_FromLiked_TakesAwayTheHeart()
        {
            var viewModel = ViewModelWith(("Liked", ["a", "b"]));
            var liked = viewModel.SelectedPlaylist!;

            liked.RemoveTrackCommand.Execute(liked.Playlist[0]);

            Assert.That(viewModel.IsLiked(PathOf("a")), Is.False);
        }

        // Taking away the heart of what is playing takes it out of Liked. That's no reason to start over from the top.
        [Test]
        public void Unliking_ThePlayingTrackInLiked_NextPlaysTheOneAfterIt()
        {
            var viewModel = ViewModelWith(("Liked", ["a", "b", "c"]));
            var liked = viewModel.SelectedPlaylist!;
            liked.PlayTrackCommand.Execute(liked.Playlist[1]);
            var playedPath = "";
            viewModel.PlayTrackEvent += (o, e) => playedPath = e.Path;

            liked.ToggleLikedCommand.Execute(liked.Playlist[1]);
            viewModel.PlayNextTrackHandler(null, System.EventArgs.Empty);

            Assert.That(playedPath, Is.EqualTo(PathOf("c")));
        }

        [Test]
        public void MovingATrack_MakesTheOrderUnsaved_UntilItIsBack()
        {
            var viewModel = ViewModelWith(("Aaa", ["a", "b", "c"]));
            var playlist = Select(viewModel, "Aaa");
            Assert.That(playlist.HasUnsavedOrder, Is.False);

            var track = playlist.Playlist[0];
            playlist.MoveTrack(track, 2);
            Assert.That(playlist.HasUnsavedOrder, Is.True);

            playlist.MoveTrack(track, 0);
            Assert.That(playlist.HasUnsavedOrder, Is.False);
        }

        [Test]
        public void MovingATrack_TellsTheViewAboutTheButtons()
        {
            var viewModel = ViewModelWith(("Aaa", ["a", "b"]));
            var playlist = Select(viewModel, "Aaa");
            var notifications = new System.Collections.Generic.List<string?>();
            playlist.PropertyChanged += (o, e) => notifications.Add(e.PropertyName);

            playlist.MoveTrack(playlist.Playlist[0], 1);

            Assert.That(notifications, Does.Contain(nameof(playlist.HasUnsavedOrder)));
        }

        [Test]
        public void Sorting_MakesTheOrderUnsaved()
        {
            var viewModel = ViewModelWith(("Aaa", ["b", "a"]));
            var playlist = Select(viewModel, "Aaa");

            playlist.SortCommand.Execute(new SortMessage(ListSortDirection.Ascending, new PlaylistColumn("Title")));

            Assert.That(Titles(playlist), Is.EqualTo(new[] { "a", "b" }));
            Assert.That(playlist.HasUnsavedOrder, Is.True);
        }

        [Test]
        public void SaveOrder_KeepsTheNewOrder()
        {
            var viewModel = ViewModelWith(("Aaa", ["a", "b", "c"]));
            var playlist = Select(viewModel, "Aaa");
            playlist.MoveTrack(playlist.Playlist[0], 2);

            playlist.SaveOrderCommand.Execute(null);

            Assert.That(playlist.HasUnsavedOrder, Is.False);
            Assert.That(SavedTitles("Aaa"), Is.EqualTo(new[] { "b", "c", "a" }));
        }

        [Test]
        public void DiscardOrder_GoesBackToTheSavedOrder()
        {
            var viewModel = ViewModelWith(("Aaa", ["a", "b", "c"]));
            var playlist = Select(viewModel, "Aaa");
            playlist.MoveTrack(playlist.Playlist[0], 2);

            playlist.DiscardOrderCommand.Execute(null);

            Assert.That(playlist.HasUnsavedOrder, Is.False);
            Assert.That(Titles(playlist), Is.EqualTo(new[] { "a", "b", "c" }));
            Assert.That(SavedTitles("Aaa"), Is.EqualTo(new[] { "a", "b", "c" }));
        }

        [Test]
        public void NewOrder_NotSaved_IsStillThereAfterLookingAtAnotherPlaylist()
        {
            var viewModel = ViewModelWith(("Aaa", ["a", "b"]), ("Bbb", ["c"]));
            var playlist = Select(viewModel, "Aaa");
            playlist.MoveTrack(playlist.Playlist[0], 1);

            Select(viewModel, "Bbb");
            Select(viewModel, "Aaa");

            Assert.That(Titles(playlist), Is.EqualTo(new[] { "b", "a" }));
            Assert.That(playlist.HasUnsavedOrder, Is.True);
        }

        [Test]
        public void TickingAChoice_WhileTheOrderIsNotSaved_KeepsItUnsaved()
        {
            var viewModel = ViewModelWith(("Aaa", ["a", "b"]));
            var playlist = Select(viewModel, "Aaa");
            playlist.MoveTrack(playlist.Playlist[0], 1);

            viewModel.ChoicesFor([Tags("c")]).Choices.Single(x => x.Name == "Aaa").ContainsTracks = true;

            Assert.That(Titles(playlist), Is.EqualTo(new[] { "b", "a", "c" }));
            Assert.That(playlist.HasUnsavedOrder, Is.True);

            playlist.DiscardOrderCommand.Execute(null);
            Assert.That(Titles(playlist), Is.EqualTo(new[] { "a", "b", "c" }));
        }

        [Test]
        public void RemovingATrack_WhileTheOrderIsNotSaved_DiscardDoesNotBringItBack()
        {
            var viewModel = ViewModelWith(("Aaa", ["a", "b", "c"]));
            var playlist = Select(viewModel, "Aaa");
            playlist.MoveTrack(playlist.Playlist[0], 2);

            playlist.RemoveTrackCommand.Execute(playlist.Playlist[0]);
            playlist.DiscardOrderCommand.Execute(null);

            Assert.That(Titles(playlist), Is.EqualTo(new[] { "a", "c" }));
        }

        [Test]
        public void PlayTrack_TellsThePlayerItComesFromThePlaylists()
        {
            var viewModel = ViewModelWith(("Aaa", ["a"]));
            var playlist = Select(viewModel, "Aaa");
            PlayTrackEventArgs? args = null;
            viewModel.PlayTrackEvent += (o, e) => args = e;

            playlist.PlayTrackCommand.Execute(playlist.Playlist[0]);

            Assert.That(args!.Path, Is.EqualTo(PathOf("a")));
            Assert.That(args.ListName, Is.EqualTo(PlaylistsViewModel.ListName));
        }

        [Test]
        public void PlayNext_WhileLookingAtAnotherPlaylist_StaysInTheOneThatIsPlaying()
        {
            var viewModel = ViewModelWith(("Aaa", ["a", "b"]), ("Bbb", ["c"]));
            var playlist = Select(viewModel, "Aaa");
            playlist.PlayTrackCommand.Execute(playlist.Playlist[0]);
            Select(viewModel, "Bbb");
            var playedPath = "";
            viewModel.PlayTrackEvent += (o, e) => playedPath = e.Path;

            viewModel.PlayNextTrackHandler(null, System.EventArgs.Empty);

            Assert.That(playedPath, Is.EqualTo(PathOf("b")));
        }

        [Test]
        public void PlayNext_NothingPlayedYet_PlaysTheSelectedPlaylist()
        {
            var viewModel = ViewModelWith(("Aaa", ["a"]), ("Bbb", ["c"]));
            Select(viewModel, "Bbb");
            var playedPath = "";
            viewModel.PlayTrackEvent += (o, e) => playedPath = e.Path;

            viewModel.PlayNextTrackHandler(null, System.EventArgs.Empty);

            Assert.That(playedPath, Is.EqualTo(PathOf("c")));
        }

        [Test]
        public void PlayNext_FollowsTheNewOrder_AlsoBeforeItIsSaved()
        {
            var viewModel = ViewModelWith(("Aaa", ["a", "b", "c"]));
            var playlist = Select(viewModel, "Aaa");
            playlist.PlayTrackCommand.Execute(playlist.Playlist[0]);
            playlist.MoveTrack(playlist.Playlist[2], 1);
            var playedPath = "";
            viewModel.PlayTrackEvent += (o, e) => playedPath = e.Path;

            viewModel.PlayNextTrackHandler(null, System.EventArgs.Empty);

            Assert.That(playedPath, Is.EqualTo(PathOf("c")));
        }

        [Test]
        public void PlayingFromAnotherPlaylist_OnlyThatOneShowsAsPlaying()
        {
            var viewModel = ViewModelWith(("Aaa", ["a"]), ("Bbb", ["c"]));
            var first = Select(viewModel, "Aaa");
            first.PlayTrackCommand.Execute(first.Playlist[0]);
            var second = Select(viewModel, "Bbb");

            second.PlayTrackCommand.Execute(second.Playlist[0]);

            Assert.That(first.CurrentIndex, Is.EqualTo(-1));
            Assert.That(second.CurrentIndex, Is.EqualTo(0));
        }

        [Test]
        public void ShuffleRepeat_AppliesToAllPlaylists_AlsoNewOnes()
        {
            var viewModel = ViewModelWith(("Aaa", ["a", "b"]));
            viewModel.ShuffleRepeatChangedHandler(null, new ChangeShuffleRepeatEventArgs(ShuffleRepeatState.RepeatOne));
            NameTheNextPlaylist("Bbb");

            viewModel.AddPlaylistCommand.Execute(null);

            Assert.That(viewModel.Playlists.Select(x => x.PlaylistShuffleRepeatState), Is.All.EqualTo(ShuffleRepeatState.RepeatOne));
        }

        [Test]
        public void AddSelectionToQueue_GoesThroughThePlaylists()
        {
            var viewModel = ViewModelWith(("Aaa", ["a"]));
            var playlist = Select(viewModel, "Aaa");
            AddTracksToQueueEventArgs? args = null;
            viewModel.AddTracksToQueueEvent += (o, e) => args = e;

            playlist.AddSelectionToQueueCommand.Execute(new[] { playlist.Playlist[0] });

            Assert.That(args!.Tracks.Select(x => x.Title), Is.EqualTo(new[] { "a" }));
            Assert.That(args.ReplaceQueue, Is.False);
        }

        [Test]
        public void TrackAnalyzed_ShowsWhatWasFound_InEveryPlaylistThatHasTheTrack()
        {
            var viewModel = ViewModelWith(("Aaa", ["a", "b"]), ("Bbb", ["b"]));
            var first = Select(viewModel, "Aaa");
            var second = Select(viewModel, "Bbb");

            viewModel.TrackAnalyzedHandler(null, new TrackAnalyzedEventArgs(PathOf("b"), "8A", 128));

            Assert.That(first.Playlist.Select(x => x.InitialKey), Is.EqualTo(new[] { "", "8A" }));
            Assert.That(second.Playlist.Select(x => x.BeatsPerMinuteString), Is.EqualTo(new[] { "128" }));
            Assert.That(first.HasUnsavedOrder, Is.False);
        }

        [Test]
        public void ScanFinished_ShowsTheNewTagsAndKeepsThePlayingTrack()
        {
            var viewModel = ViewModelWith(("Aaa", ["a", "b"]));
            var playlist = Select(viewModel, "Aaa");
            playlist.PlayTrackCommand.Execute(playlist.Playlist[1]);
            _context.Set<Track>().Single(x => x.Title == "a").Title = "new title";
            _context.SaveChanges();

            viewModel.ScanningChangedHandler(null, new ScanningChangedEventArgs(false));

            Assert.That(Titles(playlist), Is.EqualTo(new[] { "new title", "b" }));
            Assert.That(playlist.CurrentIndex, Is.EqualTo(1));
        }

        [Test]
        public void ScanDuringATransition_TheTrackMixedInIsStillTheListedOne()
        {
            var viewModel = ViewModelWith(("Aaa", ["a", "b", "c"]));
            var playlist = Select(viewModel, "Aaa");
            playlist.PlayTrackCommand.Execute(playlist.Playlist[0]);
            var next = new NextTrackEventArgs();
            viewModel.PeekNextTrackHandler(null, next);
            viewModel.ScanningChangedHandler(null, new ScanningChangedEventArgs(false));

            playlist.WentOnTo(next.Track!);

            Assert.That(playlist.CurrentIndex, Is.EqualTo(1));
            Assert.That(playlist.NextTrack?.Path, Is.EqualTo(PathOf("c")));
        }

        private void RemoveFromLibrary(string title)
        {
            _context.Remove(_context.Set<Track>().Single(x => x.Title == title));
            _context.SaveChanges();
        }

        private void AddToLibrary(string title)
        {
            _context.Add(new Track { Title = title, Path = PathOf(title), Length = 100, Album = new Album { Name = title }, TrackNumber = 1, DiscNumber = 1 });
            _context.SaveChanges();
        }

        [Test]
        public void ScanFinished_KeepsANewOrderThatIsNotSaved()
        {
            var viewModel = ViewModelWith(("Aaa", ["a", "b", "c"]));
            var playlist = Select(viewModel, "Aaa");
            playlist.MoveTrack(playlist.Playlist[0], 2);
            _context.Set<Track>().Single(x => x.Title == "a").Title = "new title";
            _context.SaveChanges();

            viewModel.ScanningChangedHandler(null, new ScanningChangedEventArgs(false));

            Assert.That(Titles(playlist), Is.EqualTo(new[] { "b", "c", "new title" }));
            Assert.That(playlist.HasUnsavedOrder, Is.True);

            playlist.DiscardOrderCommand.Execute(null);
            Assert.That(Titles(playlist), Is.EqualTo(new[] { "new title", "b", "c" }));
        }

        [Test]
        public void ScanFinished_NewOrderAndATrackThatIsBack_ThatOneGoesToTheEnd()
        {
            var viewModel = ViewModelWith(("Aaa", ["a", "b", "c"]));
            RemoveFromLibrary("b");
            var playlist = Select(viewModel, "Aaa");
            playlist.MoveTrack(playlist.Playlist[0], 1);
            AddToLibrary("b");

            viewModel.ScanningChangedHandler(null, new ScanningChangedEventArgs(false));

            Assert.That(Titles(playlist), Is.EqualTo(new[] { "c", "a", "b" }));
            Assert.That(playlist.HasUnsavedOrder, Is.True);
        }

        [Test]
        public void ScanFinished_NoNewOrderAndATrackThatIsBack_ThatOneIsInItsPlace()
        {
            var viewModel = ViewModelWith(("Aaa", ["a", "b", "c"]));
            RemoveFromLibrary("b");
            var playlist = Select(viewModel, "Aaa");
            AddToLibrary("b");

            viewModel.ScanningChangedHandler(null, new ScanningChangedEventArgs(false));

            Assert.That(Titles(playlist), Is.EqualTo(new[] { "a", "b", "c" }));
            Assert.That(playlist.HasUnsavedOrder, Is.False);
        }

        [Test]
        public void ScanFinished_PlayingTrackGone_NextPlaysTheOneAfterIt()
        {
            var viewModel = ViewModelWith(("Aaa", ["a", "b", "c"]));
            var playlist = Select(viewModel, "Aaa");
            playlist.PlayTrackCommand.Execute(playlist.Playlist[1]);
            RemoveFromLibrary("b");
            viewModel.ScanningChangedHandler(null, new ScanningChangedEventArgs(false));
            var playedPath = "";
            viewModel.PlayTrackEvent += (o, e) => playedPath = e.Path;

            viewModel.PlayNextTrackHandler(null, System.EventArgs.Empty);

            Assert.That(playedPath, Is.EqualTo(PathOf("c")));
        }

        [Test]
        public void ScanFinished_PlayingTrackAndOneBeforeItGone_PreviousPlaysTheOneBefore()
        {
            var viewModel = ViewModelWith(("Aaa", ["a", "b", "c"]));
            var playlist = Select(viewModel, "Aaa");
            playlist.PlayTrackCommand.Execute(playlist.Playlist[2]);
            RemoveFromLibrary("b");
            RemoveFromLibrary("c");
            viewModel.ScanningChangedHandler(null, new ScanningChangedEventArgs(false));
            var playedPath = "";
            viewModel.PlayTrackEvent += (o, e) => playedPath = e.Path;

            viewModel.PlayLastTrackHandler(null, System.EventArgs.Empty);

            Assert.That(playedPath, Is.EqualTo(PathOf("a")));
        }

        [Test]
        public void ScanFinished_PlayingTrackAlreadyRemoved_NextStillGoesOnFromThere()
        {
            var viewModel = ViewModelWith(("Aaa", ["a", "b", "c"]));
            var playlist = Select(viewModel, "Aaa");
            playlist.PlayTrackCommand.Execute(playlist.Playlist[1]);
            playlist.RemoveTrackCommand.Execute(playlist.Playlist[1]);
            viewModel.ScanningChangedHandler(null, new ScanningChangedEventArgs(false));
            var playedPath = "";
            viewModel.PlayTrackEvent += (o, e) => playedPath = e.Path;

            viewModel.PlayNextTrackHandler(null, System.EventArgs.Empty);

            Assert.That(playedPath, Is.EqualTo(PathOf("c")));
        }

        [Test]
        public void DiscardOrder_AfterThePlayingTrackGotRemoved_NextStartsFromTheTop()
        {
            var viewModel = ViewModelWith(("Aaa", ["a", "b", "c"]));
            var playlist = Select(viewModel, "Aaa");
            playlist.PlayTrackCommand.Execute(playlist.Playlist[2]);
            playlist.MoveTrack(playlist.Playlist[0], 1);
            playlist.RemoveTrackCommand.Execute(playlist.Playlist[2]);
            playlist.DiscardOrderCommand.Execute(null);
            var playedPath = "";
            viewModel.PlayTrackEvent += (o, e) => playedPath = e.Path;

            viewModel.PlayNextTrackHandler(null, System.EventArgs.Empty);

            Assert.That(playedPath, Is.EqualTo(PathOf("a")));
        }

        [Test]
        public void ScanStarted_ChangesNothing()
        {
            var viewModel = ViewModelWith(("Aaa", ["a"]));
            var playlist = Select(viewModel, "Aaa");
            var track = playlist.Playlist[0];

            viewModel.ScanningChangedHandler(null, new ScanningChangedEventArgs(true));

            Assert.That(playlist.Playlist[0], Is.SameAs(track));
        }
    }
}
