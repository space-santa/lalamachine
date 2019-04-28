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
            DataContext = this;
            mediaPlayer = new MediaPlayer();
            InitializeComponent();
        }

        private MediaPlayer mediaPlayer;

        public double PlayerVolume
        {
            get
            {
                return mediaPlayer.Volume * 100;
            }
            set
            {
                mediaPlayer.Volume = value / 100;
                System.Diagnostics.Debug.WriteLine($"Player: value = {value} - mediaPlayer.Volume = {mediaPlayer.Volume}");
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

        private void PlayerButtonsControl_Load(object sender, System.Windows.RoutedEventArgs e)
        {
            var source = (PlayerButtonsControl)e.Source;
            System.Diagnostics.Debug.WriteLine("a;sdfj;aslf");
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Music | *.mp3; *.m4a";

            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                var path = dlg.FileName;
                mediaPlayer.Open(new System.Uri(path));
            }
        }

        private void PlayerButtonsControl_PlayPause(object sender, System.Windows.RoutedEventArgs e)
        {
            var source = (PlayerButtonsControl)e.Source;
            if (source.PlayPauseButton.IsChecked == true)
            {
                mediaPlayer.Play();
            }
            else
            {
                mediaPlayer.Pause();
            }
        }
    }
}
