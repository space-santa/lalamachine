using Lalamachine.Wpf.ViewModel;
using LibLala.TagReader;
using NUnit.Framework;

namespace Lalamachine.Wpf.Unittests
{
    public class PlaylistViewModelTest
    {
        [Test]
        public void PlayNextTrackHandler_NoTracks_DoesNothing()
        {
            var wasCalled = false;
            var viewModel = new PlaylistBaseViewModel("LIBRARY");
            viewModel.PlayTrackEvent += (o, e) => wasCalled = true;
            viewModel.PlayNextTrackHandler(null, null);
            Assert.IsFalse(wasCalled);
        }

        [Test]
        public void PlayNextTrackHandler_HasTracks_PlayNext()
        {
            var wasCalled = false;
            var viewModel = new PlaylistBaseViewModel("LIBRARY");
            viewModel.AddTrack(new Tags("bob", "/path/to/bob"));
            viewModel.PlayTrackEvent += (o, e) => wasCalled = true;
            viewModel.PlayNextTrackHandler(null, null);
            Assert.IsTrue(wasCalled);
        }

        [Test]
        public void PlayPreviousTrackHandler_NoTracks_DoesNothing()
        {
            var wasCalled = false;
            var viewModel = new PlaylistBaseViewModel("LIBRARY");
            viewModel.PlayTrackEvent += (o, e) => wasCalled = true;
            viewModel.PlayLastTrackHandler(null, null);
            Assert.IsFalse(wasCalled);
        }

        [Test]
        public void PlayPreviousTrackHandler_HasTracks_PlayPrevious()
        {
            var wasCalled = false;
            var viewModel = new PlaylistBaseViewModel("LIBRARY");
            viewModel.AddTrack(new Tags("bob", "/path/to/bob"));
            viewModel.PlayTrackEvent += (o, e) => wasCalled = true;
            viewModel.PlayLastTrackHandler(null, null);
            Assert.IsTrue(wasCalled);
        }
    }
}
