using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Lalamachine.Desktop.Views
{
    // Avalonia has no message box.
    public partial class MessageWindow : Window
    {
        // The margins of the panel and the title bar, a bit more than it takes on Windows and macOS.
        private const double MarginsAndTitleBarHeight = 80;

        public MessageWindow()
        {
            InitializeComponent();
        }

        public MessageWindow(string message) : this()
        {
            MessageText.Text = message;
        }

        public static Task ShowAsync(Window owner, string message)
        {
            ArgumentNullException.ThrowIfNull(owner);

            return FittingOnScreen(owner, message).ShowDialog(owner);
        }

        // False when the user cancels or closes the window.
        public static Task<bool> ConfirmAsync(Window owner, string message)
        {
            ArgumentNullException.ThrowIfNull(owner);

            var window = FittingOnScreen(owner, message);
            window.CancelButton.IsVisible = true;
            return window.ShowDialog<bool>(owner);
        }

        // Some messages are long reports, and a laptop screen with scaling is lower than the text's own limit. On the panel,
        // a MaxHeight on the window only cuts off what's in it.
        private static MessageWindow FittingOnScreen(Window owner, string message)
        {
            var window = new MessageWindow(message);
            if (owner.Screens.ScreenFromWindow(owner) is { } screen)
            {
                window.MessagePanel.MaxHeight = (screen.WorkingArea.Height / screen.Scaling) - MarginsAndTitleBarHeight;
            }
            return window;
        }

        private void OkButton_Click(object? sender, RoutedEventArgs e)
        {
            Close(true);
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            Close(false);
        }
    }
}
