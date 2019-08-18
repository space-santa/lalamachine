using System;
using LalaDb.Data;
using Lalamachine.Wpf.ViewModel;
using LibLala.TagReader;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Lalamachine.Wpf.Unittests
{
    public class PlaylistViewModelTest
    {
        [Test]
        public void PlayNextTrackHandler_NoTracks_DoesNothing()
        {
            var wasCalled = false;
            var viewModel = new PlaylistViewModel();
            viewModel.PlayTrackEvent += (o, e) => wasCalled = true;
            viewModel.PlayNextTrackHandler(null, null);
            Assert.IsFalse(wasCalled);
        }

        [Test]
        public void PlayNextTrackHandler_HasTracks_PlayNext()
        {
            var wasCalled = false;
            var viewModel = new PlaylistViewModel();
            viewModel.AddTrack(new Tags());
            viewModel.PlayTrackEvent += (o, e) => wasCalled = true;
            viewModel.PlayNextTrackHandler(null, null);
            Assert.IsTrue(wasCalled);
        }

        [Test]
        public void PlayPreviousTrackHandler_NoTracks_DoesNothing()
        {
            var wasCalled = false;
            var viewModel = new PlaylistViewModel();
            viewModel.PlayTrackEvent += (o, e) => wasCalled = true;
            viewModel.PlayLastTrackHandler(null, null);
            Assert.IsFalse(wasCalled);
        }

        [Test]
        public void PlayPreviousTrackHandler_HasTracks_PlayPrevious()
        {
            var wasCalled = false;
            var viewModel = new PlaylistViewModel();
            viewModel.AddTrack(new Tags());
            viewModel.PlayTrackEvent += (o, e) => wasCalled = true;
            viewModel.PlayLastTrackHandler(null, null);
            Assert.IsTrue(wasCalled);
        }

        [Test]
        public void Instance_NoContext_HasNameLibrary()
        {
            var viewModel = new PlaylistViewModel();
            Assert.AreEqual(viewModel.Name, "LIBRARY");
        }

        [Test]
        public void Instance_WithContext_HasNameMain()
        {
            var optionsBuilder = new DbContextOptionsBuilder<LalaContext>().UseInMemoryDatabase(Guid.NewGuid().ToString());
            var contextMock = new LalaContext(optionsBuilder.Options);
            var viewModel = new PlaylistViewModel(contextMock);
            Assert.AreEqual(viewModel.Name, "MAIN");
        }
    }
}
