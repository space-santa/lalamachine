using System;
using System.Windows.Input;
using Settings;

namespace Lalamachine.ViewModel
{
    public class PlayerSettings : AppSettings
    {
        // Still "Lalamachine.Wpf", that's the name the existing settings files are stored under.
        public PlayerSettings() : base("Lalamachine.Wpf", "PlayerSettings") { }

        public double Volume { get => GetDouble(50); set => Set(value); }
        public bool IsMuted { get => GetBool(false); set => Set(value); }
        public string LastActivePlaylist { get => GetString("MAIN"); set => Set(value); }
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

        public PlayerViewModel(IAudioPlayer audioPlayer, IDialogService dialogs)
        {
            _playerSettings = new PlayerSettings();
            _audioPlayer = audioPlayer;
            _dialogs = dialogs;
            IsMuted = _playerSettings.IsMuted;
            Volume = _playerSettings.Volume;
            _audioPlayer.MediaOpened += _audioPlayer_MediaOpened;
            _audioPlayer.MediaEnded += _audioPlayer_MediaEnded;
            _audioPlayer.MediaFailed += _audioPlayer_MediaFailed;
            _audioPlayer.PositionChanged += _audioPlayer_PositionChanged;

            _changePlayPauseCommand = new DelegateCommand(OnChangePlayPause);
            _loadCommand = new DelegateCommand(OnLoad);
            _nextCommand = new DelegateCommand(OnPlayNextTrack);
            _lastCommand = new DelegateCommand(OnPlayLastTrack);
            _muteCommand = new DelegateCommand(OnMute);
        }

        public event EventHandler? PlayNextTrackEvent;
        public event EventHandler? PlayNextLibraryTrackEvent;
        protected virtual void InvokePlayNextTrackEvent()
        {
            if (LastActivePlaylist == "MAIN")
            {
                PlayNextTrackEvent?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                PlayNextLibraryTrackEvent?.Invoke(this, EventArgs.Empty);
            }
        }
        protected virtual void OnPlayNextTrack(object? commandParameter)
        {
            InvokePlayNextTrackEvent();
        }

        public event EventHandler? PlayLastTrackEvent;
        public event EventHandler? PlayLastLibraryTrackEvent;
        protected virtual void OnPlayLastTrack(object? commandParameter)
        {
            if (LastActivePlaylist == "MAIN")
            {
                PlayLastTrackEvent?.Invoke(this, EventArgs.Empty);
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

            Source = e.Path;
            LastActivePlaylist = e.ListName;
            Play();
        }

        public string Source
        {
            // LocalPath, not AbsolutePath: AbsolutePath drops the server of a network path (\\nas\music\a.mp3 becomes /music/a.mp3).
            get => _audioPlayer.Source?.LocalPath ?? "";
            set => Open(value);
        }

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

        public double Position
        {
            get => _audioPlayer.Position.TotalSeconds;
            set => _audioPlayer.Position = new TimeSpan(0, 0, (int)value);
        }

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
            InvokePlayNextTrackEvent();
        }

        private void _audioPlayer_MediaOpened(object? sender, EventArgs e)
        {
            NotifyPropertyChanged(nameof(Duration));
        }

        private void _audioPlayer_MediaFailed(object? sender, EventArgs e)
        {
            // E.g. the file is on a drive that isn't connected. Don't pretend to play it.
            IsPlaying = false;
        }

        private void _audioPlayer_PositionChanged(object? sender, EventArgs e)
        {
            NotifyPropertyChanged(nameof(Position));
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
            NotifyPropertyChanged(nameof(Source));
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
