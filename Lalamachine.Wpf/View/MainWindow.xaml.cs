using System.Windows;
using LalaDb.Data;
using Lalamachine.Wpf.ViewModel;
using Microsoft.EntityFrameworkCore;
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
        private readonly LibraryPlaylistViewModel _libraryPlaylistViewModel;

        public MainWindow()
        {
            InitializeComponent();
            var optionsBuilder = new DbContextOptionsBuilder<LalaContext>();
            optionsBuilder
                .UseLazyLoadingProxies()
                    .UseSqlite($"Data Source={LibLala.Constants.DBPATH}");
#pragma warning disable CA2000 // Dispose objects before losing scope. The DI container is taking care of the context lifecycle.
            var context = new LalaContext(optionsBuilder.Options);
#pragma warning restore CA2000 // Dispose objects before losing scope
            context.Database.Migrate();

            _player = new PlayerViewModel();
            _playlistViewModel = new PlaylistViewModel(context);
            _shuffleRepeatViewModel = new ShuffleRepeatViewModel();
            _settingsViewModel = new SettingsViewModel();
            _libraryViewModel = new LibraryViewModel(context);
            _libraryPlaylistViewModel = new LibraryPlaylistViewModel();

            _shuffleRepeatViewModel.ShuffleRepeatChanged += _playlistViewModel.ShuffleRepeatChangedHandler;
            _player.ManualLoadEvent += _playlistViewModel.ManualLoadHandler;

            _player.PlayNextTrackEvent += _playlistViewModel.PlayNextTrackHandler;
            _player.PlayLastTrackEvent += _playlistViewModel.PlayLastTrackHandler;
            _player.PlayNextLibraryTrackEvent += _libraryPlaylistViewModel.PlayNextTrackHandler;
            _player.PlayLastLibraryTrackEvent += _libraryPlaylistViewModel.PlayLastTrackHandler;

            _playlistViewModel.PlayTrackEvent += _player.PlayTrackHandler;
            _libraryPlaylistViewModel.PlayTrackEvent += _player.PlayTrackHandler;
            _libraryPlaylistViewModel.AddLibraryTracksToPlaylistEvent += _playlistViewModel.AddTracksToPlaylistHandler;

            _settingsViewModel.StartScanEvent += _libraryViewModel.StartScanHandler;
            _libraryViewModel.DisplayLibChanged += _libraryPlaylistViewModel.DisplayChangedHandler;
            _libraryViewModel.AddTracksToPlaylistEvent += _playlistViewModel.AddTracksToPlaylistHandler;

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
            if (_settings.Maximized) { SetCurrentValue(WindowStateProperty, WindowState.Maximized); }
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            _playlistViewModel.SavePlaylist();
            _settings.ActiveTab = ListTabs.SelectedIndex;
            _settings.Top = Top;
            _settings.Left = Left;
            _settings.Width = Width;
            _settings.Height = Height;
            _settings.Maximized = WindowState == WindowState.Maximized;
        }
    }
}
