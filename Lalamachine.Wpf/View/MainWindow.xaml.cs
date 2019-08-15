using System.Windows;
using Lalamachine.Wpf.ViewModel;
using Settings;

namespace Lalamachine.Wpf.View
{
    internal class MainWindowSettings : AppSettings
    {
        public MainWindowSettings() : base("Lalamachine.Wpf", "MainWindowSettings") { }

        public int ActiveTab { get => GetInt(0); set => Set(value); }
        public double Width { get => GetDouble(800); set => Set(value); }
        public double Height { get => GetDouble(800); set => Set(value); }
        public double Left { get => GetDouble(255); set => Set(value); }
        public double Top { get => GetDouble(255); set => Set(value); }
        public bool Maximized { get => GetBool(false); set => Set(value); }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly PlayerViewModel _player;
        private readonly PlaylistViewModel _playlistViewModel;
        private readonly ShuffleRepeatViewModel _shuffleRepeatViewModel;
        private readonly MainWindowSettings _settings;
        private readonly SettingsViewModel _settingsViewModel;
        private readonly LibraryViewModel _libraryViewModel;
        private readonly PlaylistViewModel _libraryPlaylistViewModel;

        public MainWindow()
        {
            InitializeComponent();
            _player = new PlayerViewModel();
            _playlistViewModel = new PlaylistViewModel();
            _shuffleRepeatViewModel = new ShuffleRepeatViewModel();
            _settingsViewModel = new SettingsViewModel();
            _libraryViewModel = new LibraryViewModel();
            _libraryPlaylistViewModel = new PlaylistViewModel("LIBRARY");

            _shuffleRepeatViewModel.ShuffleRepeatChanged += _playlistViewModel.ShuffleRepeatChangedHandler;
            _player.ManualLoadEvent += _playlistViewModel.ManualLoadHandler;

            _player.PlayNextTrackEvent += _playlistViewModel.PlayNextTrackHandler;
            _player.PlayLastTrackEvent += _playlistViewModel.PlayLastTrackHandler;
            _player.PlayNextLibraryTrackEvent += _libraryPlaylistViewModel.PlayNextTrackHandler;
            _player.PlayLastLibraryTrackEvent += _libraryPlaylistViewModel.PlayLastTrackHandler;

            _playlistViewModel.PlayTrackEvent += _player.PlayTrackHandler;
            _libraryPlaylistViewModel.PlayTrackEvent += _player.PlayTrackHandler;

            _settingsViewModel.StartScanEvent += _libraryViewModel.StartScanHandler;
            _libraryViewModel.DisplayLibChanged += _libraryPlaylistViewModel.DisplayChangedHandler;

            Player.DataContext = _player;
            Player.ShuffleRepeatControl.DataContext = _shuffleRepeatViewModel;
            MainPlaylist.DataContext = _playlistViewModel;
            Settings.DataContext = _settingsViewModel;
            Library.DataContext = _libraryViewModel;
            Library.LibraryPlaylist.DataContext = _libraryPlaylistViewModel;

            _settings = new MainWindowSettings();
            ListTabs.SelectedIndex = _settings.ActiveTab;
            Top = _settings.Top;
            Left = _settings.Left;
            Width = _settings.Width;
            Height = _settings.Height;
            if (_settings.Maximized) { WindowState = WindowState.Maximized; }
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            _settings.ActiveTab = ListTabs.SelectedIndex;
            _settings.Top = Top;
            _settings.Left = Left;
            _settings.Width = Width;
            _settings.Height = Height;
            _settings.Maximized = WindowState == WindowState.Maximized;
        }
    }
}
