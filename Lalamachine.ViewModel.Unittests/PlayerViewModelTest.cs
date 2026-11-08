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
            _viewModel.PeekNextTrackEvent += _list.PeekNextTrackHandler;
            _list.PlayTrackEvent += _viewModel.PlayTrackHandler;
            _list.PlayTrack(_list.Playlist[0]);
            _player.MixTo(Arg.Any<Uri>()).Returns(true);
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
        public void ATrackThatCantBeMixedIn_ComesWhenTheOneThatPlaysEnded()
        {
            _player.MixTo(Arg.Any<Uri>()).Returns(false);

            _player.TransitionDue += Raise.Event();
            _player.DidNotReceive().Open(Arg.Any<Uri>());
            Assert.That(_list.CurrentIndex, Is.Zero);
            _player.MediaEnded += Raise.Event();

            _player.Received(1).Open(new Uri("/music/b.mp3"));
        }

        [Test]
        public void NextDuringATransition_PlaysTheTrackThatWasMixedIn_NotWhatTheListWouldSayNow()
        {
            _player.TransitionDue += Raise.Event();
            // Like shuffle mode, where it says another one each time.
            _list.PlaylistShuffleRepeatState = ShuffleRepeatState.RepeatOne;

            _viewModel.NextCommand.Execute(null);

            _player.Received(1).Open(new Uri("/music/b.mp3"));
        }

        [Test]
        public void NextDuringATransition_WhenTheIncomingTrackWasRemoved_PlaysWhatComesNextNow()
        {
            _player.TransitionDue += Raise.Event();
            _list.RemoveTrackCommand.Execute(_list.Playlist[1]);

            _viewModel.NextCommand.Execute(null);

            _player.Received(1).Open(new Uri("/music/c.mp3"));
        }

        [Test]
        public void WhenTheMixedInTrackWasRemoved_TheListGoesOnAfterIt()
        {
            _player.TransitionDue += Raise.Event();
            _list.RemoveTrackCommand.Execute(_list.Playlist[1]);

            _player.MixedIn += Raise.Event();
            Assert.That(_list.CurrentIndex, Is.EqualTo(-1));
            _player.DidNotReceive().Open(Arg.Any<Uri>());
            _player.TransitionDue += Raise.Event();

            _player.Received(1).MixTo(new Uri("/music/c.mp3"));
        }

        [Test]
        public void WhenTheMixedInTrackTakesOver_ItsPositionAndDurationAreTold()
        {
            var changed = new List<string?>();
            _viewModel.PropertyChanged += (sender, e) => changed.Add(e.PropertyName);
            _player.TransitionDue += Raise.Event();

            _player.MixedIn += Raise.Event();

            Assert.That(changed, Does.Contain(nameof(PlayerViewModel.Position)).And.Contain(nameof(PlayerViewModel.Duration)));
        }

        [Test]
        public void DraggingThePosition_Seeks()
        {
            _viewModel.IsDraggingPosition = true;

            _viewModel.Position = 100;

            _player.Received(1).Position = TimeSpan.FromSeconds(100);
        }

        // The slider writes back a position that it clamped to its maximum, when a track's duration changes before its
        // position does: maybe the end of the new track.
        [Test]
        public void APositionWrittenBack_WhileTheUserDoesntMoveTheSlider_DoesntSeek()
        {
            _viewModel.Position = 45;

            _player.DidNotReceive().Position = Arg.Any<TimeSpan>();
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
            _viewModel.IsDraggingPosition = true;
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

        // Netherworld, 138 BPM, after it came in at 128.
        [TestCase(129.4, 129.4 / 138, true)]
        [TestCase(137.6, 137.6 / 138, false)]
        [TestCase(138, 1, false)]
        public void OffItsOwnTempo_TheBeatsPerMinuteHeardShow(double heard, double tempo, bool isShown)
        {
            _player.BeatsPerMinute.Returns(heard);
            _player.Tempo.Returns(tempo);

            Assert.That(_viewModel.IsOffTempo, Is.EqualTo(isShown));
            Assert.That(_viewModel.BeatsPerMinute, Is.EqualTo(heard));
        }

        [Test]
        public void WithoutKnownBeats_NoBeatsPerMinuteShow()
        {
            _player.BeatsPerMinute.Returns((double?)null);
            _player.Tempo.Returns(0.95);

            Assert.That(_viewModel.IsOffTempo, Is.False);
        }

        [Test]
        public void AsTheTrackPlays_TheBeatsPerMinuteHeardFollow()
        {
            var changed = new List<string?>();
            _viewModel.PropertyChanged += (sender, e) => changed.Add(e.PropertyName);

            _player.PositionChanged += Raise.Event();

            Assert.That(changed, Does.Contain(nameof(PlayerViewModel.IsOffTempo)).And.Contain(nameof(PlayerViewModel.BeatsPerMinute)));
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
