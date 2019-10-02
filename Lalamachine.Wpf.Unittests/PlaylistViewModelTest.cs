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
            Assert.IsFalse(wasCalled);
        }

        [Test]
        public void PlayNextTrackHandler_HasTracks_PlayNext()
        {
            var wasCalled = false;
            var viewModel = new PlaylistBaseViewModel("LIBRARY");
            viewModel.AddTrack(new LibLalaTagsBuilder("bob", new FakeTrackPath("/path/to/bob"), 5).Build());
            viewModel.PlayTrackEvent += (o, e) => wasCalled = true;
            viewModel.PlayNextTrackHandler(null, new System.EventArgs());
            Assert.IsTrue(wasCalled);
        }

        [Test]
        public void PlayPreviousTrackHandler_NoTracks_DoesNothing()
        {
            var wasCalled = false;
            var viewModel = new PlaylistBaseViewModel("LIBRARY");
            viewModel.PlayTrackEvent += (o, e) => wasCalled = true;
            viewModel.PlayLastTrackHandler(null, new System.EventArgs());
            Assert.IsFalse(wasCalled);
        }

        [Test]
        public void PlayPreviousTrackHandler_HasTracks_PlayPrevious()
        {
            var wasCalled = false;
            var viewModel = new PlaylistBaseViewModel("LIBRARY");
            viewModel.AddTrack(new LibLalaTagsBuilder("bob", new FakeTrackPath("/path/to/bob"), 5).Build());
            viewModel.PlayTrackEvent += (o, e) => wasCalled = true;
            viewModel.PlayLastTrackHandler(null, new System.EventArgs());
            Assert.IsTrue(wasCalled);
        }
    }
}
