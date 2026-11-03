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
        private static readonly FilePickerFileType MusicFiles = new("Music")
        {
            Patterns = ["*.mp3", "*.m4a"],
            AppleUniformTypeIdentifiers = ["public.mp3", "public.mpeg-4-audio"],
            MimeTypes = ["audio/mpeg", "audio/mp4"],
        };

        private readonly Window _owner;

        public AvaloniaDialogService(Window owner)
        {
            _owner = owner;
        }

        public Task ShowMessageAsync(string message) => MessageWindow.ShowAsync(_owner, message);

        public Task<bool> ConfirmAsync(string message) => MessageWindow.ConfirmAsync(_owner, message);

        public Task<string?> AskForTextAsync(string prompt, string text) => TextInputWindow.ShowAsync(_owner, prompt, text);

        public async Task<IReadOnlyList<string>> PickMusicFilesAsync()
        {
            var files = await _owner.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                AllowMultiple = true,
                FileTypeFilter = [MusicFiles],
            }).ConfigureAwait(true);

            var paths = new List<string>();
            foreach (var file in files)
            {
                using (file)
                {
                    if (file.TryGetLocalPath() is { } path)
                    {
                        paths.Add(path);
                    }
                }
            }
            return paths;
        }

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
