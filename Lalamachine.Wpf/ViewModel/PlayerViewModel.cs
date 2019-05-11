using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;

namespace Lalamachine.Wpf.ViewModel
{
    public class PlayerViewModel : INotifyPropertyChanged
    {
        private MediaPlayer _mediaPlayer;

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
        private void SetIsPlaying(bool value)
        {
            _isPlaying = value;
            NotifyPropertyChanged("IsPlaying");
        }
        public bool IsPlaying { get => _isPlaying; }

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

        private readonly DelegateCommand _changePlayPauseCommand;
        public ICommand ChangePlayPauseCommand => _changePlayPauseCommand;
        private readonly DelegateCommand _loadCommand;
        public ICommand LoadCommand => _loadCommand;

        public PlayerViewModel()
        {
            _mediaPlayer = new MediaPlayer();
            IsMuted = false;
            Volume = 50;

            _changePlayPauseCommand = new DelegateCommand(OnChangePlayPause, CanChangePlayPause);
            _loadCommand = new DelegateCommand(OnLoad, CanLoad);
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
            SetIsPlaying(true);
        }

        public void Pause()
        {
            _mediaPlayer.Pause();
            SetIsPlaying(false);
        }

        public void Stop()
        {
            _mediaPlayer.Stop();
            SetIsPlaying(false);
        }
    }
}