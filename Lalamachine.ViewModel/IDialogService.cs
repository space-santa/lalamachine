using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lalamachine.ViewModel
{
    // Async because the dialogs of the cross-platform UI toolkits are.
    public interface IDialogService
    {
        Task ShowMessageAsync(string message);

        // False when the user cancels.
        Task<bool> ConfirmAsync(string message);

        // Null when the user cancels.
        Task<string?> AskForTextAsync(string prompt, string text);

        // The index of the chosen one, null when the user cancels.
        Task<int?> ChooseAsync(string prompt, IReadOnlyList<string> choices);

        // Empty when the user cancels.
        Task<IReadOnlyList<string>> PickMusicFilesAsync();

        // Null when the user cancels.
        Task<string?> PickFolderAsync();
    }
}
