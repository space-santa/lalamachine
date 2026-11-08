using System;
using System.ComponentModel;
using System.Threading;
using LibLala.LibLalaTagReader;
using LibLala.MediaControls;

namespace Lalamachine.ViewModel
{
    // Lets the operating system's media controls play, pause and skip, like the buttons of Bluetooth headphones. For that,
    // the system has to know what plays, so it's told about each track, play and pause, and seek.
    public sealed class MediaControlsViewModel : IDisposable
    {
        // The system counts on from the position it was told. Off by more than this, it's told again: after a seek, or when
        // the track played at another tempo after a transition.
        private static readonly TimeSpan MaxDrift = TimeSpan.FromSeconds(1);

        private readonly PlayerViewModel _player;
        private readonly IMediaControls _controls;
        private readonly TimeProvider _time;
        // The commands come on the system's thread. Without a context, like in the tests, they're handled right away.
        private readonly SynchronizationContext? _uiContext;
        private string _title = "";
        private string _artist = "";
        private string _album = "";
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
            ReadTags();
            Show();
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
                case nameof(PlayerViewModel.Source):
                    ReadTags();
                    Show();
                    break;
                case nameof(PlayerViewModel.IsPlaying):
                case nameof(PlayerViewModel.Duration):
                    Show();
                    break;
                // A few times a second.
                case nameof(PlayerViewModel.Position):
                    if (HasDrifted())
                    {
                        Show();
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
                Execute(e.Command);
            }
            else
            {
                _uiContext.Post(_ => Execute(e.Command), null);
            }
        }

        private void Execute(MediaCommand command)
        {
            // It can have been on its way to the UI thread.
            if (_isDisposed)
            {
                return;
            }
            switch (command)
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
                default:
                    break;
            }
        }

        // Like the title in the player: a track without tags by its file name.
        private void ReadTags()
        {
            var source = _player.Source;
            (_title, _artist, _album) = ("", "", "");
            if (source.Length == 0)
            {
                return;
            }
            try
            {
                var tags = new TagReader().Read(source);
                (_title, _artist, _album) = (tags.Title, tags.ArtistString, tags.Album);
            }
            catch (TagReaderException)
            {
            }
            if (_title.Length == 0)
            {
                _title = System.IO.Path.GetFileNameWithoutExtension(source);
            }
        }

        private void Show()
        {
            _shown = _player.Source.Length == 0 ? null : new NowPlaying(_title, _artist, _album, TimeSpan.FromSeconds(_player.Duration), TimeSpan.FromSeconds(_player.Position), _player.IsPlaying);
            _shownAt = _time.GetTimestamp();
            _controls.Show(_shown);
        }

        private bool HasDrifted()
        {
            if (_shown is null)
            {
                return _player.Source.Length > 0;
            }
            var counted = _shown.IsPlaying ? _shown.Position + _time.GetElapsedTime(_shownAt) : _shown.Position;
            return (TimeSpan.FromSeconds(_player.Position) - counted).Duration() > MaxDrift;
        }
    }
}
