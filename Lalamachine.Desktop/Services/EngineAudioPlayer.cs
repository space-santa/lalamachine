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
            _engine.TookOver += (sender, e) => Post(() =>
            {
                if (e.Track != _mixTrack || _mixTrack == 0)
                {
                    return;
                }
                _track = _mixTrack;
                _mixTrack = 0;
                Source = _mixSource;
                // Not MediaOpened as well: its new duration before the new position would let the slider clamp the old
                // position to a shorter track's length and seek there.
                MixedIn?.Invoke(this, EventArgs.Empty);
            });
            // Stopped, the output doesn't keep the Mac awake with silence.
            _engine.Ended += (sender, e) => PostForTrack(e.Track, () =>
            {
                _engine.Stop();
                MediaEnded?.Invoke(this, EventArgs.Empty);
            });
            _engine.Failed += (sender, e) => PostForTrack(e.Track, () =>
            {
                _engine.Stop();
                MediaFailed?.Invoke(this, EventArgs.Empty);
            });

            _positionTimer = new Timer(_ => Post(() => PositionChanged?.Invoke(this, EventArgs.Empty)), null, 200, 200);
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

        public TimeSpan Position
        {
            get => ToTime(_engine.Position);
            set => _engine.Seek((long)Math.Round(value.TotalSeconds * AudioFormat.SampleRate));
        }

        public void Open(Uri source)
        {
            Source = source;
            _mixTrack = 0;
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

            _track = _engine.Open(decoder, _findGrid(source.LocalPath));
            PostForTrack(_track, () => MediaOpened?.Invoke(this, EventArgs.Empty));
        }

        public void MixTo(Uri source)
        {
            IAudioDecoder? decoder = null;
            try
            {
                decoder = source.IsFile ? _openDecoder(source.LocalPath) : null;
                if (decoder is not null && _engine.MixTo(decoder, _findGrid(source.LocalPath)) is var track and > 0)
                {
                    _mixTrack = track;
                    _mixSource = source;
                    return;
                }
            }
            catch (IOException)
            {
            }
            decoder?.Dispose();

            // Can't be mixed in, then like any other: it fails or plays, and takes over at once.
            Open(source);
            Play();
            Post(() => MixedIn?.Invoke(this, EventArgs.Empty));
        }

        public void Play() => _engine.Play();

        public void Pause() => _engine.Pause();

        public void Dispose()
        {
            _disposed = true;
            _positionTimer.Dispose();
            _engine.Dispose();
        }

        private static TimeSpan ToTime(long frames) => TimeSpan.FromSeconds((double)frames / AudioFormat.SampleRate);

        // Cubed, so the slider follows how loud it sounds rather than the gain: half way is about half as loud.
        private void ApplyVolume() => _engine.Volume = _isMuted ? 0 : (float)Math.Pow(Math.Clamp(_volume, 0, 1), 3);

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

        private void PostForTrack(int track, Action action)
        {
            Post(() =>
            {
                if (track == _track)
                {
                    action();
                }
            });
        }
    }
}
