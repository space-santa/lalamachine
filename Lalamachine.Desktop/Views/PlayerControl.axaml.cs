using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Lalamachine.ViewModel;

namespace Lalamachine.Desktop.Views
{
    public partial class PlayerControl : UserControl
    {
        public PlayerControl()
        {
            InitializeComponent();
            // Held before the slider handles the press, it writes its value then already. Let go after it handled the release.
            PositionSlider.AddHandler(PointerPressedEvent, (sender, e) => SetDraggingPosition(true), RoutingStrategies.Tunnel, handledEventsToo: true);
            PositionSlider.AddHandler(PointerReleasedEvent, (sender, e) => SetDraggingPosition(false), RoutingStrategies.Bubble, handledEventsToo: true);
            PositionSlider.AddHandler(PointerCaptureLostEvent, (sender, e) => SetDraggingPosition(false), RoutingStrategies.Bubble, handledEventsToo: true);
        }

        private void SetDraggingPosition(bool isDragging)
        {
            if (DataContext is PlayerViewModel player)
            {
                player.IsDraggingPosition = isDragging;
            }
        }

        private void Volume_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
        {
            // A notch of a mouse wheel is 1. A trackpad sends many small deltas, and 0 for a sideways swipe.
            VolumeSlider.Value += e.Delta.Y * 5;
            e.Handled = true;
        }
    }
}
