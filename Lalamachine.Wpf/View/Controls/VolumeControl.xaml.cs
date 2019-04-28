using System.Windows;
using System.Windows.Controls;

namespace Lalamachine.Wpf.View.Controls
{
    public partial class VolumeControl : UserControl
    {
        public static DependencyProperty MutedProperty =
           DependencyProperty.Register("Muted", typeof(bool), typeof(VolumeControl));
        public bool Muted
        {
            get => (bool)GetValue(MutedProperty);
            set
            {
                SetValue(MutedProperty, value);
                System.Diagnostics.Debug.WriteLine($"muted in volumecontrol {value}");
            }
        }

        public static DependencyProperty VolumeProperty =
           DependencyProperty.Register("Volume", typeof(double), typeof(VolumeControl));
        public double Volume
        {
            get => (double)GetValue(VolumeProperty);
            set
            {
                SetValue(VolumeProperty, value);
                System.Diagnostics.Debug.WriteLine($"volume in volumecontrol {value}");
            }
        }


        public VolumeControl()
        {
            InitializeComponent();
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
