using Lalamachine.Wpf.ViewModel;
using LibLala.DomainPrimitives;
using LibLala.LibLalaTagReader;
using NUnit.Framework;

namespace Lalamachine.Wpf.Unittests
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "<Pending>")]
    public class PlaylistViewModelTest
    {
        [Test]
        public void PlayNextTrackHandler_NoTracks_DoesNothing()
        {
            var wasCalled = false;
            var viewModel = new PlaylistBaseViewModel("LIBRARY");
            viewModel.PlayTrackEvent += (o, e) => wasCalled = true;
            viewModel.PlayNextTrackHandler(null, new System.EventArgs());
            Assert.That(!wasCalled);
        }

        [Test]
        public void PlayNextTrackHandler_HasTracks_PlayNext()
        {
            var wasCalled = false;
            var viewModel = new PlaylistBaseViewModel("LIBRARY");
            viewModel.AddTrack(new LibLalaTagsBuilder("bob", new UncheckedTrackPath("/path/to/bob"), 5).Build());
            viewModel.PlayTrackEvent += (o, e) => wasCalled = true;
            viewModel.PlayNextTrackHandler(null, new System.EventArgs());
            Assert.That(wasCalled);
        }

        [Test]
        public void PlayPreviousTrackHandler_NoTracks_DoesNothing()
        {
            var wasCalled = false;
            var viewModel = new PlaylistBaseViewModel("LIBRARY");
            viewModel.PlayTrackEvent += (o, e) => wasCalled = true;
            viewModel.PlayLastTrackHandler(null, new System.EventArgs());
            Assert.That(!wasCalled);
        }

        [Test]
        public void PlayPreviousTrackHandler_HasTracks_PlayPrevious()
        {
            var wasCalled = false;
            var viewModel = new PlaylistBaseViewModel("LIBRARY");
            viewModel.AddTrack(new LibLalaTagsBuilder("bob", new UncheckedTrackPath("/path/to/bob"), 5).Build());
            viewModel.PlayTrackEvent += (o, e) => wasCalled = true;
            viewModel.PlayLastTrackHandler(null, new System.EventArgs());
            Assert.That(wasCalled);
        }

        private static PlaylistBaseViewModel ViewModelWithTracks(int numberOfTracks, ShuffleRepeatState state)
        {
            var viewModel = new PlaylistBaseViewModel("MAIN") { PlaylistShuffleRepeatState = state };
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
