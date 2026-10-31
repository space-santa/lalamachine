using System;
using System.ComponentModel;
using System.Windows.Input;
using Settings;

namespace Lalamachine.ViewModel
{
    public class SettingsSettings : AppSettings
    {
        // Still "Lalamachine.Wpf", that's the name the existing settings files are stored under.
        public SettingsSettings() : base("Lalamachine.Wpf", "SettingsSettings") { }

        public string LibraryPath { get => GetString(""); set => Set(value); }
    }

    public class StartScanEventArgs : EventArgs
    {
        public StartScanEventArgs(string path)
        {
            Path = path;
        }
        public string Path { get; }
    }

    public class SettingsViewModel : BaseNotifyPropertyChanged
    {
        private readonly DelegateCommand _scanCommand;
        private readonly DelegateCommand _loadCommand;
        private readonly SettingsSettings _settings;
        private readonly IDialogService _dialogs;
        private bool _scanning;

        public SettingsViewModel(IDialogService dialogs)
        {
            _dialogs = dialogs;
            _loadCommand = new DelegateCommand(OnLoad);
            _scanCommand = new DelegateCommand(OnScan);
            _settings = new SettingsSettings();
            Scanning = false;
            PropertyChanged += LibraryPathChanged;
        }

        private void LibraryPathChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LibraryPath))
            {
                _settings.LibraryPath = LibraryPath;
            }
        }

        public string LibraryPath
        {
            get => _settings.LibraryPath;
            set
            {
                _settings.LibraryPath = value;
                NotifyPropertyChanged();
            }
        }

        public bool Scanning
        {
            get => _scanning;
            set
            {
                _scanning = value;
                NotifyPropertyChanged();
            }
        }

        public void ScanningChangedHandler(object? sender, ScanningChangedEventArgs e)
        {
            if (e is null)
            {
                return;
            }

            Scanning = e.Scanning;
        }

        public ICommand ScanCommand => _scanCommand;

        public ICommand LoadCommand => _loadCommand;

        private async void OnLoad(object? commandParameter)
        {
            if (Scanning) { return; }
            var folder = await _dialogs.PickFolderAsync().ConfigureAwait(true);

            if (!string.IsNullOrWhiteSpace(folder))
            {
                LibraryPath = folder;
                ScanCommand.Execute(null);
            }
        }

        private void OnScan(object? commandParameters)
        {
            if (Scanning) { return; }
            if (LibraryPath.Length > 0)
            {
                InvokeStartScanEvent();
            }
        }

        public event EventHandler<StartScanEventArgs>? StartScanEvent;
        protected virtual void InvokeStartScanEvent()
        {
            var args = new StartScanEventArgs(LibraryPath);
            StartScanEvent?.Invoke(this, args);
        }
    }
}
