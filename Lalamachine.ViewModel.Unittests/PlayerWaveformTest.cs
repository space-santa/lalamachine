using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using LibLala.Audio.Analysis;
using LibLala.DomainPrimitives;
using LibLala.LibLalaTagReader;
using NSubstitute;
using NUnit.Framework;

namespace Lalamachine.ViewModel.Unittests
{
    // The waveform of the track that plays, shown as its position slider.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "The teardown disposes them.")]
    public class PlayerWaveformTest
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);
        // How long the tracks are, in seconds, to tell their waveforms apart. One that isn't here can't be read.
        private static readonly Dictionary<string, double> Seconds = new() { ["a"] = 1, ["b"] = 2, ["c"] = 3 };

        private string _dir = "";
        private SynchronizationContext? _previousContext;
        private PostedActions _ui = null!;
        private IAudioPlayer _player = null!;
        private WaveformReader _waveforms = null!;
        private PlayerViewModel _viewModel = null!;
        private PlaylistBaseViewModel _list = null!;
        private ConcurrentQueue<string> _opened = null!;
        private ManualResetEventSlim _gateOfA = null!;
        // Of a track that can't be read: it fails when the gate opens.
        private ManualResetEventSlim _gateOfGone = null!;
        // What the analysis stored, and what the player stored of the tracks it read.
        private ConcurrentDictionary<string, Waveform> _stored = null!;

        [SetUp]
        public void SetUp()
        {
            // Files, for the stamps that a waveform is stored with.
            _dir = Path.Combine(Path.GetTempPath(), $"lalatest-{Guid.NewGuid()}");
            Directory.CreateDirectory(_dir);
            foreach (var name in Seconds.Keys)
            {
                File.WriteAllBytes(PathOf(name), [1]);
            }

            // The view model is on the UI thread, which runs what's posted to it when the test says so.
            _previousContext = SynchronizationContext.Current;
            _ui = new PostedActions();
            SynchronizationContext.SetSynchronizationContext(_ui);

            _opened = new ConcurrentQueue<string>();
            _gateOfA = new ManualResetEventSlim(initialState: true);
            _gateOfGone = new ManualResetEventSlim(initialState: true);
            _stored = new ConcurrentDictionary<string, Waveform>();
            _waveforms = new WaveformReader(
                path =>
                {
                    _opened.Enqueue(path);
                    var name = Path.GetFileNameWithoutExtension(path);
                    if (!Seconds.TryGetValue(name, out var seconds))
                    {
                        _gateOfGone.Wait();
                        throw new IOException("No such file.");
                    }
                    return new ToneDecoder(seconds) { Gate = name == "a" ? _gateOfA : null };
                },
                path => _stored.GetValueOrDefault(path),
                (path, stamp, waveform) => _stored[path] = waveform);
            _player = Substitute.For<IAudioPlayer>();
            _player.MixTo(Arg.Any<Uri>()).Returns(true);
            _viewModel = new PlayerViewModel(_player, Substitute.For<IDialogService>(), new PlayerSettings(new InMemorySettingsFile()), _waveforms);
            // Wired like the queue in the main window.
            _list = new PlaylistBaseViewModel("MAIN", Substitute.For<IDialogService>(), Substitute.For<ITrackPlaylists>());
            foreach (var name in new[] { "a", "b", "c" })
            {
                _list.AddTrack(new LibLalaTagsBuilder(name, new UncheckedTrackPath(PathOf(name)), 5).Build());
            }
            _viewModel.PeekNextTrackEvent += _list.PeekNextTrackHandler;
            _list.PlayTrackEvent += _viewModel.PlayTrackHandler;
        }

        [TearDown]
        public void TearDown()
        {
            _gateOfA.Set();
            _gateOfGone.Set();
            _waveforms.Dispose();
            _gateOfA.Dispose();
            _gateOfGone.Dispose();
            SynchronizationContext.SetSynchronizationContext(_previousContext);
            Directory.Delete(_dir, recursive: true);
        }

        [Test]
        public void TheWaveformOfTheTrackThatPlays_IsShown_OnceItIsRead()
        {
            Play("a");

            Assert.That(ShownSeconds(), Is.EqualTo(1));
        }

        [Test]
        public void TheWaveformOfTheTrackBefore_IsGone_WhileTheNextOneIsRead()
        {
            Play("b");
            Assert.That(ShownSeconds(), Is.EqualTo(2));
            _gateOfA.Reset();

            Play("a");

            Assert.That(_viewModel.Waveform, Is.Null);
        }

        [Test]
        public void AWaveformRead_AfterAnotherTrackStarted_IsntShown()
        {
            _gateOfA.Reset();
            Play("a");
            Play("b");
            Assert.That(ShownSeconds(), Is.EqualTo(2));

            _gateOfA.Set();
            Assert.That(_waveforms.Read(PathOf("a")).Wait(Timeout), Is.True);
            SpinWait.SpinUntil(() =>
            {
                _ui.Run();
                return false;
            }, TimeSpan.FromMilliseconds(200));

            Assert.That(SecondsOf(_viewModel), Is.EqualTo(2));
        }

        [Test]
        public void ATrackPlayedBefore_IsntReadFromItsFileAgain()
        {
            foreach (var name in new[] { "a", "b", "c" })
            {
                Play(name);
                Assert.That(ShownSeconds(), Is.EqualTo(Seconds[name]));
            }

            Play("a");

            Assert.That(ShownSeconds(), Is.EqualTo(1));
            Assert.That(_opened, Is.EqualTo(new[] { PathOf("a"), PathOf("b"), PathOf("c") }));
        }

        [Test]
        public void ATrackTheAnalysisGotTo_IsntReadFromItsFile()
        {
            _stored[PathOf("a")] = Waveform.FromBytes(new byte[20]);

            Play("a");

            Assert.That(ShownSeconds(), Is.EqualTo(1));
            Assert.That(_opened, Is.Empty);
        }

        [Test]
        public void TheTrackMixedIn_IsReadBeforeItTakesOver_AndShownAtOnce()
        {
            _player.Source.Returns(UriOf("a"));
            _list.PlayTrack(_list.Playlist[0]);
            Assert.That(ShownSeconds(), Is.EqualTo(1));

            _player.TransitionDue += Raise.Event();
            Assert.That(SpinWait.SpinUntil(() => _opened.Contains(PathOf("b")), Timeout), Is.True);
            Assert.That(_waveforms.Read(PathOf("b")).Wait(Timeout), Is.True);
            _player.Source.Returns(UriOf("b"));
            _player.MixedIn += Raise.Event();

            Assert.That(SecondsOf(_viewModel), Is.EqualTo(2));
            Assert.That(_opened, Has.Exactly(1).EqualTo(PathOf("b")));
        }

        [Test]
        public void ATrackThatCantBeRead_ShowsNoWaveform()
        {
            Play("a");
            Assert.That(ShownSeconds(), Is.EqualTo(1));

            _gateOfGone.Reset();
            Play("gone");
            _gateOfGone.Set();
            // Till what comes after the read is run.
            Assert.That(SpinWait.SpinUntil(() => _ui.Run() > 0, Timeout), Is.True);

            Assert.That(_opened, Does.Contain(PathOf("gone")));
            Assert.That(_viewModel.Waveform, Is.Null);
        }

        // Like the list does: the player opens it and plays.
        private void Play(string name)
        {
            _player.Source.Returns(UriOf(name));
            _viewModel.PlayTrackHandler(this, new PlayTrackEventArgs(PathOf(name), "MAIN"));
        }

        // Of the waveform shown once one is, with what's posted to the UI thread run.
        private double? ShownSeconds()
        {
            SpinWait.SpinUntil(() =>
            {
                _ui.Run();
                return _viewModel.Waveform is not null;
            }, Timeout);
            return SecondsOf(_viewModel);
        }

        private static double? SecondsOf(PlayerViewModel viewModel) => viewModel.Waveform is { } waveform ? Math.Round(waveform.SliceCount * Waveform.SliceSeconds, 3) : null;

        private string PathOf(string name) => Path.Combine(_dir, $"{name}.mp3");

        private Uri UriOf(string name) => new(PathOf(name));
    }
}
