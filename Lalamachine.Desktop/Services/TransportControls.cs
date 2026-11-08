#if WINDOWS
using System;
using LibLala.MediaControls;
using Windows.Media;
using Windows.Media.Playback;

namespace Lalamachine.Desktop.Services
{
    // Windows's controls: SystemMediaTransportControls have the buttons of headphones and the media keys, and show the track
    // in the media flyout next to the volume. They're a MediaPlayer's, one that never plays anything itself. Windows calls
    // back on a thread of its own.
    internal sealed class TransportControls : IMediaControls
    {
        private readonly MediaPlayer _mediaPlayer;
        private readonly SystemMediaTransportControls _controls;
        private bool _isDisposed;

        public TransportControls()
        {
            _mediaPlayer = new MediaPlayer();
            // Else the buttons would play and pause that player, not lalamachine.
            _mediaPlayer.CommandManager.IsEnabled = false;
            _controls = _mediaPlayer.SystemMediaTransportControls;
            _controls.IsPlayEnabled = true;
            _controls.IsPauseEnabled = true;
            _controls.IsNextEnabled = true;
            _controls.IsPreviousEnabled = true;
            _controls.ButtonPressed += OnButtonPressed;
        }

        public event EventHandler<MediaCommandEventArgs>? CommandReceived;

        public void Show(NowPlaying? nowPlaying)
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            var display = _controls.DisplayUpdater;
            if (nowPlaying is null)
            {
                display.ClearAll();
                display.Update();
                _controls.PlaybackStatus = MediaPlaybackStatus.Closed;
                return;
            }

            display.Type = MediaPlaybackType.Music;
            display.MusicProperties.Title = nowPlaying.Title;
            display.MusicProperties.Artist = nowPlaying.Artist;
            display.MusicProperties.AlbumTitle = nowPlaying.Album;
            display.Update();
            // Which of play and pause a headphone's one button for both is.
            _controls.PlaybackStatus = nowPlaying.IsPlaying ? MediaPlaybackStatus.Playing : MediaPlaybackStatus.Paused;
            _controls.UpdateTimelineProperties(new SystemMediaTransportControlsTimelineProperties
            {
                StartTime = TimeSpan.Zero,
                EndTime = nowPlaying.Duration,
                MinSeekTime = TimeSpan.Zero,
                MaxSeekTime = nowPlaying.Duration,
                Position = nowPlaying.Position,
            });
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }
            _isDisposed = true;
            _controls.ButtonPressed -= OnButtonPressed;
            _mediaPlayer.Dispose();
        }

        private void OnButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            MediaCommand? command = args.Button switch
            {
                SystemMediaTransportControlsButton.Play => MediaCommand.Play,
                SystemMediaTransportControlsButton.Pause => MediaCommand.Pause,
                SystemMediaTransportControlsButton.Next => MediaCommand.Next,
                SystemMediaTransportControlsButton.Previous => MediaCommand.Previous,
                _ => null,
            };
            if (command is { } pressed)
            {
                CommandReceived?.Invoke(this, new MediaCommandEventArgs(pressed));
            }
        }
    }
}
#endif
