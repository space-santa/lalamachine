using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using LibLala.DomainPrimitives;
using LibLala.LibLalaTagReader;
using NSubstitute;
using NUnit.Framework;

namespace Lalamachine.ViewModel.Unittests
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "<Pending>")]
    public class LibraryPlaylistViewModelTest
    {
        private static PlaylistTags Track(string title, string key, uint beatsPerMinute = 0)
        {
            return new PlaylistTags(new LibLalaTagsBuilder(title, new UncheckedTrackPath($"/path/to/{title}"), 5).WithInitialKey(key).WithBeatsPerMinute(beatsPerMinute).Build());
        }

        private static void ShowLibrary(LibraryPlaylistViewModel viewModel, params PlaylistTags[] tracks)
        {
            viewModel.DisplayChangedHandler(null, new DisplayLibChangedEventArgs(new ObservableCollection<PlaylistTags>(tracks)));
        }

        private static LibraryPlaylistViewModel ViewModelWith(params PlaylistTags[] tracks)
        {
            var viewModel = new LibraryPlaylistViewModel(Substitute.For<IDialogService>(), Substitute.For<ITrackPlaylists>());
            ShowLibrary(viewModel, tracks);
            return viewModel;
        }

        private static void Tick(LibraryPlaylistViewModel viewModel, string key)
        {
            viewModel.KeyFilterOptions.Single(x => x.Key == key).IsSelected = true;
        }

        private static IEnumerable<string> Titles(LibraryPlaylistViewModel viewModel)
        {
            return viewModel.Playlist.Select(x => x.Title);
        }

        [Test]
        public void KeyFilterOptions_AreTheKeysOfTheTracks_InColumnOrderWithoutKeyLast()
        {
            var viewModel = ViewModelWith(Track("a", "10A"), Track("b", ""), Track("c", "2A"), Track("d", "4B"), Track("e", "4A"), Track("f", "2A"));

            Assert.That(viewModel.KeyFilterOptions.Select(x => x.Key), Is.EqualTo(new[] { "2A", "4A", "4B", "10A", "" }));
            Assert.That(viewModel.KeyFilterOptions.Last().Label, Is.EqualTo("(no key)"));
            Assert.That(viewModel.KeyFilterOptions.First().Label, Is.EqualTo("2A"));
            Assert.That(viewModel.IsKeyFilterActive, Is.False);
        }

        [Test]
        public void TickingKeys_ShowsTheTracksWithAnyOfThem()
        {
            var viewModel = ViewModelWith(Track("a", "10A"), Track("b", "2A"), Track("c", "4B"));

            Tick(viewModel, "10A");
            Assert.That(Titles(viewModel), Is.EqualTo(new[] { "a" }));
            Assert.That(viewModel.IsKeyFilterActive, Is.True);

            Tick(viewModel, "4B");
            Assert.That(Titles(viewModel), Is.EqualTo(new[] { "a", "c" }));
            Assert.That(viewModel.NumberOfTracks, Is.EqualTo(2));
        }

        [Test]
        public void TickingNoKey_ShowsTheTracksWithoutAKey()
        {
            var viewModel = ViewModelWith(Track("a", "10A"), Track("b", ""));

            Tick(viewModel, "");

            Assert.That(Titles(viewModel), Is.EqualTo(new[] { "b" }));
        }

        [Test]
        public void UntickingTheLastKey_ShowsEverything()
        {
            var viewModel = ViewModelWith(Track("a", "10A"), Track("b", "2A"));
            Tick(viewModel, "10A");

            viewModel.KeyFilterOptions.Single(x => x.Key == "10A").IsSelected = false;

            Assert.That(Titles(viewModel), Is.EqualTo(new[] { "a", "b" }));
            Assert.That(viewModel.IsKeyFilterActive, Is.False);
        }

        [Test]
        public void ClearKeyFilter_ShowsEverythingAndUnticksAll()
        {
            var viewModel = ViewModelWith(Track("a", "10A"), Track("b", "2A"), Track("c", "4B"));
            Tick(viewModel, "10A");
            Tick(viewModel, "4B");

            viewModel.ClearKeyFilterCommand.Execute(null);

            Assert.That(Titles(viewModel), Is.EqualTo(new[] { "a", "b", "c" }));
            Assert.That(viewModel.KeyFilterOptions.Any(x => x.IsSelected), Is.False);
            Assert.That(viewModel.IsKeyFilterActive, Is.False);
        }

        [Test]
        public void ClearKeyFilterHandler_ShowsEverythingAndUnticksAll()
        {
            var viewModel = ViewModelWith(Track("a", "10A"), Track("b", "2A"));
            Tick(viewModel, "10A");

            viewModel.ClearKeyFilterHandler(null, System.EventArgs.Empty);

            Assert.That(Titles(viewModel), Is.EqualTo(new[] { "a", "b" }));
            Assert.That(viewModel.IsKeyFilterActive, Is.False);
        }

        [Test]
        public void ClearKeyFilterHandler_NoFilter_DoesNotRebuildTheList()
        {
            var viewModel = ViewModelWith(Track("a", "10A"), Track("b", "2A"));
            var notifications = new List<string?>();
            viewModel.PropertyChanged += (o, e) => notifications.Add(e.PropertyName);

            viewModel.ClearKeyFilterHandler(null, System.EventArgs.Empty);

            Assert.That(notifications, Is.Empty);
        }

        [Test]
        public void Filtering_TellsTheViewWhatChanged()
        {
            var viewModel = ViewModelWith(Track("a", "10A"), Track("b", "2A"));
            var notifications = new List<string?>();
            viewModel.PropertyChanged += (o, e) => notifications.Add(e.PropertyName);

            Tick(viewModel, "10A");

            Assert.That(notifications, Does.Contain(nameof(viewModel.Playlist)));
            Assert.That(notifications, Does.Contain(nameof(viewModel.IsKeyFilterActive)));
            Assert.That(notifications, Does.Contain(nameof(viewModel.NumberOfTracks)));
        }

        [Test]
        public void NewLibrarySelection_KeepsTheFilter()
        {
            var viewModel = ViewModelWith(Track("a", "10A"), Track("b", "2A"));
            Tick(viewModel, "10A");

            ShowLibrary(viewModel, Track("c", "10A"), Track("d", "4B"));

            Assert.That(Titles(viewModel), Is.EqualTo(new[] { "c" }));
            Assert.That(viewModel.KeyFilterOptions.Select(x => x.Key), Is.EqualTo(new[] { "4B", "10A" }));
            Assert.That(viewModel.KeyFilterOptions.Single(x => x.Key == "10A").IsSelected, Is.True);
        }

        [Test]
        public void NewLibrarySelection_WithoutTheTickedKey_StaysListedAndShowsNothing()
        {
            var viewModel = ViewModelWith(Track("a", "10A"), Track("b", "2A"));
            Tick(viewModel, "10A");

            ShowLibrary(viewModel, Track("c", "4B"));

            Assert.That(Titles(viewModel), Is.Empty);
            Assert.That(viewModel.KeyFilterOptions.Select(x => x.Key), Is.EqualTo(new[] { "4B", "10A" }));
            Assert.That(viewModel.IsKeyFilterActive, Is.True);
        }

        [Test]
        public void ClearKeyFilter_DropsTheKeyThatNoTrackHasAnyMore()
        {
            var viewModel = ViewModelWith(Track("a", "10A"));
            Tick(viewModel, "10A");
            ShowLibrary(viewModel, Track("c", "4B"));

            viewModel.ClearKeyFilterCommand.Execute(null);

            Assert.That(viewModel.KeyFilterOptions.Select(x => x.Key), Is.EqualTo(new[] { "4B" }));
            Assert.That(Titles(viewModel), Is.EqualTo(new[] { "c" }));
        }

        [Test]
        public void SortWhileFiltered_SortsWhatIsShown()
        {
            var viewModel = ViewModelWith(Track("a", "10A", 128), Track("b", "2A", 90), Track("c", "10A", 87));
            Tick(viewModel, "10A");

            viewModel.SortCommand.Execute(new SortMessage(ListSortDirection.Ascending, new PlaylistColumn("BPM")));

            Assert.That(Titles(viewModel), Is.EqualTo(new[] { "c", "a" }));
        }

        [Test]
        public void SortWhileFiltered_HiddenTracksAreInOrderWhenTheFilterIsCleared()
        {
            var viewModel = ViewModelWith(Track("a", "10A", 128), Track("b", "2A", 90), Track("c", "10A", 87));
            Tick(viewModel, "10A");
            viewModel.SortCommand.Execute(new SortMessage(ListSortDirection.Ascending, new PlaylistColumn("BPM")));

            viewModel.ClearKeyFilterCommand.Execute(null);

            Assert.That(Titles(viewModel), Is.EqualTo(new[] { "c", "b", "a" }));
        }

        // Sorting by BPM and then by key gives the BPM order within each key. Filtering must not undo that.
        [Test]
        public void Filtering_KeepsTheOrderOfEarlierSorts()
        {
            var viewModel = ViewModelWith(Track("a", "10A", 128), Track("b", "2A", 140), Track("c", "10A", 87), Track("d", "2A", 90));
            viewModel.SortCommand.Execute(new SortMessage(ListSortDirection.Ascending, new PlaylistColumn("BPM")));
            viewModel.SortCommand.Execute(new SortMessage(ListSortDirection.Ascending, new PlaylistColumn("Key")));

            Tick(viewModel, "10A");
            Assert.That(Titles(viewModel), Is.EqualTo(new[] { "c", "a" }));

            viewModel.ClearKeyFilterCommand.Execute(null);
            Assert.That(Titles(viewModel), Is.EqualTo(new[] { "d", "b", "c", "a" }));
        }

        [Test]
        public void PlayNext_WhileFiltered_SkipsTheHiddenTracks()
        {
            var viewModel = ViewModelWith(Track("a", "10A"), Track("b", "2A"), Track("c", "10A"));
            Tick(viewModel, "10A");
            viewModel.PlayTrackCommand.Execute(viewModel.Playlist[0]);
            var playedPath = "";
            viewModel.PlayTrackEvent += (o, e) => playedPath = e.Path;

            viewModel.PlayNextTrackHandler(null, new System.EventArgs());

            Assert.That(playedPath, Is.EqualTo("/path/to/c"));
        }

        [Test]
        public void PlayNext_PlayingTrackGotHidden_PlaysTheFirstOneShown()
        {
            var viewModel = ViewModelWith(Track("a", "2A"), Track("b", "10A"), Track("c", "10A"));
            viewModel.PlayTrackCommand.Execute(viewModel.Playlist[0]);
            Tick(viewModel, "10A");
            var playedPath = "";
            viewModel.PlayTrackEvent += (o, e) => playedPath = e.Path;

            viewModel.PlayNextTrackHandler(null, new System.EventArgs());

            Assert.That(playedPath, Is.EqualTo("/path/to/b"));
        }

        // Otherwise two tracks would show as playing once the filter lets the hidden one through again.
        [Test]
        public void PlayingAnotherTrack_PlayingTrackGotHidden_OnlyTheNewOneIsPlaying()
        {
            var viewModel = ViewModelWith(Track("a", "2A"), Track("b", "10A"));
            viewModel.PlayTrackCommand.Execute(viewModel.Playlist[0]);
            Tick(viewModel, "10A");

            viewModel.PlayTrackCommand.Execute(viewModel.Playlist[0]);
            viewModel.ClearKeyFilterCommand.Execute(null);

            Assert.That(viewModel.Playlist.Where(x => x.IsPlaying).Select(x => x.Title), Is.EqualTo(new[] { "b" }));
        }

        [Test]
        public void Filtering_ThePlayingTrackKeepsPlayingWhenShownAgain()
        {
            var viewModel = ViewModelWith(Track("a", "2A"), Track("b", "10A"));
            viewModel.PlayTrackCommand.Execute(viewModel.Playlist[0]);
            Tick(viewModel, "10A");

            viewModel.ClearKeyFilterCommand.Execute(null);

            Assert.That(viewModel.CurrentIndex, Is.EqualTo(0));
        }

        [Test]
        public void NewLibrarySelection_WithThePlayingTrack_NextPlaysTheOneAfterIt()
        {
            var viewModel = ViewModelWith(Track("a", "2A"), Track("b", "2A"), Track("c", "2A"));
            viewModel.PlayTrackCommand.Execute(viewModel.Playlist[1]);
            var playedPath = "";
            viewModel.PlayTrackEvent += (o, e) => playedPath = e.Path;

            ShowLibrary(viewModel, Track("a", "2A"), Track("b", "2A"), Track("c", "2A"));

            Assert.That(viewModel.CurrentIndex, Is.EqualTo(1));
            viewModel.PlayNextTrackHandler(null, new System.EventArgs());
            Assert.That(playedPath, Is.EqualTo("/path/to/c"));
        }

        [Test]
        public void NewLibrarySelection_PlayingTrackComesBack_IsPlayingAgain()
        {
            var viewModel = ViewModelWith(Track("a", "2A"), Track("b", "2A"));
            viewModel.PlayTrackCommand.Execute(viewModel.Playlist[1]);

            ShowLibrary(viewModel, Track("c", "2A"));
            Assert.That(viewModel.CurrentIndex, Is.EqualTo(-1));

            ShowLibrary(viewModel, Track("a", "2A"), Track("b", "2A"));
            Assert.That(viewModel.CurrentIndex, Is.EqualTo(1));
        }

        [Test]
        public void NewLibrarySelection_AfterPlayingAnotherTrack_OnlyMarksThatOne()
        {
            var viewModel = ViewModelWith(Track("a", "2A"), Track("b", "2A"));
            viewModel.PlayTrackCommand.Execute(viewModel.Playlist[0]);
            viewModel.PlayTrackCommand.Execute(viewModel.Playlist[1]);

            ShowLibrary(viewModel, Track("a", "2A"), Track("b", "2A"));

            Assert.That(viewModel.Playlist.Where(x => x.IsPlaying).Select(x => x.Title), Is.EqualTo(new[] { "b" }));
        }

        [Test]
        public void NewLibrarySelection_NothingPlayedYet_MarksNothing()
        {
            var viewModel = ViewModelWith(Track("a", "2A"));

            ShowLibrary(viewModel, Track("a", "2A"));

            Assert.That(viewModel.CurrentIndex, Is.EqualTo(-1));
        }

        [Test]
        public void NewLibrarySelection_MarksTheLikedTracks()
        {
            var trackPlaylists = Substitute.For<ITrackPlaylists>();
            trackPlaylists.IsLiked("/path/to/a").Returns(true);
            var viewModel = new LibraryPlaylistViewModel(Substitute.For<IDialogService>(), trackPlaylists);

            ShowLibrary(viewModel, Track("a", "2A"), Track("b", "2A"));

            Assert.That(viewModel.Playlist.Select(x => x.IsLiked), Is.EqualTo(new[] { true, false }));
        }

        // A filter makes a new list, but it's the same objects for the tracks that it let through before.
        [Test]
        public void LikedChanged_TrackHiddenByTheFilter_IsLikedWhenShownAgain()
        {
            var trackPlaylists = Substitute.For<ITrackPlaylists>();
            var viewModel = new LibraryPlaylistViewModel(Substitute.For<IDialogService>(), trackPlaylists);
            ShowLibrary(viewModel, Track("a", "2A"), Track("b", "10A"));
            Tick(viewModel, "10A");

            trackPlaylists.LikedChanged += Raise.EventWith(new LikedChangedEventArgs("/path/to/a", true));
            viewModel.ClearKeyFilterCommand.Execute(null);

            Assert.That(viewModel.Playlist.Select(x => x.IsLiked), Is.EqualTo(new[] { true, false }));
        }

        [TestCase(ShuffleRepeatState.RepeatOne, "/path/to/a")]
        [TestCase(ShuffleRepeatState.RepeatAll, "/path/to/b")]
        public void ShuffleRepeatChanged_AppliesToTheLibraryToo(ShuffleRepeatState state, string expected)
        {
            var viewModel = ViewModelWith(Track("a", "2A"), Track("b", "2A"));
            var shuffleRepeat = new ShuffleRepeatViewModel();
            shuffleRepeat.ShuffleRepeatChanged += viewModel.ShuffleRepeatChangedHandler;
            viewModel.PlayTrackCommand.Execute(viewModel.Playlist[0]);
            var playedPath = "";
            viewModel.PlayTrackEvent += (o, e) => playedPath = e.Path;

            shuffleRepeat.State = state;
            viewModel.PlayNextTrackHandler(null, new System.EventArgs());

            Assert.That(playedPath, Is.EqualTo(expected));
        }
    }
}
