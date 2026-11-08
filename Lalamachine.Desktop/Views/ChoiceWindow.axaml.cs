using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Lalamachine.Desktop.Views
{
    public partial class ChoiceWindow : Window
    {
        public ChoiceWindow()
        {
            InitializeComponent();
        }

        public ChoiceWindow(string prompt, IReadOnlyList<string> choices) : this()
        {
            PromptText.Text = prompt;
            ChoiceList.ItemsSource = choices;
        }

        // The index of the chosen one, null when the user cancels or closes the window.
        public static Task<int?> ShowAsync(Window owner, string prompt, IReadOnlyList<string> choices)
        {
            ArgumentNullException.ThrowIfNull(owner);

            return new ChoiceWindow(prompt, choices).ShowDialog<int?>(owner);
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            ChoiceList.Focus();
        }

        private void ChoiceList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            OkButton.IsEnabled = ChoiceList.SelectedIndex >= 0;
        }

        private void ChoiceList_DoubleTapped(object? sender, TappedEventArgs e)
        {
            if (ChoiceList.SelectedIndex >= 0)
            {
                Close(ChoiceList.SelectedIndex);
            }
        }

        private void OkButton_Click(object? sender, RoutedEventArgs e)
        {
            Close(ChoiceList.SelectedIndex);
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            Close(null);
        }
    }
}
