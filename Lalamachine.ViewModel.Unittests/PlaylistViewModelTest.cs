using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using LibLala.DomainPrimitives;
using LibLala.LibLalaTagReader;
using NSubstitute;
using NUnit.Framework;

namespace Lalamachine.ViewModel.Unittests
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "<Pending>")]
    public class PlaylistViewModelTest
    {
        [Test]
        public void PlayNextTrackHandler_NoTracks_DoesNothing()
        {
            var wasCalled = false;
            var viewModel = new PlaylistBaseViewModel("LIBRARY", Substitute.For<ITrackPlaylists>());
            viewModel.PlayTrackEvent += (o, e) => wasCalled = true;
            viewModel.PlayNextTrackHandler(null, new System.EventArgs());
            Assert.That(!wasCalled);
        }

        [Test]
        public void PlayNextTrackHandler_HasTracks_PlayNext()
        {
            var wasCalled = false;
            var viewModel = new PlaylistBaseViewModel("LIBRARY", Substitute.For<ITrackPlaylists>());
            viewModel.AddTrack(new LibLalaTagsBuilder("bob", new UncheckedTrackPath("/path/to/bob"), 5).Build());
            viewModel.PlayTrackEvent += (o, e) => wasCalled = true;
            viewModel.PlayNextTrackHandler(null, new System.EventArgs());
            Assert.That(wasCalled);
        }

        [Test]
        public void PeekNextTrackHandler_SaysWhatsNext_AndDoesntPlayIt()
        {
            var wasCalled = false;
            var viewModel = new PlaylistBaseViewModel("LIBRARY", Substitute.For<ITrackPlaylists>());
            viewModel.AddTrack(new LibLalaTagsBuilder("bob", new UncheckedTrackPath("/path/to/bob"), 5).Build());
            viewModel.PlayTrackEvent += (o, e) => wasCalled = true;
            var request = new NextTrackEventArgs();

            viewModel.PeekNextTrackHandler(null, request);

            Assert.That(request.Track?.Path, Is.EqualTo("/path/to/bob"));
            Assert.That(request.List, Is.SameAs(viewModel));
            Assert.That(!wasCalled);
            Assert.That(viewModel.CurrentIndex, Is.EqualTo(-1));
        }

        [Test]
        public void PlayPreviousTrackHandler_NoTracks_DoesNothing()
        {
            var wasCalled = false;
            var viewModel = new PlaylistBaseViewModel("LIBRARY", Substitute.For<ITrackPlaylists>());
            viewModel.PlayTrackEvent += (o, e) => wasCalled = true;
            viewModel.PlayLastTrackHandler(null, new System.EventArgs());
            Assert.That(!wasCalled);
        }

        [Test]
        public void PlayPreviousTrackHandler_HasTracks_PlayPrevious()
        {
            var wasCalled = false;
            var viewModel = new PlaylistBaseViewModel("LIBRARY", Substitute.For<ITrackPlaylists>());
            viewModel.AddTrack(new LibLalaTagsBuilder("bob", new UncheckedTrackPath("/path/to/bob"), 5).Build());
            viewModel.PlayTrackEvent += (o, e) => wasCalled = true;
            viewModel.PlayLastTrackHandler(null, new System.EventArgs());
            Assert.That(wasCalled);
        }

        [Test]
        public void RemoveTrack_NotifiesNumberOfTracksAndTotalPlaytime()
        {
            var viewModel = ViewModelWithTracks(2, ShuffleRepeatState.None);
            var notifications = new System.Collections.Generic.List<string?>();
            viewModel.PropertyChanged += (o, e) => notifications.Add(e.PropertyName);

            viewModel.RemoveTrackCommand.Execute(viewModel.Playlist[0]);

            Assert.That(viewModel.NumberOfTracks, Is.EqualTo(1));
            Assert.That(notifications, Does.Contain(nameof(viewModel.NumberOfTracks)));
            Assert.That(notifications, Does.Contain(nameof(viewModel.TotalPlaytime)));
        }

        [Test]
        public void TrackAnalyzed_ShowsWhatWasFound_WhereverTheTrackIsListed()
        {
            // The queue can have a track twice.
            var viewModel = ViewModelWithKeys("", "");
            viewModel.AddTrack(new LibLalaTagsBuilder("alice", new UncheckedTrackPath("/path/to/alice"), 5).Build());

            viewModel.TrackAnalyzedHandler(null, new TrackAnalyzedEventArgs("/path/to/bob", "8A", 128));

            Assert.That(viewModel.Playlist.Select(x => x.InitialKey), Is.EqualTo(new[] { "8A", "8A", "" }));
            Assert.That(viewModel.Playlist.Select(x => x.BeatsPerMinuteString), Is.EqualTo(new[] { "128", "128", "" }));
        }

        [Test]
        public void TrackAnalyzed_NothingFound_TakesAwayWhatWasFoundBefore()
        {
            // The file changed, and what was a track with a beat and a key is a recording of rain now.
            var viewModel = ViewModelWithKeys("");
            viewModel.TrackAnalyzedHandler(null, new TrackAnalyzedEventArgs("/path/to/bob", "8A", 128));

            viewModel.TrackAnalyzedHandler(null, new TrackAnalyzedEventArgs("/path/to/bob", "", 0));

            Assert.That((viewModel.Playlist[0].InitialKey, viewModel.Playlist[0].BeatsPerMinuteString), Is.EqualTo(("", "")));
        }

        private static PlaylistBaseViewModel ViewModelWithTitles(params string[] titles)
        {
            var viewModel = new PlaylistBaseViewModel("MAIN", Substitute.For<ITrackPlaylists>());
            foreach (var title in titles)
            {
                viewModel.AddTrack(new LibLalaTagsBuilder(title, new UncheckedTrackPath($"/path/to/{title}"), 5).Build());
            }
            return viewModel;
        }

        private static IEnumerable<string> Titles(PlaylistBaseViewModel viewModel) => viewModel.Playlist.Select(x => x.Title);

        private static SortMessage ByTitle(ListSortDirection direction) => new SortMessage(direction, new PlaylistColumn("Title"));

        [Test]
        public void Unsort_HasTheOrderBackFromBeforeTheSort()
        {
            var viewModel = ViewModelWithTitles("b", "c", "a");
            viewModel.SortCommand.Execute(ByTitle(ListSortDirection.Ascending));
            viewModel.SortCommand.Execute(ByTitle(ListSortDirection.Descending));
            Assert.That(Titles(viewModel), Is.EqualTo(new[] { "c", "b", "a" }));

            viewModel.UnsortCommand.Execute(null);

            Assert.That(Titles(viewModel), Is.EqualTo(new[] { "b", "c", "a" }));
            Assert.That(viewModel.CurrentSort, Is.Null);
        }

        [Test]
        public void Unsort_AfterSortingByTwoColumns_GoesBackToBeforeTheFirst()
        {
            var viewModel = ViewModelWithTitles("b", "c", "a");
            viewModel.SortCommand.Execute(ByTitle(ListSortDirection.Ascending));
            viewModel.SortCommand.Execute(new SortMessage(ListSortDirection.Descending, new PlaylistColumn("Length")));

            viewModel.UnsortCommand.Execute(null);

            Assert.That(Titles(viewModel), Is.EqualTo(new[] { "b", "c", "a" }));
        }

        [Test]
        public void Unsort_TracksAddedSinceTheSort_ComeLast_AndRemovedOnesStayGone()
        {
            var viewModel = ViewModelWithTitles("b", "c", "a");
            viewModel.SortCommand.Execute(ByTitle(ListSortDirection.Ascending));
            viewModel.AddTrack(new LibLalaTagsBuilder("d", new UncheckedTrackPath("/path/to/d"), 5).Build());
            viewModel.RemoveTrackCommand.Execute(viewModel.Playlist.Single(x => x.Title == "c"));

            viewModel.UnsortCommand.Execute(null);

            Assert.That(Titles(viewModel), Is.EqualTo(new[] { "b", "a", "d" }));
        }

        [Test]
        public void Unsort_ACopyAddedSinceTheSort_ComesLast_NotNextToTheFirst()
        {
            var viewModel = ViewModelWithTitles("a", "b", "c");
            viewModel.SortCommand.Execute(ByTitle(ListSortDirection.Descending));
            viewModel.AddTrack(new LibLalaTagsBuilder("a", new UncheckedTrackPath("/path/to/a"), 5).Build());
            viewModel.AddTrack(new LibLalaTagsBuilder("d", new UncheckedTrackPath("/path/to/d"), 5).Build());
            var copy = viewModel.Playlist[3];

            viewModel.UnsortCommand.Execute(null);

            Assert.That(Titles(viewModel), Is.EqualTo(new[] { "a", "b", "c", "a", "d" }));
            Assert.That(viewModel.Playlist[3], Is.SameAs(copy));
        }

        [Test]
        public void AddTrack_ToASortedList_ItIsNotSortedAnyMore_ButCanBeSortedAndUnsortedAgain()
        {
            var viewModel = ViewModelWithTitles("b", "c", "a");
            viewModel.SortCommand.Execute(ByTitle(ListSortDirection.Ascending));
            var notifications = new List<string?>();
            viewModel.PropertyChanged += (o, e) => notifications.Add(e.PropertyName);

            viewModel.AddTrack(new LibLalaTagsBuilder("aa", new UncheckedTrackPath("/path/to/aa"), 5).Build());
            Assert.That(viewModel.CurrentSort, Is.Null);
            Assert.That(notifications, Does.Contain(nameof(viewModel.CurrentSort)));
            Assert.That(Titles(viewModel), Is.EqualTo(new[] { "a", "b", "c", "aa" }));

            viewModel.SortCommand.Execute(ByTitle(ListSortDirection.Ascending));
            Assert.That(Titles(viewModel), Is.EqualTo(new[] { "a", "aa", "b", "c" }));

            viewModel.UnsortCommand.Execute(null);
            Assert.That(Titles(viewModel), Is.EqualTo(new[] { "b", "c", "a", "aa" }));
        }

        [Test]
        public void Unsort_TheSameTrackTwice_EachGoesBackToItsPlace()
        {
            var viewModel = ViewModelWithTitles("b", "a", "b");
            var first = viewModel.Playlist[0];
            viewModel.SortCommand.Execute(ByTitle(ListSortDirection.Descending));

            viewModel.UnsortCommand.Execute(null);

            Assert.That(Titles(viewModel), Is.EqualTo(new[] { "b", "a", "b" }));
            Assert.That(viewModel.Playlist[0], Is.SameAs(first));
        }

        [Test]
        public void Sort_SaysWhatItIsSortedBy()
        {
            var viewModel = ViewModelWithTitles("b", "a");
            var notifications = new List<string?>();
            viewModel.PropertyChanged += (o, e) => notifications.Add(e.PropertyName);

            viewModel.SortCommand.Execute(ByTitle(ListSortDirection.Descending));

            Assert.That((viewModel.CurrentSort?.Column.Name, viewModel.CurrentSort?.Direction), Is.EqualTo(("Title", ListSortDirection.Descending)));
            Assert.That(notifications, Does.Contain(nameof(viewModel.CurrentSort)));
        }

        [Test]
        public void MoveTrack_ItIsNotSortedAnyMore_AndUnsortChangesNothing()
        {
            var viewModel = ViewModelWithTitles("b", "c", "a");
            viewModel.SortCommand.Execute(ByTitle(ListSortDirection.Ascending));

            viewModel.MoveTrack(viewModel.Playlist[0], 2);
            viewModel.UnsortCommand.Execute(null);

            Assert.That(viewModel.CurrentSort, Is.Null);
            Assert.That(Titles(viewModel), Is.EqualTo(new[] { "b", "c", "a" }));
        }

        [Test]
        public void Unsort_NotSorted_ChangesNothing()
        {
            var viewModel = ViewModelWithTitles("b", "a");
            var notifications = new List<string?>();
            viewModel.PropertyChanged += (o, e) => notifications.Add(e.PropertyName);

            viewModel.UnsortCommand.Execute(null);

            Assert.That(Titles(viewModel), Is.EqualTo(new[] { "b", "a" }));
            Assert.That(notifications, Is.Empty);
        }

        [Test]
        public void Unsort_NextPlaysWhatComesAfterThePlayingTrackInTheOrderBefore()
        {
            var viewModel = ViewModelWithTitles("b", "c", "a");
            viewModel.SortCommand.Execute(ByTitle(ListSortDirection.Ascending));
            viewModel.PlayTrackCommand.Execute(viewModel.Playlist.Single(x => x.Title == "b"));

            viewModel.UnsortCommand.Execute(null);

            Assert.That(viewModel.NextTrack?.Title, Is.EqualTo("c"));
        }

        private static PlaylistBaseViewModel ViewModelWithKeys(params string[] keys)
        {
            var viewModel = new PlaylistBaseViewModel("MAIN", Substitute.For<ITrackPlaylists>());
            foreach (var key in keys)
            {
                viewModel.AddTrack(new LibLalaTagsBuilder("bob", new UncheckedTrackPath("/path/to/bob"), 5).WithInitialKey(key).Build());
            }
            return viewModel;
        }

        private static PlaylistBaseViewModel ViewModelWithBeatsPerMinute(params uint[] values)
        {
            var viewModel = new PlaylistBaseViewModel("MAIN", Substitute.For<ITrackPlaylists>());
            foreach (var value in values)
            {
                viewModel.AddTrack(new LibLalaTagsBuilder("bob", new UncheckedTrackPath("/path/to/bob"), 5).WithBeatsPerMinute(value).Build());
            }
            return viewModel;
        }

        [Test]
        public void SortByKey_Ascending_SortsByNumberThenLetter()
        {
            var viewModel = ViewModelWithKeys("10A", "4B", "1A", "11B", "3A", "4A", "2A");

            viewModel.SortCommand.Execute(new SortMessage(ListSortDirection.Ascending, new PlaylistColumn("Key")));

            Assert.That(viewModel.Playlist.Select(x => x.InitialKey), Is.EqualTo(new[] { "1A", "2A", "3A", "4A", "4B", "10A", "11B" }));
        }

        [Test]
        public void SortByKey_Descending_IsTheReverse()
        {
            var viewModel = ViewModelWithKeys("10A", "4B", "1A", "11B", "3A", "4A", "2A");

            viewModel.SortCommand.Execute(new SortMessage(ListSortDirection.Descending, new PlaylistColumn("Key")));

            Assert.That(viewModel.Playlist.Select(x => x.InitialKey), Is.EqualTo(new[] { "11B", "10A", "4B", "4A", "3A", "2A", "1A" }));
        }

        // A tag with something else than a key, like ID3's o for off key, sorts after the keys.
        [TestCase(ListSortDirection.Ascending, new[] { "2A", "10A", "o", "" })]
        [TestCase(ListSortDirection.Descending, new[] { "o", "10A", "2A", "" })]
        public void SortByKey_TrackWithoutKey_StaysAtTheBottom(ListSortDirection direction, string[] expected)
        {
            var viewModel = ViewModelWithKeys("", "o", "10A", "2A");

            viewModel.SortCommand.Execute(new SortMessage(direction, new PlaylistColumn("Key")));

            Assert.That(viewModel.Playlist.Select(x => x.InitialKey), Is.EqualTo(expected));
        }

        [TestCase(ListSortDirection.Ascending, new uint[] { 87, 128, 172, 0, 0 })]
        [TestCase(ListSortDirection.Descending, new uint[] { 172, 128, 87, 0, 0 })]
        public void SortByBeatsPerMinute_TrackWithoutValue_StaysAtTheBottom(ListSortDirection direction, uint[] expected)
        {
            var viewModel = ViewModelWithBeatsPerMinute(0, 128, 87, 0, 172);

            viewModel.SortCommand.Execute(new SortMessage(direction, new PlaylistColumn("BPM")));

            Assert.That(viewModel.Playlist.Select(x => x.BeatsPerMinute), Is.EqualTo(expected));
        }

        private static PlaylistBaseViewModel ViewModelWithDiscs()
        {
            var viewModel = new PlaylistBaseViewModel("MAIN", Substitute.For<ITrackPlaylists>());
            foreach (var (disc, track) in new (uint, uint)[] { (2, 1), (1, 2), (2, 2), (1, 1) })
            {
                viewModel.AddTrack(new LibLalaTagsBuilder($"d{disc}t{track}", new UncheckedTrackPath("/path/to/bob"), 5).WithDiscNumber(disc).WithTrackNumber(track).Build());
            }
            return viewModel;
        }

        [TestCase("Album", ListSortDirection.Ascending)]
        [TestCase("Album", ListSortDirection.Descending)]
        [TestCase("Artist", ListSortDirection.Ascending)]
        [TestCase("Artist", ListSortDirection.Descending)]
        [TestCase("Genre", ListSortDirection.Ascending)]
        [TestCase("Genre", ListSortDirection.Descending)]
        public void Sort_AlbumWithSeveralDiscs_KeepsTheDiscsTogether(string column, ListSortDirection direction)
        {
            var viewModel = ViewModelWithDiscs();

            viewModel.SortCommand.Execute(new SortMessage(direction, new PlaylistColumn(column)));

            Assert.That(viewModel.Playlist.Select(x => x.Title), Is.EqualTo(new[] { "d1t1", "d1t2", "d2t1", "d2t2" }));
        }

        [Test]
        public void AddTracksToQueueHandler_ReplaceQueue_ReplacesTheTracks()
        {
            var viewModel = ViewModelWithTracks(2, ShuffleRepeatState.None);
            var tracks = new LibLalaTags[] { new LibLalaTagsBuilder("new", new UncheckedTrackPath("/path/to/new"), 5).Build() };

            viewModel.AddTracksToQueueHandler(null, new AddTracksToQueueEventArgs(tracks, true));

            Assert.That(viewModel.Playlist.Select(x => x.Title), Is.EqualTo(new[] { "new" }));
        }

        [Test]
        public void AddTracksToQueueHandler_ReplaceQueueWithoutTracks_KeepsTheQueue()
        {
            var viewModel = ViewModelWithTracks(2, ShuffleRepeatState.None);

            viewModel.AddTracksToQueueHandler(null, new AddTracksToQueueEventArgs(new List<LibLalaTags>(), true));

            Assert.That(viewModel.Playlist.Select(x => x.Title), Is.EqualTo(new[] { "track0", "track1" }));
        }

        [TestCase(2, 0, new[] { "track2", "track0", "track1" })]
        [TestCase(0, 2, new[] { "track1", "track2", "track0" })]
        [TestCase(1, 1, new[] { "track0", "track1", "track2" })]
        [TestCase(1, 7, new[] { "track0", "track1", "track2" })]
        public void MoveTrack_MovesItToThatIndex(int from, int to, string[] expected)
        {
            var viewModel = ViewModelWithTracks(3, ShuffleRepeatState.None);

            viewModel.MoveTrack(viewModel.Playlist[from], to);

            Assert.That(viewModel.Playlist.Select(x => x.Title), Is.EqualTo(expected));
        }

        [Test]
        public void AddTrack_LikedTrack_HasItsHeartFilled()
        {
            var trackPlaylists = Substitute.For<ITrackPlaylists>();
            trackPlaylists.IsLiked("/path/to/liked").Returns(true);
            var viewModel = new PlaylistBaseViewModel("MAIN", trackPlaylists);

            viewModel.AddTrack(new LibLalaTagsBuilder("liked", new UncheckedTrackPath("/path/to/liked"), 5).Build());
            viewModel.AddTrack(new LibLalaTagsBuilder("other", new UncheckedTrackPath("/path/to/other"), 5).Build());

            Assert.That(viewModel.Playlist.Select(x => x.IsLiked), Is.EqualTo(new[] { true, false }));
        }

        [Test]
        public void LikedChanged_UpdatesEveryCopyOfThatTrack()
        {
            var trackPlaylists = Substitute.For<ITrackPlaylists>();
            var viewModel = new PlaylistBaseViewModel("MAIN", trackPlaylists);
            var track = new LibLalaTagsBuilder("a", new UncheckedTrackPath("/path/to/a"), 5).Build();
            viewModel.AddTrack(track);
            viewModel.AddTrack(new LibLalaTagsBuilder("b", new UncheckedTrackPath("/path/to/b"), 5).Build());
            viewModel.AddTrack(track);

            trackPlaylists.LikedChanged += Raise.EventWith(new LikedChangedEventArgs(new HashSet<string> { "/path/to/a" }, true));

            Assert.That(viewModel.Playlist.Select(x => x.IsLiked), Is.EqualTo(new[] { true, false, true }));
        }

        [Test]
        public void ToggleLiked_GoesToTheNamedPlaylists()
        {
            var trackPlaylists = Substitute.For<ITrackPlaylists>();
            var viewModel = new PlaylistBaseViewModel("MAIN", trackPlaylists);
            viewModel.AddTrack(new LibLalaTagsBuilder("a", new UncheckedTrackPath("/path/to/a"), 5).Build());

            viewModel.ToggleLikedCommand.Execute(viewModel.Playlist[0]);

            trackPlaylists.Received(1).ToggleLiked(viewModel.Playlist[0]);
        }

        [TestCase(0, "/path/to/track1")]
        [TestCase(1, "/path/to/track2")]
        public void PlayNextTrackHandler_PlayingTrackRemoved_PlaysWhatTookItsPlace(int playing, string expected)
        {
            var playedPath = "";
            var viewModel = ViewModelWithTracks(3, ShuffleRepeatState.None);
            viewModel.PlayTrackCommand.Execute(viewModel.Playlist[playing]);
            viewModel.RemoveTrackCommand.Execute(viewModel.Playlist[playing]);
            viewModel.PlayTrackEvent += (o, e) => playedPath = e.Path;

            viewModel.PlayNextTrackHandler(null, new System.EventArgs());

            Assert.That(playedPath, Is.EqualTo(expected));
        }

        [Test]
        public void PlayNextTrackHandler_LastTrackPlayingAndRemoved_Stops()
        {
            var wasCalled = false;
            var viewModel = ViewModelWithTracks(3, ShuffleRepeatState.None);
            viewModel.PlayTrackCommand.Execute(viewModel.Playlist[2]);
            viewModel.RemoveTrackCommand.Execute(viewModel.Playlist[2]);
            viewModel.PlayTrackEvent += (o, e) => wasCalled = true;

            viewModel.PlayNextTrackHandler(null, new System.EventArgs());

            Assert.That(!wasCalled);
        }

        [Test]
        public void PlayPreviousTrackHandler_PlayingTrackRemoved_PlaysTheOneBeforeIt()
        {
            var playedPath = "";
            var viewModel = ViewModelWithTracks(4, ShuffleRepeatState.None);
            viewModel.PlayTrackCommand.Execute(viewModel.Playlist[2]);
            viewModel.RemoveTrackCommand.Execute(viewModel.Playlist[2]);
            viewModel.PlayTrackEvent += (o, e) => playedPath = e.Path;

            viewModel.PlayLastTrackHandler(null, new System.EventArgs());

            Assert.That(playedPath, Is.EqualTo("/path/to/track1"));
        }

        [Test]
        public void PlayNextTrackHandler_RemovedThenPlayedAnother_GoesOnFromThatOne()
        {
            var playedPath = "";
            var viewModel = ViewModelWithTracks(4, ShuffleRepeatState.None);
            viewModel.PlayTrackCommand.Execute(viewModel.Playlist[2]);
            viewModel.RemoveTrackCommand.Execute(viewModel.Playlist[2]);
            viewModel.PlayTrackCommand.Execute(viewModel.Playlist[0]);
            viewModel.PlayTrackEvent += (o, e) => playedPath = e.Path;

            viewModel.PlayNextTrackHandler(null, new System.EventArgs());

            Assert.That(playedPath, Is.EqualTo("/path/to/track1"));
        }

        [Test]
        public void PlayNextTrackHandler_PlayingTrackRemovedThenAnEarlierOne_PlaysWhatTookItsPlace()
        {
            var playedPath = "";
            var viewModel = ViewModelWithTracks(5, ShuffleRepeatState.None);
            viewModel.PlayTrackCommand.Execute(viewModel.Playlist[2]);
            viewModel.RemoveTrackCommand.Execute(viewModel.Playlist[2]);
            viewModel.RemoveTrackCommand.Execute(viewModel.Playlist[0]);
            viewModel.PlayTrackEvent += (o, e) => playedPath = e.Path;

            viewModel.PlayNextTrackHandler(null, new System.EventArgs());

            Assert.That(playedPath, Is.EqualTo("/path/to/track3"));
        }

        [Test]
        public void PlayPreviousTrackHandler_PlayingTrackRemovedThenAnEarlierOne_PlaysTheOneBeforeIt()
        {
            var playedPath = "";
            var viewModel = ViewModelWithTracks(5, ShuffleRepeatState.None);
            viewModel.PlayTrackCommand.Execute(viewModel.Playlist[2]);
            viewModel.RemoveTrackCommand.Execute(viewModel.Playlist[2]);
            viewModel.RemoveTrackCommand.Execute(viewModel.Playlist[0]);
            viewModel.PlayTrackEvent += (o, e) => playedPath = e.Path;

            viewModel.PlayLastTrackHandler(null, new System.EventArgs());

            Assert.That(playedPath, Is.EqualTo("/path/to/track1"));
        }

        [Test]
        public void PlayNextTrackHandler_PlayingTrackRemovedThenALaterOne_PlaysWhatTookItsPlace()
        {
            var playedPath = "";
            var viewModel = ViewModelWithTracks(5, ShuffleRepeatState.None);
            viewModel.PlayTrackCommand.Execute(viewModel.Playlist[2]);
            viewModel.RemoveTrackCommand.Execute(viewModel.Playlist[2]);
            viewModel.RemoveTrackCommand.Execute(viewModel.Playlist[3]);
            viewModel.PlayTrackEvent += (o, e) => playedPath = e.Path;

            viewModel.PlayNextTrackHandler(null, new System.EventArgs());

            Assert.That(playedPath, Is.EqualTo("/path/to/track3"));
        }

        // After removing track2 the list is 0, 1, 3, 4 and track3 is next. Moving others around it doesn't change that.
        [TestCase(0, 3)]
        [TestCase(3, 0)]
        [TestCase(0, 1)]
        [TestCase(1, 0)]
        public void PlayNextTrackHandler_PlayingTrackRemovedThenOthersMoved_PlaysWhatTookItsPlace(int from, int to)
        {
            var playedPath = "";
            var viewModel = ViewModelWithTracks(5, ShuffleRepeatState.None);
            viewModel.PlayTrackCommand.Execute(viewModel.Playlist[2]);
            viewModel.RemoveTrackCommand.Execute(viewModel.Playlist[2]);
            viewModel.MoveTrack(viewModel.Playlist[from], to);
            viewModel.PlayTrackEvent += (o, e) => playedPath = e.Path;

            viewModel.PlayNextTrackHandler(null, new System.EventArgs());

            Assert.That(playedPath, Is.EqualTo("/path/to/track3"));
        }

        // Sorted, the place it had doesn't mean anything any more.
        [Test]
        public void PlayNextTrackHandler_PlayingTrackRemovedThenSorted_StartsFromTheTop()
        {
            var playedPath = "";
            var viewModel = ViewModelWithTracks(4, ShuffleRepeatState.None);
            viewModel.PlayTrackCommand.Execute(viewModel.Playlist[2]);
            viewModel.RemoveTrackCommand.Execute(viewModel.Playlist[2]);
            viewModel.SortCommand.Execute(new SortMessage(ListSortDirection.Descending, new PlaylistColumn("Title")));
            viewModel.PlayTrackEvent += (o, e) => playedPath = e.Path;

            viewModel.PlayNextTrackHandler(null, new System.EventArgs());

            Assert.That(playedPath, Is.EqualTo("/path/to/track3"));
        }

        private static PlaylistBaseViewModel ViewModelWithTracks(int numberOfTracks, ShuffleRepeatState state)
        {
            var viewModel = new PlaylistBaseViewModel("MAIN", Substitute.For<ITrackPlaylists>()) { PlaylistShuffleRepeatState = state };
            for (var i = 0; i < numberOfTracks; ++i)
            {
                viewModel.AddTrack(new LibLalaTagsBuilder($"track{i}", new UncheckedTrackPath($"/path/to/track{i}"), 5).Build());
            }
            return viewModel;
        }

        [TestCase(ShuffleRepeatState.None)]
        [TestCase(ShuffleRepeatState.RepeatOne)]
        [TestCase(ShuffleRepeatState.RepeatAll)]
        [TestCase(ShuffleRepeatState.Shuffle)]
        public void PlayNextTrackHandler_NoTracks_DoesNothingInAnyMode(ShuffleRepeatState state)
        {
            var wasCalled = false;
            var viewModel = ViewModelWithTracks(0, state);
            viewModel.PlayTrackEvent += (o, e) => wasCalled = true;
            viewModel.PlayNextTrackHandler(null, new System.EventArgs());
            Assert.That(!wasCalled);
        }

        [Test]
        public void PlayNextTrackHandler_RepeatOneNothingPlaying_PlaysFirstTrack()
        {
            var playedPath = "";
            var viewModel = ViewModelWithTracks(3, ShuffleRepeatState.RepeatOne);
            viewModel.PlayTrackEvent += (o, e) => playedPath = e.Path;
            viewModel.PlayNextTrackHandler(null, new System.EventArgs());
            Assert.That(playedPath, Is.EqualTo("/path/to/track0"));
        }

        [Test]
        public void PlayNextTrackHandler_RepeatOne_RepeatsCurrentTrack()
        {
            var playedPath = "";
            var viewModel = ViewModelWithTracks(3, ShuffleRepeatState.RepeatOne);
            viewModel.PlayTrackCommand.Execute(viewModel.Playlist[1]);
            viewModel.PlayTrackEvent += (o, e) => playedPath = e.Path;
            viewModel.PlayNextTrackHandler(null, new System.EventArgs());
            Assert.That(playedPath, Is.EqualTo("/path/to/track1"));
        }

        [Test]
        public void PlayNextTrackHandler_RepeatAllAtTheEnd_StartsOver()
        {
            var playedPath = "";
            var viewModel = ViewModelWithTracks(3, ShuffleRepeatState.RepeatAll);
            viewModel.PlayTrackCommand.Execute(viewModel.Playlist[2]);
            viewModel.PlayTrackEvent += (o, e) => playedPath = e.Path;
            viewModel.PlayNextTrackHandler(null, new System.EventArgs());
            Assert.That(playedPath, Is.EqualTo("/path/to/track0"));
        }

        [Test]
        public void PlayNextTrackHandler_NoRepeatAtTheEnd_Stops()
        {
            var wasCalled = false;
            var viewModel = ViewModelWithTracks(3, ShuffleRepeatState.None);
            viewModel.PlayTrackCommand.Execute(viewModel.Playlist[2]);
            viewModel.PlayTrackEvent += (o, e) => wasCalled = true;
            viewModel.PlayNextTrackHandler(null, new System.EventArgs());
            Assert.That(!wasCalled);
        }

        [Test]
        public void NextTrack_Shuffle_CanPickEveryTrack()
        {
            var viewModel = ViewModelWithTracks(3, ShuffleRepeatState.Shuffle);
            var picked = new bool[3];
            for (var i = 0; i < 1000; ++i)
            {
                picked[viewModel.Playlist.IndexOf(viewModel.NextTrack!)] = true;
            }
            Assert.That(picked, Is.All.True);
        }
    }
}
