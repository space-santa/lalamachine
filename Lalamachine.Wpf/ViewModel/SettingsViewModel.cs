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
        public StartScanEventArgs(string path)
        {
            Path = path;
        }
        public string Path { get; }
    }

    public class SettingsViewModel : INotifyPropertyChanged
    {
        public SettingsViewModel()
        {
            _loadCommand = new DelegateCommand(OnLoad);
            _scanCommand = new DelegateCommand(OnScan);
            _settings = new SettingsSettings();
            _libraryPath = _settings.LibraryPath;
        }

        private readonly SettingsSettings _settings;

        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _libraryPath;
        public string LibraryPath
        {
            get => _libraryPath;
            set
            {
                _libraryPath = value;
                _settings.LibraryPath = _libraryPath;
                NotifyPropertyChanged();
            }
        }


        private string _scanVisible = "Collapsed";
        public string ScanVisible
        {
            get => _scanVisible;
            set
            {
                _scanVisible = value;
                NotifyPropertyChanged();
            }
        }

        internal void ScanVisibleChangedHandler(object? sender, ScanVisibleChangedEventArgs e)
        {
            ScanVisible = e.ScanVisible;
        }

        private readonly DelegateCommand _scanCommand;
        public ICommand ScanCommand => _scanCommand;

        private readonly DelegateCommand _loadCommand;
        public ICommand LoadCommand => _loadCommand;

        private void OnLoad(object commandParameter)
        {
            if (ScanVisible == "Visible") { return; }
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
            if (ScanVisible == "Visible") { return; }
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
