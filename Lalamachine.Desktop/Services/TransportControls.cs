#if WINDOWS
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using LibLala.MediaControls;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Storage.Streams;

namespace Lalamachine.Desktop.Services
{
    // Windows's controls: SystemMediaTransportControls have the buttons of headphones and the media keys, and show the track
    // in the media flyout next to the volume. They're a MediaPlayer's, one that never plays anything itself. Windows calls
    // back on a thread of its own.
    internal sealed class TransportControls : IMediaControls
    {
        private readonly MediaPlayer _mediaPlayer;
        private readonly SystemMediaTransportControls _controls;
        // The cover that the thumbnail is made of, and the stream that Windows reads it from.
        private byte[]? _cover;
        private InMemoryRandomAccessStream? _coverStream;
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
            _controls.PlaybackPositionChangeRequested += OnPlaybackPositionChangeRequested;
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
                SetCover(null);
                return;
            }

            display.Type = MediaPlaybackType.Music;
            display.MusicProperties.Title = nowPlaying.Title;
            display.MusicProperties.Artist = nowPlaying.Artist;
            display.MusicProperties.AlbumTitle = nowPlaying.Album;
            // Made once for each track, not at each play, pause and seek.
            if (!ReferenceEquals(nowPlaying.Cover, _cover))
            {
                SetCover(nowPlaying.Cover);
                display.Thumbnail = _coverStream is null ? null : RandomAccessStreamReference.CreateFromStream(_coverStream);
            }
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
            _controls.PlaybackPositionChangeRequested -= OnPlaybackPositionChangeRequested;
            _mediaPlayer.Dispose();
            SetCover(null);
        }

        // Windows reads the picture from the stream when it shows it, so the stream stays open until the next cover. A
        // stream of Windows's own: it reads from a copy of it, which a .NET stream made into one can't make.
        private void SetCover(byte[]? cover)
        {
            _cover = cover;
            _coverStream?.Dispose();
            _coverStream = null;
            if (cover is null)
            {
                return;
            }
            _coverStream = new InMemoryRandomAccessStream();
            // Into memory, there's nothing that it would wait for on the UI thread.
            _coverStream.WriteAsync(cover.AsBuffer()).AsTask().GetAwaiter().GetResult();
            _coverStream.Seek(0);
        }

        private void OnPlaybackPositionChangeRequested(SystemMediaTransportControls sender, PlaybackPositionChangeRequestedEventArgs args)
        {
            CommandReceived?.Invoke(this, new MediaCommandEventArgs(MediaCommand.Seek, args.RequestedPlaybackPosition));
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
