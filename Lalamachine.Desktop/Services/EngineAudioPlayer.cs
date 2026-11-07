using System;
using System.IO;
using System.Threading;
using Lalamachine.ViewModel;
using LibLala.Audio;

namespace Lalamachine.Desktop.Services
{
    // Plays through lalamachine's own AudioEngine. Its events come on the output's thread and are passed on to the thread that
    // created the player, i.e. the UI thread.
    internal sealed class EngineAudioPlayer : IAudioPlayer, IDisposable
    {
        // While it plays, the position slider follows it this often.
        private const int PositionMilliseconds = 200;

        private readonly SynchronizationContext _uiContext;
        private readonly Func<string, IAudioDecoder> _openDecoder;
        private readonly Func<string, BeatGrid?> _findGrid;
        private readonly AudioEngine _engine;
        private readonly Timer _positionTimer;
        // The engine's number of the open track. Its events about an earlier one are late, the user started another.
        private int _track;
        // Of the track mixed in, until it takes over.
        private int _mixTrack;
        private Uri? _mixSource;
        private bool _isOpen;
        private bool _isTicking;
        private double _volume = 1;
        private bool _isMuted;
        private bool _disposed;

        // findGrid finds the beat grids of tracks, for the transitions. With clicks, they are heard.
        public EngineAudioPlayer(Func<IAudioRenderer, IAudioOutput> createOutput, Func<string, IAudioDecoder> openDecoder, Func<string, BeatGrid?> findGrid, bool clicks)
        {
            _uiContext = SynchronizationContext.Current ?? new SynchronizationContext();
            _openDecoder = openDecoder;
            _findGrid = findGrid;
            _engine = new AudioEngine(createOutput) { Clicks = clicks };
            _engine.TransitionDue += (sender, e) => PostForTrack(e.Track, () => TransitionDue?.Invoke(this, EventArgs.Empty));
            _engine.TookOver += (sender, e) => Post(() => TakeOver(e.Track));
            // Stopped, the output doesn't keep the Mac awake with silence.
            _engine.Ended += (sender, e) => PostForTrack(e.Track, () =>
            {
                Stop();
                MediaEnded?.Invoke(this, EventArgs.Empty);
            });
            _engine.Failed += (sender, e) => PostForTrack(e.Track, () =>
            {
                Stop();
                MediaFailed?.Invoke(this, EventArgs.Empty);
            });

            // Only while it plays, see Play.
            _positionTimer = new Timer(_ => Post(() => PositionChanged?.Invoke(this, EventArgs.Empty)), null, Timeout.Infinite, Timeout.Infinite);
        }

        public event EventHandler? MediaOpened;
        public event EventHandler? MediaEnded;
        public event EventHandler? MediaFailed;
        public event EventHandler? PositionChanged;
        public event EventHandler? TransitionDue;
        public event EventHandler? MixedIn;

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

        public TimeSpan? Duration => _isOpen ? ToTime(_engine.Length) : null;

        public bool Transitions
        {
            get => _engine.Transitions;
            set => _engine.Transitions = value;
        }

        public TimeSpan? IncomingPosition => _engine.IncomingPosition is { } frames ? ToTime(frames) : null;

        public TimeSpan? IncomingDuration => _engine.IncomingLength is { } frames ? ToTime(frames) : null;

        public TimeSpan Position
        {
            get => ToTime(_engine.Position);
            set
            {
                _engine.Seek((long)Math.Round(value.TotalSeconds * AudioFormat.SampleRate));
                // Paused, no tick shows where it went.
                if (!_isTicking)
                {
                    Post(() => PositionChanged?.Invoke(this, EventArgs.Empty));
                }
            }
        }

        public void Open(Uri source)
        {
            Source = source;
            _mixTrack = 0;
            SetTicking(false);
            // Before the decoder, which has to be given to the engine once it's open.
            var grid = source.IsFile ? _findGrid(source.LocalPath) : null;
            IAudioDecoder? decoder = null;
            try
            {
                decoder = source.IsFile ? _openDecoder(source.LocalPath) : null;
            }
            catch (IOException)
            {
            }

            _isOpen = decoder is not null;
            if (decoder is null)
            {
                _track = _engine.Close();
                // Later, like VLC: the caller plays right after opening, and the failure has to come after that.
                PostForTrack(_track, () => MediaFailed?.Invoke(this, EventArgs.Empty));
                return;
            }

            _track = _engine.Open(decoder, grid);
            PostForTrack(_track, () => MediaOpened?.Invoke(this, EventArgs.Empty));
        }

        // A track that can't be read now isn't mixed in. The one that plays goes on to its end, the next one is opened then
        // and fails or plays like any other.
        public bool MixTo(Uri source)
        {
            if (!source.IsFile)
            {
                return false;
            }
            var grid = _findGrid(source.LocalPath);
            IAudioDecoder decoder;
            try
            {
                decoder = _openDecoder(source.LocalPath);
            }
            catch (IOException)
            {
                return false;
            }

            try
            {
                if (_engine.MixTo(decoder, grid) is var track and > 0)
                {
                    _mixTrack = track;
                    _mixSource = source;
                    return true;
                }
            }
            catch (IOException)
            {
            }
            decoder.Dispose();
            return false;
        }

        public void Play()
        {
            _engine.Play();
            SetTicking(_isOpen);
        }

        public void Pause()
        {
            _engine.Pause();
            SetTicking(false);
        }

        public void Dispose()
        {
            _disposed = true;
            _positionTimer.Dispose();
            _engine.Dispose();
        }

        private static TimeSpan ToTime(long frames) => TimeSpan.FromSeconds((double)frames / AudioFormat.SampleRate);

        // Cubed, so the slider follows how loud it sounds rather than the gain: half way is about half as loud.
        private void ApplyVolume() => _engine.Volume = _isMuted ? 0 : (float)Math.Pow(Math.Clamp(_volume, 0, 1), 3);

        private void Stop()
        {
            _engine.Stop();
            SetTicking(false);
        }

        // Idle, the app doesn't wake up five times a second for nothing.
        private void SetTicking(bool isTicking)
        {
            if (isTicking != _isTicking)
            {
                _isTicking = isTicking;
                var period = isTicking ? PositionMilliseconds : Timeout.Infinite;
                _positionTimer.Change(period, period);
            }
        }

        // The track mixed in is the current one now.
        private void TakeOver(int track)
        {
            if (track != _mixTrack || _mixTrack == 0)
            {
                return;
            }
            _track = _mixTrack;
            _mixTrack = 0;
            Source = _mixSource;
            MixedIn?.Invoke(this, EventArgs.Empty);
        }

        // An event of the track mixed in can come before its TookOver: the engine raises that when the output played the end
        // of the transition, the others already when it rendered them. It's the current track then, first.
        private void PostForTrack(int track, Action action)
        {
            Post(() =>
            {
                TakeOver(track);
                if (track == _track)
                {
                    action();
                }
            });
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
