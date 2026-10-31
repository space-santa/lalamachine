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
            VolumeSlider.Value += e.Delta.Y > 0 ? 5 : -5;
            e.Handled = true;
        }
    }
}
