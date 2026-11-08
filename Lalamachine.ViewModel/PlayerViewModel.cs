using System;
using System.Windows.Input;
using LibLala.LibLalaTagReader;
using Settings;

namespace Lalamachine.ViewModel
{
    public class PlayerSettings : AppSettingsBase
    {
        public PlayerSettings() : this(new SettingsFile("Lalamachine.Desktop", "PlayerSettings")) { }

        public PlayerSettings(SettingsFile file) : base(file) { }

        public double Volume { get => GetDouble(50); set => Set(value); }
        public bool IsMuted { get => GetBool(false); set => Set(value); }
        public string LastActivePlaylist { get => GetString("MAIN"); set => Set(value); }
        public bool Transitions { get => GetBool(true); set => Set(value); }
    }

    public class ManualLoadEventArgs : EventArgs
    {
        public ManualLoadEventArgs(string path)
        {
            Path = path;
        }

        public string Path { get; set; }
    }

    public class PlayerViewModel : BaseNotifyPropertyChanged
    {
        private readonly IAudioPlayer _audioPlayer;
        private readonly IDialogService _dialogs;
        private readonly PlayerSettings _playerSettings;
        // The track mixed in and the list it's from, until it takes over. Then the list is told it went on to it.
        private NextTrackEventArgs? _mixing;
        // While the list is told: it plays already.
        private bool _isTakingOver;
        private bool _isDraggingPosition;
        // The track changed while the position slider was held: where it's dragged to is of the track before.
        private bool _isDragOfTrackBefore;

        public PlayerViewModel(IAudioPlayer audioPlayer, IDialogService dialogs) : this(audioPlayer, dialogs, new PlayerSettings())
        {
        }

        public PlayerViewModel(IAudioPlayer audioPlayer, IDialogService dialogs, PlayerSettings settings)
        {
            _playerSettings = settings;
            _audioPlayer = audioPlayer;
            _dialogs = dialogs;
            IsMuted = _playerSettings.IsMuted;
            Volume = _playerSettings.Volume;
            _audioPlayer.Transitions = _playerSettings.Transitions;
            _audioPlayer.MediaOpened += _audioPlayer_MediaOpened;
            _audioPlayer.MediaEnded += _audioPlayer_MediaEnded;
            _audioPlayer.MediaFailed += _audioPlayer_MediaFailed;
            _audioPlayer.PositionChanged += _audioPlayer_PositionChanged;
            _audioPlayer.TransitionDue += _audioPlayer_TransitionDue;
            _audioPlayer.MixedIn += _audioPlayer_MixedIn;

            _changePlayPauseCommand = new DelegateCommand(OnChangePlayPause);
            _loadCommand = new DelegateCommand(OnLoad);
            _nextCommand = new DelegateCommand(OnPlayNextTrack);
            _lastCommand = new DelegateCommand(OnPlayLastTrack);
            _muteCommand = new DelegateCommand(OnMute);
        }

        public event EventHandler? PlayNextTrackEvent;
        public event EventHandler? PlayNextPlaylistTrackEvent;
        public event EventHandler? PlayNextLibraryTrackEvent;
        protected virtual void InvokePlayNextTrackEvent()
        {
            if (LastActivePlaylist == "MAIN")
            {
                PlayNextTrackEvent?.Invoke(this, EventArgs.Empty);
            }
            else if (LastActivePlaylist == PlaylistsViewModel.ListName)
            {
                PlayNextPlaylistTrackEvent?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                PlayNextLibraryTrackEvent?.Invoke(this, EventArgs.Empty);
            }
        }
        protected virtual void OnPlayNextTrack(object? commandParameter)
        {
            // During a transition, the track that comes in. Asked again, a list in shuffle mode would say another one.
            if (_mixing is { Track: { } track, List: { } list } && list.PlayTrack(track))
            {
                return;
            }
            InvokePlayNextTrackEvent();
        }

        // The list that plays says what it would play next, without going on to it.
        public event EventHandler<NextTrackEventArgs>? PeekNextTrackEvent;
        public event EventHandler<NextTrackEventArgs>? PeekNextPlaylistTrackEvent;
        public event EventHandler<NextTrackEventArgs>? PeekNextLibraryTrackEvent;
        private NextTrackEventArgs PeekNextTrack()
        {
            var next = new NextTrackEventArgs();
            if (LastActivePlaylist == "MAIN")
            {
                PeekNextTrackEvent?.Invoke(this, next);
            }
            else if (LastActivePlaylist == PlaylistsViewModel.ListName)
            {
                PeekNextPlaylistTrackEvent?.Invoke(this, next);
            }
            else
            {
                PeekNextLibraryTrackEvent?.Invoke(this, next);
            }
            return next;
        }

        public event EventHandler? PlayLastTrackEvent;
        public event EventHandler? PlayLastPlaylistTrackEvent;
        public event EventHandler? PlayLastLibraryTrackEvent;
        protected virtual void OnPlayLastTrack(object? commandParameter)
        {
            if (LastActivePlaylist == "MAIN")
            {
                PlayLastTrackEvent?.Invoke(this, EventArgs.Empty);
            }
            else if (LastActivePlaylist == PlaylistsViewModel.ListName)
            {
                PlayLastPlaylistTrackEvent?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                PlayLastLibraryTrackEvent?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler<ManualLoadEventArgs>? ManualLoadEvent;
        protected virtual void OnManualLoad(ManualLoadEventArgs e)
        {
            ManualLoadEvent?.Invoke(this, e);
        }

        public void PlayTrackHandler(object? sender, PlayTrackEventArgs e)
        {
            if (e is null)
            {
                return;
            }

            LastActivePlaylist = e.ListName;
            if (_isTakingOver)
            {
                return;
            }

            // A track that's started ends the mixing.
            _mixing = null;
            Source = e.Path;
            Play();
        }

        public string Source
        {
            // LocalPath, not AbsolutePath: AbsolutePath drops the server of a network path (\\nas\music\a.mp3 becomes /music/a.mp3).
            get => _audioPlayer.Source?.LocalPath ?? "";
            set => Open(value);
        }

        // What's shown of the track that plays, in the player and in the system's media controls. Read once for each track.
        public TrackInfo Track { get; private set; } = TrackInfo.None;

        public string LastActivePlaylist
        {
            get => _playerSettings.LastActivePlaylist;
            set => _playerSettings.LastActivePlaylist = value;
        }

        private bool _isPlaying;
        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                if (Source.Length > 0)
                {
                    _isPlaying = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public bool IsMuted
        {
            get => _audioPlayer.IsMuted;
            set
            {
                _audioPlayer.IsMuted = value;
                _playerSettings.IsMuted = _audioPlayer.IsMuted;
                NotifyPropertyChanged();
            }
        }

        public double Volume
        {
            get => _audioPlayer.Volume * 100;
            set
            {
                _audioPlayer.Volume = value / 100;
                _playerSettings.Volume = value;
                NotifyPropertyChanged();
            }
        }

        public double Duration => _audioPlayer.Duration?.TotalSeconds ?? 0;

        // Each track mixed into the next, like a DJ.
        public bool Transitions
        {
            get => _audioPlayer.Transitions;
            set
            {
                _audioPlayer.Transitions = value;
                _playerSettings.Transitions = value;
                NotifyPropertyChanged();
            }
        }

        public double Position
        {
            // Never past the end, the length of a track can be an estimate or unknown.
            get => Math.Min(_audioPlayer.Position.TotalSeconds, Duration);
            set
            {
                // Only a seek when the user moves the slider. It also writes back a value that it clamped to its maximum, when
                // the track changed and its position and duration come one after the other. That would seek the new track
                // to a place in the one before, maybe to its end and on to the next.
                if (!_isDraggingPosition || _isDragOfTrackBefore)
                {
                    return;
                }
                _audioPlayer.Position = new TimeSpan(0, 0, (int)value);
            }
        }

        // From elsewhere than the position slider, like the system's media controls. Within the track: not while its length
        // isn't known, that would go back to its start.
        public void Seek(TimeSpan position)
        {
            if (Source.Length == 0 || _audioPlayer.Duration is not { } duration)
            {
                return;
            }
            _audioPlayer.Position = position < TimeSpan.Zero ? TimeSpan.Zero : position > duration ? duration : position;
            NotifyPropertyChanged(nameof(Position));
        }

        // The view says while the user moves the position slider: holds it, or presses a key on it. When another track
        // starts while it's held, the slider is at a place in the one before, it doesn't seek until it's let go.
        public bool IsDraggingPosition
        {
            get => _isDraggingPosition;
            set
            {
                _isDraggingPosition = value;
                if (!value)
                {
                    _isDragOfTrackBefore = false;
                }
            }
        }

        // While the next track comes in: it as heard.
        public bool IsMixing => _audioPlayer.IncomingPosition is not null;
        public double IncomingPosition => _audioPlayer.IncomingPosition?.TotalSeconds ?? 0;
        public double IncomingDuration => _audioPlayer.IncomingDuration?.TotalSeconds ?? 0;

        // While the track plays at another tempo than its own, after a transition until it's back at its own: the beats per
        // minute heard. Not when they round to its own.
        public bool IsOffTempo => _audioPlayer.BeatsPerMinute is { } heard && Math.Round(heard) != Math.Round(heard / _audioPlayer.Tempo);
        public double BeatsPerMinute => _audioPlayer.BeatsPerMinute ?? 0;

        private readonly DelegateCommand _changePlayPauseCommand;
        public ICommand ChangePlayPauseCommand => _changePlayPauseCommand;
        private readonly DelegateCommand _loadCommand;
        public ICommand LoadCommand => _loadCommand;

        private readonly DelegateCommand _nextCommand;
        public ICommand NextCommand => _nextCommand;

        private readonly DelegateCommand _lastCommand;
        public ICommand LastCommand => _lastCommand;

        private readonly DelegateCommand _muteCommand;
        public ICommand MuteCommand => _muteCommand;

        private void _audioPlayer_MediaEnded(object? sender, EventArgs e)
        {
            // The next track sets this again, if there is one.
            IsPlaying = false;
            InvokePlayNextTrackEvent();
        }

        private void _audioPlayer_MediaOpened(object? sender, EventArgs e)
        {
            NotifyPropertyChanged(nameof(Position));
            NotifyPropertyChanged(nameof(Duration));
        }

        private void _audioPlayer_MediaFailed(object? sender, EventArgs e)
        {
            // E.g. the file is on a drive that isn't connected. Don't pretend to play it.
            IsPlaying = false;
        }

        // The list that plays decides what comes next, like at the end of a track, but doesn't go on to it yet: the track that
        // plays is the current one until the transition is over.
        private void _audioPlayer_TransitionDue(object? sender, EventArgs e)
        {
            if (!Transitions || !IsPlaying)
            {
                return;
            }
            var next = PeekNextTrack();
            if (next.Track is { } track)
            {
                // Not mixed in, the next track comes when this one ended, like without transitions.
                _mixing = _audioPlayer.MixTo(new Uri(track.Path)) ? next : null;
            }
        }

        // Now the list goes on to it.
        private void _audioPlayer_MixedIn(object? sender, EventArgs e)
        {
            if (_mixing is { Track: { } track, List: { } list })
            {
                _mixing = null;
                _isTakingOver = true;
                try
                {
                    list.WentOnTo(track);
                }
                finally
                {
                    _isTakingOver = false;
                }
            }
            TrackChanged();
        }

        private void TrackChanged()
        {
            _isDragOfTrackBefore = _isDraggingPosition;
            Track = TrackInfo.Read(Source);
            NotifyPropertyChanged(nameof(Source));
            NotifyPropertyChanged(nameof(Track));
            NotifyPropertyChanged(nameof(Position));
            NotifyPropertyChanged(nameof(Duration));
            NotifyAsHeard();
        }

        // What changes as it plays, besides the position.
        private void NotifyAsHeard()
        {
            NotifyPropertyChanged(nameof(IsMixing));
            NotifyPropertyChanged(nameof(IncomingPosition));
            NotifyPropertyChanged(nameof(IncomingDuration));
            NotifyPropertyChanged(nameof(IsOffTempo));
            NotifyPropertyChanged(nameof(BeatsPerMinute));
        }

        private void _audioPlayer_PositionChanged(object? sender, EventArgs e)
        {
            NotifyPropertyChanged(nameof(Position));
            NotifyAsHeard();
        }

        private void OnChangePlayPause(object? commandParameter)
        {
            if (IsPlaying)
            {
                Pause();
            }
            else
            {
                Play();
            }
        }

        private async void OnLoad(object? commandParameter)
        {
            var paths = await _dialogs.PickMusicFilesAsync().ConfigureAwait(true);

            foreach (var path in paths)
            {
                OnManualLoad(new ManualLoadEventArgs(path));
            }
        }

        private void OnMute(object? commandParameter)
        {
            IsMuted = !IsMuted;
        }

        public void Open(string path)
        {
            _audioPlayer.Open(new Uri(path));
            // The new track's position right away, not the old one's until the next tick.
            TrackChanged();
        }

        public void Play()
        {
            if (Source.Length > 0)
            {
                _audioPlayer.Play();
                IsPlaying = true;
            }
            else
            {
                _nextCommand.Execute(new object());
            }
        }

        public void Pause()
        {
            _audioPlayer.Pause();
            IsPlaying = false;
        }
    }
}
