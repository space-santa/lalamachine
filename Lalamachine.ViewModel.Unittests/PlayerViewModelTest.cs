using System;
using System.Collections.Generic;
using LibLala.DomainPrimitives;
using LibLala.LibLalaTagReader;
using NSubstitute;
using NUnit.Framework;
using Settings;

namespace Lalamachine.ViewModel.Unittests
{
    public class PlayerViewModelTest
    {
        private IAudioPlayer _player = null!;
        private PlayerViewModel _viewModel = null!;
        private PlaylistBaseViewModel _list = null!;

        [SetUp]
        public void SetUp()
        {
            _player = Substitute.For<IAudioPlayer>();
            _player.Source.Returns(new Uri("file:///music/a.mp3"));
            _viewModel = new PlayerViewModel(_player, Substitute.For<IDialogService>(), new PlayerSettings(new InMemorySettingsFile()));
            // Wired like the queue in the main window.
            _list = new PlaylistBaseViewModel("MAIN", Substitute.For<IDialogService>(), Substitute.For<ITrackPlaylists>());
            foreach (var name in new[] { "a", "b", "c" })
            {
                _list.AddTrack(new LibLalaTagsBuilder(name, new UncheckedTrackPath($"/music/{name}.mp3"), 5).Build());
            }
            _viewModel.PlayNextTrackEvent += _list.PlayNextTrackHandler;
            _list.PlayTrackEvent += _viewModel.PlayTrackHandler;
            _list.PlayTrack(_list.Playlist[0]);
            _player.ClearReceivedCalls();
        }

        [Test]
        public void TransitionsAreOn_AtFirst()
        {
            Assert.That(_viewModel.Transitions, Is.True);
            Assert.That(_player.Transitions, Is.True);
        }

        [Test]
        public void ATransitionDue_MixesInWhatTheListWouldPlayNext_WithoutGoingOnToIt()
        {
            _player.TransitionDue += Raise.Event();

            _player.Received(1).MixTo(new Uri("/music/b.mp3"));
            _player.DidNotReceive().Open(Arg.Any<Uri>());
            Assert.That(_list.CurrentIndex, Is.Zero);
            Assert.That(_viewModel.IsPlaying, Is.True);
        }

        [Test]
        public void WhenTheMixedInTrackTakesOver_TheListGoesOnToIt()
        {
            _player.TransitionDue += Raise.Event();

            _player.MixedIn += Raise.Event();

            Assert.That(_list.CurrentIndex, Is.EqualTo(1));
            _player.DidNotReceive().Open(Arg.Any<Uri>());
            Assert.That(_viewModel.IsPlaying, Is.True);
        }

        [Test]
        public void NextDuringATransition_PlaysTheTrackThatWasMixedIn_AndNothingIsSkipped()
        {
            _player.TransitionDue += Raise.Event();

            _viewModel.NextCommand.Execute(null);
            _player.MixedIn += Raise.Event();

            _player.Received(1).Open(new Uri("/music/b.mp3"));
            Assert.That(_list.CurrentIndex, Is.EqualTo(1));
        }

        [Test]
        public void WithoutTransitions_NothingIsMixedIn()
        {
            _viewModel.Transitions = false;

            _player.TransitionDue += Raise.Event();

            _player.DidNotReceive().MixTo(Arg.Any<Uri>());
            Assert.That(_player.Transitions, Is.False);
        }

        [Test]
        public void WhilePaused_NothingIsMixedIn()
        {
            _viewModel.Pause();

            _player.TransitionDue += Raise.Event();

            _player.DidNotReceive().MixTo(Arg.Any<Uri>());
        }

        [Test]
        public void AtTheEndOfTheList_NothingIsMixedIn()
        {
            _list.PlayTrack(_list.Playlist[2]);

            _player.TransitionDue += Raise.Event();

            _player.DidNotReceive().MixTo(Arg.Any<Uri>());
        }

        private sealed class InMemorySettingsFile : SettingsFile
        {
            private Dictionary<string, string> _settings = [];

            public InMemorySettingsFile() : base("Test", "Test", "/nowhere") { }

            public override Dictionary<string, string> Load() => new(_settings);

            public override void Save(Dictionary<string, string> settings) => _settings = new(settings);
        }
    }
}
