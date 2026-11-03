using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Lalamachine.Desktop.Views
{
    // Avalonia has no message box.
    public partial class MessageWindow : Window
    {
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
            return new MessageWindow(message).ShowDialog(owner);
        }

        // False when the user cancels or closes the window.
        public static Task<bool> ConfirmAsync(Window owner, string message)
        {
            var window = new MessageWindow(message);
            window.CancelButton.IsVisible = true;
            return window.ShowDialog<bool>(owner);
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
