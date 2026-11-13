using System;
using System.ComponentModel;
using System.Threading;
using LibLala.MediaControls;

namespace Lalamachine.ViewModel
{
    // Lets the operating system's media controls play, pause, skip and seek, like the buttons of Bluetooth headphones and the
    // Control Center. For that, the system has to know what plays, so it's told about each track with its cover, play and
    // pause, and seek.
    public sealed class MediaControlsViewModel : IDisposable
    {
        // macOS counts on from the position it was told. Off by more than this, it's told again: after a seek, or when the
        // track played at another tempo after a transition.
        private static readonly TimeSpan MaxDrift = TimeSpan.FromSeconds(1);
        // Windows doesn't count on, its media session stays at the position it was told. Microsoft recommends telling it
        // where the track is about this often while it plays.
        private static readonly TimeSpan PlayingRefresh = TimeSpan.FromSeconds(5);

        private readonly PlayerViewModel _player;
        private readonly IMediaControls _controls;
        private readonly TimeProvider _time;
        // The commands come on the system's thread. Without a context, like in the tests, they're handled right away.
        private readonly SynchronizationContext? _uiContext;
        private NowPlaying? _shown;
        private long _shownAt;
        private bool _isDisposed;

        // Owns the controls, disposes them.
        public MediaControlsViewModel(PlayerViewModel player, IMediaControls controls) : this(player, controls, TimeProvider.System)
        {
        }

        public MediaControlsViewModel(PlayerViewModel player, IMediaControls controls, TimeProvider time)
        {
            ArgumentNullException.ThrowIfNull(player);
            ArgumentNullException.ThrowIfNull(controls);
            _player = player;
            _controls = controls;
            _time = time;
            _uiContext = SynchronizationContext.Current;
            _player.PropertyChanged += Player_PropertyChanged;
            _controls.CommandReceived += Controls_CommandReceived;
            Show(Current());
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }
            _isDisposed = true;
            _player.PropertyChanged -= Player_PropertyChanged;
            _controls.CommandReceived -= Controls_CommandReceived;
            _controls.Dispose();
        }

        private void Player_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(PlayerViewModel.Track):
                case nameof(PlayerViewModel.IsPlaying):
                case nameof(PlayerViewModel.Duration):
                // A few times a second.
                case nameof(PlayerViewModel.Position):
                    var now = Current();
                    if (!IsShown(now))
                    {
                        Show(now);
                    }
                    break;
                default:
                    break;
            }
        }

        private void Controls_CommandReceived(object? sender, MediaCommandEventArgs e)
        {
            if (_uiContext is null)
            {
                Execute(e);
            }
            else
            {
                _uiContext.Post(_ => Execute(e), null);
            }
        }

        private void Execute(MediaCommandEventArgs command)
        {
            // It can have been on its way to the UI thread.
            if (_isDisposed)
            {
                return;
            }
            switch (command.Command)
            {
                case MediaCommand.Play:
                    if (!_player.IsPlaying)
                    {
                        _player.Play();
                    }
                    break;
                case MediaCommand.Pause:
                    if (_player.IsPlaying)
                    {
                        _player.Pause();
                    }
                    break;
                case MediaCommand.TogglePlayPause:
                    _player.ChangePlayPauseCommand.Execute(null);
                    break;
                case MediaCommand.Next:
                    _player.NextCommand.Execute(null);
                    break;
                case MediaCommand.Previous:
                    _player.LastCommand.Execute(null);
                    break;
                case MediaCommand.Seek:
                    _player.Seek(command.Position);
                    break;
                default:
                    break;
            }
        }

        private NowPlaying? Current()
        {
            if (_player.Source.Length == 0)
            {
                return null;
            }
            var track = _player.Track;
            return new NowPlaying(track.Title, track.Artist, track.Album, TimeSpan.FromSeconds(_player.Duration), TimeSpan.FromSeconds(_player.Position), _player.IsPlaying, track.Cover);
        }

        // Whether the system shows it already: the player says the same again, like the duration when the track opened, or
        // the position where the system counted on to.
        private bool IsShown(NowPlaying? now)
        {
            if (now is null || _shown is null)
            {
                return now is null && _shown is null;
            }
            if (now with { Position = _shown.Position } != _shown)
            {
                return false;
            }
            if (!_shown.IsPlaying)
            {
                return (now.Position - _shown.Position).Duration() <= MaxDrift;
            }
            var elapsed = _time.GetElapsedTime(_shownAt);
            return elapsed < PlayingRefresh && (now.Position - (_shown.Position + elapsed)).Duration() <= MaxDrift;
        }

        // Kept as shown when the controls fail too: they're told again at the next change, not at each tick of the position.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "The system's controls are an extra. When they fail, like a WinRT or Objective-C call that throws, the track plays on: this runs while the player changes tracks.")]
        private void Show(NowPlaying? nowPlaying)
        {
            _shown = nowPlaying;
            _shownAt = _time.GetTimestamp();
            try
            {
                _controls.Show(nowPlaying);
            }
            catch (Exception)
            {
            }
        }
    }
}
