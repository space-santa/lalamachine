using System;
using System.Threading;
using Lalamachine.ViewModel;
using LibVLCSharp.Shared;

namespace Lalamachine.Desktop.Services
{
    // LibVLC raises its events on its own threads, and calling back into it from there deadlocks.
    // So all events are passed on to the thread that created the player, i.e. the UI thread.
    internal sealed class VlcAudioPlayer : IAudioPlayer, IDisposable
    {
        private readonly SynchronizationContext _uiContext;
        private readonly LibVLC _libVlc;
        private readonly MediaPlayer _mediaPlayer;
        private readonly Timer _positionTimer;
        private double _volume = 1;
        private bool _isMuted;
        private bool _disposed;

        public VlcAudioPlayer()
        {
            _uiContext = SynchronizationContext.Current ?? new SynchronizationContext();

            Core.Initialize();
            _libVlc = new LibVLC("--no-video");
            _mediaPlayer = new MediaPlayer(_libVlc);
            _mediaPlayer.LengthChanged += (sender, e) => Post(() => MediaOpened?.Invoke(this, EventArgs.Empty));
            // VLC ignores Play and seeking on a track that ended until it is stopped.
            _mediaPlayer.EndReached += (sender, e) => Post(() =>
            {
                _mediaPlayer.Stop();
                MediaEnded?.Invoke(this, EventArgs.Empty);
            });
            _mediaPlayer.EncounteredError += (sender, e) => Post(() => MediaFailed?.Invoke(this, EventArgs.Empty));
            // VLC forgets volume and mute while it has no audio output, so set them again once a track plays.
            _mediaPlayer.Playing += (sender, e) => Post(ApplyVolume);

            // Like the WPF player, poll the position instead of handling every one of VLC's many TimeChanged events.
            _positionTimer = new Timer(_ => Post(() => PositionChanged?.Invoke(this, EventArgs.Empty)), null, 200, 200);
        }

        public event EventHandler? MediaOpened;
        public event EventHandler? MediaEnded;
        public event EventHandler? MediaFailed;
        public event EventHandler? PositionChanged;

        public Uri? Source { get; private set; }

        public bool IsMuted
        {
            get => _isMuted;
            set
            {
                _isMuted = value;
                ApplyVolume();
            }
        }

        public double Volume
        {
            get => _volume;
            set
            {
                _volume = value;
                ApplyVolume();
            }
        }

        public TimeSpan? Duration => _mediaPlayer.Length > 0 ? TimeSpan.FromMilliseconds(_mediaPlayer.Length) : null;

        // VLC reports -1 while nothing is loaded.
        public TimeSpan Position
        {
            get => _mediaPlayer.Time > 0 ? TimeSpan.FromMilliseconds(_mediaPlayer.Time) : TimeSpan.Zero;
            set => _mediaPlayer.Time = (long)value.TotalMilliseconds;
        }

        public void Open(Uri source)
        {
            Source = source;
            // The player keeps its own reference to the media.
            using var media = new Media(_libVlc, source);
            _mediaPlayer.Media = media;
        }

        public void Play() => _mediaPlayer.Play();

        public void Pause() => _mediaPlayer.SetPause(true);

        public void Dispose()
        {
            _disposed = true;
            _positionTimer.Dispose();
            _mediaPlayer.Stop();
            _mediaPlayer.Dispose();
            _libVlc.Dispose();
        }

        private void ApplyVolume()
        {
            _mediaPlayer.Volume = (int)Math.Round(_volume * 100);
            _mediaPlayer.Mute = _isMuted;
        }

        private void Post(Action action)
        {
            _uiContext.Post(_ =>
            {
                if (!_disposed)
                {
                    action();
                }
            }, null);
        }
    }
}
