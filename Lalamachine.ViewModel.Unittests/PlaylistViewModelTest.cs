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
            var viewModel = new PlaylistBaseViewModel("LIBRARY", Substitute.For<IDialogService>());
            viewModel.PlayTrackEvent += (o, e) => wasCalled = true;
            viewModel.PlayNextTrackHandler(null, new System.EventArgs());
            Assert.That(!wasCalled);
        }

        [Test]
        public void PlayNextTrackHandler_HasTracks_PlayNext()
        {
            var wasCalled = false;
            var viewModel = new PlaylistBaseViewModel("LIBRARY", Substitute.For<IDialogService>());
            viewModel.AddTrack(new LibLalaTagsBuilder("bob", new UncheckedTrackPath("/path/to/bob"), 5).Build());
            viewModel.PlayTrackEvent += (o, e) => wasCalled = true;
            viewModel.PlayNextTrackHandler(null, new System.EventArgs());
            Assert.That(wasCalled);
        }

        [Test]
        public void PlayPreviousTrackHandler_NoTracks_DoesNothing()
        {
            var wasCalled = false;
            var viewModel = new PlaylistBaseViewModel("LIBRARY", Substitute.For<IDialogService>());
            viewModel.PlayTrackEvent += (o, e) => wasCalled = true;
            viewModel.PlayLastTrackHandler(null, new System.EventArgs());
            Assert.That(!wasCalled);
        }

        [Test]
        public void PlayPreviousTrackHandler_HasTracks_PlayPrevious()
        {
            var wasCalled = false;
            var viewModel = new PlaylistBaseViewModel("LIBRARY", Substitute.For<IDialogService>());
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

        private static PlaylistBaseViewModel ViewModelWithKeys(params string[] keys)
        {
            var viewModel = new PlaylistBaseViewModel("MAIN", Substitute.For<IDialogService>());
            foreach (var key in keys)
            {
                viewModel.AddTrack(new LibLalaTagsBuilder("bob", new UncheckedTrackPath("/path/to/bob"), 5).WithInitialKey(key).Build());
            }
            return viewModel;
        }

        private static PlaylistBaseViewModel ViewModelWithBeatsPerMinute(params uint[] values)
        {
            var viewModel = new PlaylistBaseViewModel("MAIN", Substitute.For<IDialogService>());
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

        [TestCase(ListSortDirection.Ascending, new[] { "2A", "10A", "Am", "" })]
        [TestCase(ListSortDirection.Descending, new[] { "Am", "10A", "2A", "" })]
        public void SortByKey_TrackWithoutKey_StaysAtTheBottom(ListSortDirection direction, string[] expected)
        {
            var viewModel = ViewModelWithKeys("", "Am", "10A", "2A");

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
            var viewModel = new PlaylistBaseViewModel("MAIN", Substitute.For<IDialogService>());
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
        public void AddTracksToPlaylistHandler_NewPlaylist_ReplacesTheTracks()
        {
            var viewModel = ViewModelWithTracks(2, ShuffleRepeatState.None);
            var tracks = new LibLalaTags[] { new LibLalaTagsBuilder("new", new UncheckedTrackPath("/path/to/new"), 5).Build() };

            viewModel.AddTracksToPlaylistHandler(null, new AddTracksToPlaylistEventArgs(tracks, true));

            Assert.That(viewModel.Playlist.Select(x => x.Title), Is.EqualTo(new[] { "new" }));
        }

        [Test]
        public void AddTracksToPlaylistHandler_NewPlaylistWithoutTracks_KeepsThePlaylist()
        {
            var viewModel = ViewModelWithTracks(2, ShuffleRepeatState.None);

            viewModel.AddTracksToPlaylistHandler(null, new AddTracksToPlaylistEventArgs(new List<LibLalaTags>(), true));

            Assert.That(viewModel.Playlist.Select(x => x.Title), Is.EqualTo(new[] { "track0", "track1" }));
        }

        private static PlaylistBaseViewModel ViewModelWithTracks(int numberOfTracks, ShuffleRepeatState state)
        {
            var viewModel = new PlaylistBaseViewModel("MAIN", Substitute.For<IDialogService>()) { PlaylistShuffleRepeatState = state };
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
