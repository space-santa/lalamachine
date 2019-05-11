using System.ComponentModel;
using System.Runtime.CompilerServices;
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
            get => _mediaPlayer.Source.AbsolutePath;
            set => Open(value);
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

        public PlayerViewModel()
        {
            _mediaPlayer = new MediaPlayer();
            IsMuted = false;
            Volume = 50;
        }

        public void Open(string path)
        {
            _mediaPlayer.Open(new System.Uri(path));
            NotifyPropertyChanged("Source");
        }

        public void Play()
        {
            _mediaPlayer.Play();
        }

        public void Pause()
        {
            _mediaPlayer.Pause();
        }

        public void Stop()
        {
            _mediaPlayer.Stop();
        }
    }
}