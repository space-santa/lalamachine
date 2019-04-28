using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Media;

namespace Lalamachine.Wpf.View.Controls
{
    /// <summary>
    /// Interaction logic for PlayerControl.xaml
    /// </summary>
    public partial class PlayerControl : UserControl, INotifyPropertyChanged
    {
        public PlayerControl()
        {
            InitializeComponent();
            mediaPlayer = new MediaPlayer();
        }

        private MediaPlayer mediaPlayer;

        public int PlayerVolume
        {
            get
            {
                return (int) (mediaPlayer.Volume * 100);
            }
            set
            {
                mediaPlayer.Volume = value / 100;
                System.Diagnostics.Debug.WriteLine($"Player - {mediaPlayer.Volume}");
                NotifyPropertyChanged();
            }
        }

        public bool PlayerMuted
        {
            get => mediaPlayer.IsMuted;
            set
            {
                mediaPlayer.IsMuted = value;
                System.Diagnostics.Debug.WriteLine($"Player - {mediaPlayer.IsMuted}");
                NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
