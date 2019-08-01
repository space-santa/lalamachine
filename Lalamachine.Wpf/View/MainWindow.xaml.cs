using Lalamachine.Wpf.ViewModel;
using Settings;
using System.Windows;

namespace Lalamachine.Wpf.View
{
    class MainWindowSettings : AppSettings
    {
        public MainWindowSettings() : base("Lalamachine.Wpf", "MainWindowSettings") { }

        public int ActiveTab { get => GetInt(0); set => Set(value); }
        public double Width { get => GetDouble(800); set { Set(value); } }
        public double Height { get => GetDouble(800); set { Set(value); } }
        public double Left { get => GetDouble(255); set { Set(value); } }
        public double Top { get => GetDouble(255); set { Set(value); } }
        public bool Maximized { get => GetBool(false); set { Set(value); } }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly PlayerViewModel _player;
        private PlaylistViewModel _playlistViewModel;
        private ShuffleRepeatViewModel _shuffleRepeatViewModel;
        private MainWindowSettings _settings;
        private SettingsViewModel _settingsViewModel;

        public MainWindow()
        {
            InitializeComponent();
            _player = new PlayerViewModel();
            _playlistViewModel = new PlaylistViewModel();
            _shuffleRepeatViewModel = new ShuffleRepeatViewModel();
            _settingsViewModel = new SettingsViewModel();
            _shuffleRepeatViewModel.ShuffleRepeatChanged += _playlistViewModel.ShuffleRepeatChangedHandler;
            _player.ManualLoadEvent += _playlistViewModel.ManualLoadHandler;
            _player.PlayNextTrackEvent += _playlistViewModel.PlayNextTrackHandler;
            _player.PlayLastTrackEvent += _playlistViewModel.PlayLastTrackHandler;
            _playlistViewModel.PlayTrackEvent += _player.PlayTrackHandler;
            Player.DataContext = _player;
            Player.ShuffleRepeatControl.DataContext = _shuffleRepeatViewModel;
            MainPlaylist.DataContext = _playlistViewModel;
            Settings.DataContext = _settingsViewModel;

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
