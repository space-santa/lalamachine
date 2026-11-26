using Avalonia.Controls;
using Avalonia.Interactivity;
using Lalamachine.ViewModel;

namespace Lalamachine.Desktop.Views
{
    public partial class PlayerControl : UserControl
    {
        private bool _isPositionHeld;
        private bool _isPositionKeyDown;

        public PlayerControl()
        {
            InitializeComponent();
            // What the position slider writes is a seek only while the user moves it, see PlayerViewModel.Position. Held
            // before the slider handles the press, it writes its value then already. Let go after it handled the release.
            PositionSlider.AddHandler(PointerPressedEvent, (sender, e) => SetPositionHeld(true), RoutingStrategies.Tunnel, handledEventsToo: true);
            PositionSlider.AddHandler(PointerReleasedEvent, (sender, e) => SetPositionHeld(false), RoutingStrategies.Bubble, handledEventsToo: true);
            PositionSlider.AddHandler(PointerCaptureLostEvent, (sender, e) => SetPositionHeld(false), RoutingStrategies.Bubble, handledEventsToo: true);
            // The arrow keys, Home and End move it too, the same way round.
            PositionSlider.AddHandler(KeyDownEvent, (sender, e) => SetPositionKeyDown(true), RoutingStrategies.Tunnel, handledEventsToo: true);
            PositionSlider.AddHandler(KeyDownEvent, (sender, e) => SetPositionKeyDown(false), RoutingStrategies.Bubble, handledEventsToo: true);
        }

        private void SetPositionHeld(bool isHeld)
        {
            _isPositionHeld = isHeld;
            UpdateDraggingPosition();
        }

        private void SetPositionKeyDown(bool isKeyDown)
        {
            _isPositionKeyDown = isKeyDown;
            UpdateDraggingPosition();
        }

        private void UpdateDraggingPosition()
        {
            if (DataContext is PlayerViewModel player)
            {
                player.IsDraggingPosition = _isPositionHeld || _isPositionKeyDown;
            }
        }
    }
}
