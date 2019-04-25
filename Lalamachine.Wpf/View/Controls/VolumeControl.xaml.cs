using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace Lalamachine.Wpf.View.Controls
{
    public partial class VolumeControl : UserControl, INotifyPropertyChanged
    {
        private bool muted;
        public bool Muted
        {
            get => muted;
            set
            {
                muted = value;
                NotifyPropertyChanged();
            }
        }

        private int volume;
        public int Volume
        {
            get => volume;
            set
            {
                volume = value;
                NotifyPropertyChanged();
            }
        }

        public VolumeControl()
        {
            DataContext = this;
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void MutedButton_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                VolumeSlider.Value += 5;
            }
            else
            {
                VolumeSlider.Value -= 5;
            }
        }
    }
}
