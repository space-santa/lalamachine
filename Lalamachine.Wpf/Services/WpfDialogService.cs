using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Lalamachine.ViewModel;
using Microsoft.Win32;

namespace Lalamachine.Wpf.Services
{
    internal sealed class WpfDialogService : IDialogService
    {
        public Task ShowMessageAsync(string message)
        {
            MessageBox.Show(message);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<string>> PickMusicFilesAsync()
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Music | *.mp3; *.m4a"
            };

            IReadOnlyList<string> paths = dialog.ShowDialog() == true ? dialog.FileNames : Array.Empty<string>();
            return Task.FromResult(paths);
        }

        public Task<string?> PickFolderAsync()
        {
            var dialog = new OpenFolderDialog();
            var folder = dialog.ShowDialog() == true ? dialog.FolderName : null;
            return Task.FromResult(folder);
        }
    }
}
