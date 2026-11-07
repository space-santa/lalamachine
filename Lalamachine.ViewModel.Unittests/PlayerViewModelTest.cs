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
        public void WhenTheMixedInTrackTakesOver_ThePositionIsToldBeforeTheDuration()
        {
            // The other way round, the slider would clamp the old track's position to a shorter new track and seek there.
            var changed = new List<string?>();
            _viewModel.PropertyChanged += (sender, e) => changed.Add(e.PropertyName);
            _player.TransitionDue += Raise.Event();

            _player.MixedIn += Raise.Event();

            Assert.That(changed.IndexOf(nameof(PlayerViewModel.Position)), Is.GreaterThanOrEqualTo(0).And.LessThan(changed.IndexOf(nameof(PlayerViewModel.Duration))));
        }

        [Test]
        public void WhenATrackOpens_ThePositionIsToldBeforeTheDuration()
        {
            var changed = new List<string?>();
            _viewModel.PropertyChanged += (sender, e) => changed.Add(e.PropertyName);

            _player.MediaOpened += Raise.Event();

            Assert.That(changed, Is.EqualTo(new[] { nameof(PlayerViewModel.Position), nameof(PlayerViewModel.Duration) }));
        }

        [Test]
        public void DraggingThePosition_Seeks()
        {
            _viewModel.IsDraggingPosition = true;

            _viewModel.Position = 100;

            _player.Received(1).Position = TimeSpan.FromSeconds(100);
        }

        [Test]
        public void WhileThePositionIsHeld_ATrackThatTookOver_IsntSeekedByIt_UntilItsLetGo()
        {
            _viewModel.IsDraggingPosition = true;
            _player.TransitionDue += Raise.Event();
            _player.MixedIn += Raise.Event();

            _viewModel.Position = 290;
            _player.DidNotReceive().Position = Arg.Any<TimeSpan>();

            _viewModel.IsDraggingPosition = false;
            _viewModel.Position = 10;
            _player.Received(1).Position = TimeSpan.FromSeconds(10);
        }

        [Test]
        public void WhileTheNextTrackComesIn_ItIsShownAsHeard()
        {
            Assert.That(_viewModel.IsMixing, Is.False);

            _player.IncomingPosition.Returns(TimeSpan.FromSeconds(5));
            _player.IncomingDuration.Returns(TimeSpan.FromSeconds(251));

            Assert.That(_viewModel.IsMixing, Is.True);
            Assert.That(_viewModel.IncomingPosition, Is.EqualTo(5));
            Assert.That(_viewModel.IncomingDuration, Is.EqualTo(251));
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
