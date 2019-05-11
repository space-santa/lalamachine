using System.Windows;
using System.Windows.Controls;

namespace Lalamachine.Wpf.View.Controls
{
    public partial class VolumeControl : UserControl
    {
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
