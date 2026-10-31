using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lalamachine.ViewModel
{
    // Async because the dialogs of the cross-platform UI toolkits are.
    public interface IDialogService
    {
        Task ShowMessageAsync(string message);

        // Empty when the user cancels.
        Task<IReadOnlyList<string>> PickMusicFilesAsync();

        // Null when the user cancels.
        Task<string?> PickFolderAsync();
    }
}
