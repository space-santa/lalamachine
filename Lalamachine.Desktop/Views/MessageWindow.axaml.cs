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

        private void OkButton_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
