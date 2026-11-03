using Avalonia.Controls;
using Avalonia.Input;

namespace Lalamachine.Desktop.Views
{
    public partial class PlayerControl : UserControl
    {
        public PlayerControl()
        {
            InitializeComponent();
        }

        private void Volume_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
        {
            // A notch of a mouse wheel is 1. A trackpad sends many small deltas, and 0 for a sideways swipe.
            VolumeSlider.Value += e.Delta.Y * 5;
            e.Handled = true;
        }
    }
}
