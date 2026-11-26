using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Lalamachine.Desktop.Views;
using Lalamachine.ViewModel;

namespace Lalamachine.Desktop.Services
{
    internal sealed class AvaloniaDialogService : IDialogService
    {
        private readonly Window _owner;

        public AvaloniaDialogService(Window owner)
        {
            _owner = owner;
        }

        public Task ShowMessageAsync(string message) => MessageWindow.ShowAsync(_owner, message);

        public Task<bool> ConfirmAsync(string message) => MessageWindow.ConfirmAsync(_owner, message);

        public Task<string?> AskForTextAsync(string prompt, string text) => TextInputWindow.ShowAsync(_owner, prompt, text);

        public Task<int?> ChooseAsync(string prompt, IReadOnlyList<string> choices) => ChoiceWindow.ShowAsync(_owner, prompt, choices);

        public async Task<string?> PickFolderAsync()
        {
            var folders = await _owner.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()).ConfigureAwait(true);
            if (folders.Count == 0)
            {
                return null;
            }

            using var folder = folders[0];
            return folder.TryGetLocalPath();
        }
    }
}
