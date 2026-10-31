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

        public static Task ShowAsync(Window owner, string message)
        {
            var window = new MessageWindow();
            window.MessageText.Text = message;
            return window.ShowDialog(owner);
        }

        private void OkButton_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
