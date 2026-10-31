using System;
using System.Windows.Media;
using System.Windows.Threading;
using Lalamachine.ViewModel;

namespace Lalamachine.Wpf.Services
{
    internal sealed class WpfAudioPlayer : IAudioPlayer
    {
        private readonly MediaPlayer _mediaPlayer;
        private readonly DispatcherTimer _positionTimer;

        public WpfAudioPlayer()
        {
            _mediaPlayer = new MediaPlayer();
            _mediaPlayer.MediaOpened += (sender, e) => MediaOpened?.Invoke(this, EventArgs.Empty);
            _mediaPlayer.MediaEnded += (sender, e) => MediaEnded?.Invoke(this, EventArgs.Empty);
            _mediaPlayer.MediaFailed += (sender, e) => MediaFailed?.Invoke(this, EventArgs.Empty);

            // MediaPlayer has no event for this, so poll it.
            _positionTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 200) };
            _positionTimer.Tick += (sender, e) => PositionChanged?.Invoke(this, EventArgs.Empty);
            _positionTimer.Start();
        }

        public event EventHandler? MediaOpened;
        public event EventHandler? MediaEnded;
        public event EventHandler? MediaFailed;
        public event EventHandler? PositionChanged;

        public Uri? Source => _mediaPlayer.Source;

        public bool IsMuted
        {
            get => _mediaPlayer.IsMuted;
            set => _mediaPlayer.IsMuted = value;
        }

        public double Volume
        {
            get => _mediaPlayer.Volume;
            set => _mediaPlayer.Volume = value;
        }

        public TimeSpan? Duration => _mediaPlayer.NaturalDuration.HasTimeSpan ? _mediaPlayer.NaturalDuration.TimeSpan : null;

        public TimeSpan Position
        {
            get => _mediaPlayer.Position;
            set => _mediaPlayer.Position = value;
        }

        public void Open(Uri source) => _mediaPlayer.Open(source);
        public void Play() => _mediaPlayer.Play();
        public void Pause() => _mediaPlayer.Pause();
    }
}
