using Settings;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Lalamachine.Wpf.ViewModel
{
    public class PlayerSettings : AppSettings
    {
        public PlayerSettings() : base("Lalamachine.Wpf", "PlayerSettings") { }

        public double Volume { get => GetDouble(50); set { Set(value); } }
        public bool IsMuted { get => GetBool(false); set { Set(value); } }
    }

    public class ManualLoadEventArgs : EventArgs
    {
        public string Path { get; set; }
    }

    public class PlayerViewModel : INotifyPropertyChanged
    {
        private MediaPlayer _mediaPlayer;
        private DispatcherTimer _dispatcherTimer;
        private PlayerSettings _playerSettings;

        public PlayerViewModel()
        {
            _playerSettings = new PlayerSettings();
            _mediaPlayer = new MediaPlayer();
            IsMuted = _playerSettings.IsMuted;
            Volume = _playerSettings.Volume;
            _mediaPlayer.MediaOpened += _mediaPlayer_MediaOpened;
            _mediaPlayer.MediaEnded += _mediaPlayer_MediaEnded;

            _changePlayPauseCommand = new DelegateCommand(OnChangePlayPause);
            _loadCommand = new DelegateCommand(OnLoad);
            _nextCommand = new DelegateCommand(OnPlayNextTrack);
            _lastCommand = new DelegateCommand(OnPlayLastTrack);

            SetupTimer();
            _dispatcherTimer.Start();
        }

        public event EventHandler PlayNextTrackEvent;
        protected virtual void InvokePlayNextTrackEvent()
        {
            PlayNextTrackEvent?.Invoke(this, EventArgs.Empty);
        }
        protected virtual void OnPlayNextTrack(object commandParameter)
        {
            InvokePlayNextTrackEvent();
        }

        public event EventHandler PlayLastTrackEvent;
        protected virtual void OnPlayLastTrack(object commandParameter)
        {
            PlayLastTrackEvent?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<ManualLoadEventArgs> ManualLoadEvent;
        protected virtual void OnManualLoad(ManualLoadEventArgs e)
        {
            ManualLoadEvent?.Invoke(this, e);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void PlayTrackHandler(object sender, PlayTrackEventArgs e)
        {
            Source = e.Path;
            Play();
        }

        public string Source
        {
            get
            {
                try
                {
                    return _mediaPlayer.Source.AbsolutePath;
                }
                catch (NullReferenceException)
                {
                    return "";
                }
            }
            set => Open(value);
        }

        private bool _isPlaying;
        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                _isPlaying = value;
                NotifyPropertyChanged("IsPlaying");
            }
        }
        public bool IsMuted
        {
            get => _mediaPlayer.IsMuted;
            set
            {
                _mediaPlayer.IsMuted = value;
                _playerSettings.IsMuted = _mediaPlayer.IsMuted;
                NotifyPropertyChanged();
            }
        }

        public double Volume
        {
            get => _mediaPlayer.Volume * 100;
            set
            {
                _mediaPlayer.Volume = value / 100;
                _playerSettings.Volume = value;
                NotifyPropertyChanged();
            }
        }

        public double Duration
        {
            get
            {
                if (_mediaPlayer.NaturalDuration.HasTimeSpan)
                {
                    return _mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                }

                return 0;
            }
        }

        public double Position
        {
            get
            {
                return _mediaPlayer.Position.TotalSeconds;
            }
            set
            {
                _mediaPlayer.Position = new TimeSpan(0, 0, (int)value);
            }
        }

        private readonly DelegateCommand _changePlayPauseCommand;
        public ICommand ChangePlayPauseCommand => _changePlayPauseCommand;
        private readonly DelegateCommand _loadCommand;
        public ICommand LoadCommand => _loadCommand;

        private readonly DelegateCommand _nextCommand;
        public ICommand NextCommand => _nextCommand;

        private readonly DelegateCommand _lastCommand;
        public ICommand LastCommand => _lastCommand;

        private void _mediaPlayer_MediaEnded(object sender, EventArgs e)
        {
            InvokePlayNextTrackEvent();
        }

        private void _mediaPlayer_MediaOpened(object sender, EventArgs e)
        {
            NotifyPropertyChanged("Duration");
        }

        private void SetupTimer()
        {
            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Tick += new EventHandler(DispatcherTimerTick);
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 200);
        }

        private void DispatcherTimerTick(object sender, EventArgs e)
        {
            NotifyPropertyChanged("Position");
        }

        private void OnChangePlayPause(object commandParameter)
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

        private void OnLoad(object commandParameter)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Multiselect = true;
            dlg.Filter = "Music | *.mp3; *.m4a";

            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                foreach (var path in dlg.FileNames)
                {
                    OnManualLoad(new ManualLoadEventArgs { Path = path });
                }
            }
        }

        public void Open(string path)
        {
            _mediaPlayer.Open(new Uri(path));
            NotifyPropertyChanged("Source");
            _changePlayPauseCommand.InvokeCanExecuteChanged();
        }

        public void Play()
        {
            if (_mediaPlayer.HasAudio)
            {
                _mediaPlayer.Play();
                IsPlaying = true;
            }
            else
            {
                _nextCommand.Execute(null);
            }
        }

        public void Pause()
        {
            _mediaPlayer.Pause();
            IsPlaying = false;
        }

        public void Stop()
        {
            _mediaPlayer.Stop();
            IsPlaying = false;
        }
    }
}