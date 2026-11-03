using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Lalamachine.Desktop.Views
{
    public partial class TextInputWindow : Window
    {
        public TextInputWindow()
        {
            InitializeComponent();
        }

        public TextInputWindow(string prompt, string text) : this()
        {
            PromptText.Text = prompt;
            InputBox.Text = text;
        }

        // Null when the user cancels.
        public static Task<string?> ShowAsync(Window owner, string prompt, string text)
        {
            return new TextInputWindow(prompt, text).ShowDialog<string?>(owner);
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            InputBox.Focus();
            InputBox.SelectAll();
        }

        private void OkButton_Click(object? sender, RoutedEventArgs e)
        {
            Close(InputBox.Text ?? "");
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            Close(null);
        }
    }
}
