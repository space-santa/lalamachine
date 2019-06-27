using NUnit.Framework;
using Lalamachine.Wpf.ViewModel;
using NSubstitute;
using NSubstitute.Extensions;
using LibLala.TagReader;

namespace Lalamachine.Wpf.Unittests
{
    public class PlaylistViewModelTest
    {
        [Test]
        public void PlayNextTrackHandler_NoTracks_DoesNothing()
        {
            var wasCalled = false;
            PlaylistViewModel viewModel = new PlaylistViewModel();
            viewModel.PlayTrackEvent += (o, e) => wasCalled = true;
            viewModel.PlayNextTrackHandler(null, null);
            Assert.IsFalse(wasCalled);
        }

        [Test]
        public void PlayNextTrackHandler_HasTracks_PlayNext()
        {
            var wasCalled = false;
            PlaylistViewModel viewModel = new PlaylistViewModel();
            viewModel.AddTrack(new Tags());
            viewModel.PlayTrackEvent += (o, e) => wasCalled = true;
            viewModel.PlayNextTrackHandler(null, null);
            Assert.IsTrue(wasCalled);
        }

        [Test]
        public void PlayPreviousTrackHandler_NoTracks_DoesNothing()
        {
            var wasCalled = false;
            PlaylistViewModel viewModel = new PlaylistViewModel();
            viewModel.PlayTrackEvent += (o, e) => wasCalled = true;
            viewModel.PlayLastTrackHandler(null, null);
            Assert.IsFalse(wasCalled);
        }

        [Test]
        public void PlayPreviousTrackHandler_HasTracks_PlayPrevious()
        {
            var wasCalled = false;
            PlaylistViewModel viewModel = new PlaylistViewModel();
            viewModel.AddTrack(new Tags());
            viewModel.PlayTrackEvent += (o, e) => wasCalled = true;
            viewModel.PlayLastTrackHandler(null, null);
            Assert.IsTrue(wasCalled);
        }
    }
}