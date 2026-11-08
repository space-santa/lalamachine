using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using LibLala.MediaControls;
using NSubstitute;
using NUnit.Framework;
using TestHelpers;

namespace Lalamachine.ViewModel.Unittests
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "The teardown disposes them.")]
    public class MediaControlsViewModelTest
    {
        private IAudioPlayer _audioPlayer = null!;
        private PlayerViewModel _player = null!;
        private IMediaControls _controls = null!;
        private ManualTime _time = null!;
        private MediaControlsViewModel _viewModel = null!;
        private List<NowPlaying?> _shown = null!;
        private QueueingContext _ui = null!;

        [SetUp]
        public void SetUp()
        {
            _audioPlayer = Substitute.For<IAudioPlayer>();
            _audioPlayer.Duration.Returns(TimeSpan.FromSeconds(300));
            _player = new PlayerViewModel(_audioPlayer, Substitute.For<IDialogService>(), new PlayerSettings(new InMemorySettingsFile()));
            _controls = Substitute.For<IMediaControls>();
            _shown = [];
            _controls.Show(Arg.Do<NowPlaying?>(_shown.Add));
            _time = new ManualTime();
            _ui = new QueueingContext();
            // The view model takes the context it's made on for the UI thread.
            var previous = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(_ui);
            try
            {
                _viewModel = new MediaControlsViewModel(_player, _controls, _time);
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previous);
            }
        }

        [TearDown]
        public void TearDown()
        {
            _viewModel.Dispose();
        }

        [Test]
        public void BeforeATrack_NothingIsShown()
        {
            Assert.That(_shown, Is.EqualTo(new NowPlaying?[] { null }));
        }

        [Test]
        public void ATrackThatPlays_IsShown_ByItsFileNameWhenItHasNoTags()
        {
            PlayTrack("a");

            Assert.That(_shown[^1], Is.EqualTo(new NowPlaying("a", "", "", TimeSpan.FromSeconds(300), TimeSpan.Zero, IsPlaying: true)));
        }

        [Test]
        public void ATrackThatPlays_IsShown_WithItsTagsAndCover_TheSameCoverEachTime()
        {
            var path = Path.Combine(Path.GetTempPath(), $"lalatest-{Guid.NewGuid()}.mp3");
            try
            {
                TestMp3.Write(path, "Netherworld", "Somebody", "Underground");
                AddCover(path, [0xFF, 0xD8, 0xFF, 0xE0, 1]);

                PlayPath(path);
                var playing = _shown[^1];
                _player.Pause();
                var paused = _shown[^1];

                Assert.That((playing?.Title, playing?.Artist, playing?.Album), Is.EqualTo(("Netherworld", "Somebody", "Underground")));
                Assert.That(playing?.Cover, Is.EqualTo(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 1 }));
                Assert.That(paused?.Cover, Is.SameAs(playing?.Cover));
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Test]
        public void ATrackWithoutACover_IsShownWithout()
        {
            PlayTrack("a");

            Assert.That(_shown[^1]?.Cover, Is.Null);
        }

        [Test]
        public void Pause_IsShown_WhereTheTrackIs()
        {
            PlayTrack("a");
            _audioPlayer.Position.Returns(TimeSpan.FromSeconds(42));

            _player.Pause();

            Assert.That(_shown[^1], Is.EqualTo(new NowPlaying("a", "", "", TimeSpan.FromSeconds(300), TimeSpan.FromSeconds(42), IsPlaying: false)));
        }

        [Test]
        public void ATrackThatStarts_IsShownPausedThenPlaying_NotEachTimeThePlayerSaysTheSame()
        {
            _shown.Clear();

            PlayTrack("a");
            _audioPlayer.MediaOpened += Raise.Event();

            Assert.That(_shown.ConvertAll(shown => shown?.IsPlaying), Is.EqualTo(new bool?[] { false, true }));
        }

        [Test]
        public void ATrackThatFollows_IsShownOnce()
        {
            PlayTrack("a");
            _shown.Clear();

            PlayTrack("b");
            _audioPlayer.MediaOpened += Raise.Event();

            Assert.That(_shown, Is.EqualTo(new NowPlaying?[] { new NowPlaying("b", "", "", TimeSpan.FromSeconds(300), TimeSpan.Zero, IsPlaying: true) }));
        }

        [Test]
        public void WhileItPlaysOnAsCounted_ItIsntShownAgain_ForFiveSeconds()
        {
            PlayTrack("a");
            var count = _shown.Count;

            PlayOn(4);

            Assert.That(_shown, Has.Count.EqualTo(count));
        }

        [Test]
        public void WhileItPlays_ItIsShownWhereItIs_EveryFiveSeconds()
        {
            PlayTrack("a");
            var count = _shown.Count;

            PlayOn(10);

            Assert.That(_shown, Has.Count.EqualTo(count + 2));
            Assert.That(_shown[^1]?.Position, Is.EqualTo(TimeSpan.FromSeconds(10.5)));
        }

        [Test]
        public void ControlsThatFail_DontStopTheTrack()
        {
            _controls.When(controls => controls.Show(Arg.Any<NowPlaying?>())).Do(_ => throw new InvalidOperationException("The system's controls failed."));

            PlayTrack("a");

            _audioPlayer.Received(1).Play();
            Assert.That(_player.IsPlaying, Is.True);
        }

        [Test]
        public void ASeek_IsShown()
        {
            PlayTrack("a");
            var count = _shown.Count;

            _time.Advance(TimeSpan.FromSeconds(1));
            _audioPlayer.Position.Returns(TimeSpan.FromSeconds(120));
            _audioPlayer.PositionChanged += Raise.Event();

            Assert.That(_shown, Has.Count.EqualTo(count + 1));
            Assert.That(_shown[^1]?.Position, Is.EqualTo(TimeSpan.FromSeconds(120)));
        }

        [Test]
        public void WhilePaused_ItDoesntCountOn()
        {
            PlayTrack("a");
            _audioPlayer.Position.Returns(TimeSpan.FromSeconds(42));
            _player.Pause();
            var count = _shown.Count;

            _time.Advance(TimeSpan.FromSeconds(10));
            _audioPlayer.PositionChanged += Raise.Event();

            Assert.That(_shown, Has.Count.EqualTo(count));
        }

        [Test]
        public void TogglePlayPause_PausesWhatPlays_AndPlaysItAgain()
        {
            PlayTrack("a");

            Press(MediaCommand.TogglePlayPause);
            Assert.That(_player.IsPlaying, Is.False);
            _audioPlayer.Received(1).Pause();

            Press(MediaCommand.TogglePlayPause);
            Assert.That(_player.IsPlaying, Is.True);
        }

        [Test]
        public void Play_WhilePlaying_AndPause_WhilePaused_ChangeNothing()
        {
            PlayTrack("a");
            _audioPlayer.ClearReceivedCalls();

            Press(MediaCommand.Play);
            Assert.That(_player.IsPlaying, Is.True);
            _player.Pause();
            _audioPlayer.ClearReceivedCalls();
            Press(MediaCommand.Pause);

            Assert.That(_player.IsPlaying, Is.False);
            _audioPlayer.DidNotReceive().Play();
            _audioPlayer.DidNotReceive().Pause();
        }

        [Test]
        public void Play_WhilePaused_Plays()
        {
            PlayTrack("a");
            _player.Pause();

            Press(MediaCommand.Play);

            Assert.That(_player.IsPlaying, Is.True);
        }

        [Test]
        public void NextAndPrevious_AreThePlayersButtons()
        {
            var next = 0;
            var previous = 0;
            _player.PlayNextTrackEvent += (sender, e) => next++;
            _player.PlayLastTrackEvent += (sender, e) => previous++;

            Press(MediaCommand.Next);
            Press(MediaCommand.Previous);

            Assert.That((next, previous), Is.EqualTo((1, 1)));
        }

        [Test]
        public void Seek_GoesThere_AndIsShown()
        {
            PlayTrack("a");

            Press(MediaCommand.Seek, TimeSpan.FromSeconds(120));

            Assert.That(_audioPlayer.Position, Is.EqualTo(TimeSpan.FromSeconds(120)));
            Assert.That(_shown[^1]?.Position, Is.EqualTo(TimeSpan.FromSeconds(120)));
        }

        [Test]
        public void Seek_PastTheEnd_GoesToTheEnd()
        {
            PlayTrack("a");

            Press(MediaCommand.Seek, TimeSpan.FromSeconds(301));

            Assert.That(_audioPlayer.Position, Is.EqualTo(TimeSpan.FromSeconds(300)));
        }

        [Test]
        public void Seek_WhileTheLengthIsntKnown_DoesNothing()
        {
            PlayTrack("a");
            _audioPlayer.Duration.Returns((TimeSpan?)null);
            _audioPlayer.ClearReceivedCalls();

            Press(MediaCommand.Seek, TimeSpan.FromSeconds(120));

            _audioPlayer.DidNotReceive().Position = Arg.Any<TimeSpan>();
        }

        [Test]
        public void Seek_WithoutATrack_DoesNothing()
        {
            Press(MediaCommand.Seek, TimeSpan.FromSeconds(120));

            _audioPlayer.DidNotReceive().Position = Arg.Any<TimeSpan>();
        }

        [Test]
        public void ACommand_IsHandledOnTheUIThread()
        {
            PlayTrack("a");

            _controls.CommandReceived += Raise.EventWith(new MediaCommandEventArgs(MediaCommand.Pause));
            Assert.That(_player.IsPlaying, Is.True);
            _ui.RunAll();

            Assert.That(_player.IsPlaying, Is.False);
        }

        [Test]
        public void ACommand_OnItsWayToTheUIThreadWhenDisposed_IsDropped()
        {
            PlayTrack("a");

            _controls.CommandReceived += Raise.EventWith(new MediaCommandEventArgs(MediaCommand.Pause));
            _viewModel.Dispose();
            _ui.RunAll();

            Assert.That(_player.IsPlaying, Is.True);
        }

        [Test]
        public void Dispose_DisposesTheControls_AndLetsGoOfThePlayer()
        {
            PlayTrack("a");
            _viewModel.Dispose();
            _shown.Clear();

            _player.Pause();

            _controls.Received(1).Dispose();
            Assert.That(_shown, Is.Empty);
        }

        // On the system's thread, then on the UI thread.
        private void Press(MediaCommand command, TimeSpan position = default)
        {
            _controls.CommandReceived += Raise.EventWith(new MediaCommandEventArgs(command, position));
            _ui.RunAll();
        }

        // A second at a time, half a second ahead of where it started.
        private void PlayOn(int seconds)
        {
            for (var second = 1; second <= seconds; second++)
            {
                _time.Advance(TimeSpan.FromSeconds(1));
                _audioPlayer.Position.Returns(TimeSpan.FromSeconds(second + 0.5));
                _audioPlayer.PositionChanged += Raise.Event();
            }
        }

        // One that isn't there.
        private void PlayTrack(string name) => PlayPath(Path.GetFullPath($"/music/{name}.mp3"));

        private void PlayPath(string path)
        {
            _audioPlayer.Source.Returns(new Uri(path));
            _player.PlayTrackHandler(this, new PlayTrackEventArgs(path, "MAIN"));
        }

        private static void AddCover(string path, byte[] picture)
        {
            using var file = TagLib.File.Create(path);
            var id3 = (TagLib.Id3v2.Tag)file.GetTag(TagLib.TagTypes.Id3v2, create: true);
            id3.AddFrame(new TagLib.Id3v2.AttachmentFrame(new TagLib.Picture(new TagLib.ByteVector(picture)) { Type = TagLib.PictureType.FrontCover, MimeType = "image/jpeg" }));
            file.Save();
        }

        private sealed class ManualTime : TimeProvider
        {
            private long _timestamp;

            public override long TimestampFrequency => TimeSpan.TicksPerSecond;

            public override long GetTimestamp() => _timestamp;

            public void Advance(TimeSpan time) => _timestamp += time.Ticks;
        }

        // Like the UI thread: what's posted runs when it gets to it.
        private sealed class QueueingContext : SynchronizationContext
        {
            private readonly Queue<(SendOrPostCallback Callback, object? State)> _posted = new();

            public override void Post(SendOrPostCallback d, object? state) => _posted.Enqueue((d, state));

            public void RunAll()
            {
                while (_posted.TryDequeue(out var posted))
                {
                    posted.Callback(posted.State);
                }
            }
        }
    }
}
