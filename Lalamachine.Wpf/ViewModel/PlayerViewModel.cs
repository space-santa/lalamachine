using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Lalamachine.Wpf.ViewModel
{
    public class PlayerViewModel : INotifyPropertyChanged
    {
        private MediaPlayer _mediaPlayer;
        private DispatcherTimer _dispatcherTimer;

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
                NotifyPropertyChanged();
            }
        }

        public double Volume
        {
            get => _mediaPlayer.Volume * 100;
            set
            {
                _mediaPlayer.Volume = value / 100;
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

        public PlayerViewModel()
        {
            _mediaPlayer = new MediaPlayer();
            IsMuted = false;
            Volume = 50;
            _mediaPlayer.MediaOpened += _mediaPlayer_MediaOpened;

            _changePlayPauseCommand = new DelegateCommand(OnChangePlayPause, CanChangePlayPause);
            _loadCommand = new DelegateCommand(OnLoad, CanLoad);

            SetupTimer();
            _dispatcherTimer.Start();
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

        private bool CanChangePlayPause(object commandParameter)
        {
            if (Source.Length < 1 && !IsPlaying)
            {
                return false;
            }

            return true;
        }

        private void OnLoad(object commandParameter)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Music | *.mp3; *.m4a";

            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                var path = dlg.FileName;
                Open(path);
            }
        }

        private bool CanLoad(object commandParameter)
        {
            return true;
        }

        public void Open(string path)
        {
            _mediaPlayer.Open(new System.Uri(path));
            NotifyPropertyChanged("Source");
            _changePlayPauseCommand.InvokeCanExecuteChanged();
        }

        public void Play()
        {
            _mediaPlayer.Play();
            IsPlaying = true;
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