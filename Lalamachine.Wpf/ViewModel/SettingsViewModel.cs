using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;

namespace Lalamachine.Wpf.ViewModel
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        public SettingsViewModel()
        {
            _loadCommand = new DelegateCommand(OnLoad);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string libraryPath;
        public string LibraryPath
        {
            get => libraryPath;
            set
            {
                libraryPath = value;
                NotifyPropertyChanged();
            }
        }

        private readonly DelegateCommand _loadCommand;
        public ICommand LoadCommand => _loadCommand;

        private void OnLoad(object commandParameter)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    LibraryPath = fbd.SelectedPath;
                }
            }
        }
    }
}
