using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows.Input;
using Settings;

namespace Lalamachine.Wpf.ViewModel
{
    public class SettingsSettings : AppSettings
    {
        public SettingsSettings() : base("Lalamachine.Wpf", "SettingsSettings") { }

        public string LibraryPath { get => GetString(""); set => Set(value); }
    }

    public class StartScanEventArgs : EventArgs
    {
        public string Path { get; set; }
    }

    public class SettingsViewModel : INotifyPropertyChanged
    {
        public SettingsViewModel()
        {
            _loadCommand = new DelegateCommand(OnLoad);
            _scanCommand = new DelegateCommand(OnScan);
            _settings = new SettingsSettings();
            LibraryPath = _settings.LibraryPath;
        }

        private readonly SettingsSettings _settings;

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
                _settings.LibraryPath = libraryPath;
                NotifyPropertyChanged();
            }
        }

        private readonly DelegateCommand _scanCommand;
        public ICommand ScanCommand => _scanCommand;

        private readonly DelegateCommand _loadCommand;
        public ICommand LoadCommand => _loadCommand;

        private void OnLoad(object commandParameter)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                var result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    LibraryPath = fbd.SelectedPath;
                    ScanCommand.Execute(null);
                }
            }
        }

        private void OnScan(object commandParameters)
        {
            if (LibraryPath.Length > 0)
            {
                InvokeStartScanEvent();
            }
        }

        public event EventHandler<StartScanEventArgs> StartScanEvent;
        protected virtual void InvokeStartScanEvent()
        {
            var args = new StartScanEventArgs
            {
                Path = LibraryPath
            };
            StartScanEvent?.Invoke(this, args);
        }
    }
}
