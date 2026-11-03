using System;
using Avalonia;
using Avalonia.Controls;
using LalaDb.Data;
using Lalamachine.Desktop.Services;
using Lalamachine.ViewModel;
using Microsoft.EntityFrameworkCore;
using Settings;

namespace Lalamachine.Desktop.Views
{
    internal sealed class MainWindowSettings : AppSettings
    {
        // The settings file is named after the second argument only. Not "MainWindowSettings", people may still have that
        // file from the old WPF app. It has the window position in device-independent units, Avalonia's is in pixels.
        public MainWindowSettings() : base("Lalamachine.Desktop", "DesktopWindowSettings") { }

        public int ActiveTab { get => GetInt(0); set => Set(value); }
        public double Width { get => GetDouble(800); set => Set(value); }
        public double Height { get => GetDouble(800); set => Set(value); }
        public int Left { get => GetInt(255); set => Set(value); }
        public int Top { get => GetInt(255); set => Set(value); }
        public bool Maximized { get => GetBool(false); set => Set(value); }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "The audio player is disposed when the window closes.")]
    public partial class MainWindow : Window
    {
        private readonly VlcAudioPlayer _audioPlayer;
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
#pragma warning disable CA2000 // Dispose objects before losing scope. The UI's context lives as long as the main window, i.e. the whole app.
            var context = new LalaContext(optionsBuilder.Options);
#pragma warning restore CA2000 // Dispose objects before losing scope
            context.Database.Migrate();

            var dialogs = new AvaloniaDialogService(this);
            _audioPlayer = new VlcAudioPlayer();
            _player = new PlayerViewModel(_audioPlayer, dialogs);
            _playlistViewModel = new PlaylistViewModel(context, dialogs);
            _shuffleRepeatViewModel = new ShuffleRepeatViewModel();
            _settingsViewModel = new SettingsViewModel(dialogs);
            _libraryViewModel = new LibraryViewModel(context, () => new LalaContext(optionsBuilder.Options), dialogs);
            _libraryPlaylistViewModel = new LibraryPlaylistViewModel(dialogs);

            _shuffleRepeatViewModel.ShuffleRepeatChanged += _playlistViewModel.ShuffleRepeatChangedHandler;
            _shuffleRepeatViewModel.ShuffleRepeatChanged += _libraryPlaylistViewModel.ShuffleRepeatChangedHandler;
            _player.ManualLoadEvent += _playlistViewModel.ManualLoadHandler;

            _player.PlayNextTrackEvent += _playlistViewModel.PlayNextTrackHandler;
            _player.PlayLastTrackEvent += _playlistViewModel.PlayLastTrackHandler;
            _player.PlayNextLibraryTrackEvent += _libraryPlaylistViewModel.PlayNextTrackHandler;
            _player.PlayLastLibraryTrackEvent += _libraryPlaylistViewModel.PlayLastTrackHandler;

            _playlistViewModel.PlayTrackEvent += _player.PlayTrackHandler;
            _libraryPlaylistViewModel.PlayTrackEvent += _player.PlayTrackHandler;
            _libraryPlaylistViewModel.AddLibraryTracksToPlaylistEvent += _playlistViewModel.AddTracksToPlaylistHandler;

            _settingsViewModel.StartScanEvent += StartScanHandler;
            _settingsViewModel.StartScanEvent += _libraryViewModel.StartScanHandler;
            _libraryViewModel.ScanningChanged += _settingsViewModel.ScanningChangedHandler;
            _libraryViewModel.DisplayLibChanged += _libraryPlaylistViewModel.DisplayChangedHandler;
            _libraryViewModel.FiltersCleared += _libraryPlaylistViewModel.ClearKeyFilterHandler;
            _libraryViewModel.AddTracksToPlaylistEvent += _playlistViewModel.AddTracksToPlaylistHandler;

            Player.DataContext = _player;
            Player.ShuffleRepeatButton.DataContext = _shuffleRepeatViewModel;
            MainPlaylist.DataContext = _playlistViewModel;
            SettingsPanel.DataContext = _settingsViewModel;
            Library.DataContext = _libraryViewModel;
            Library.LibraryPlaylist.DataContext = _libraryPlaylistViewModel;

            _settings = new MainWindowSettings();
            ListTabs.SelectedIndex = _settings.ActiveTab;
            Width = _settings.Width;
            Height = _settings.Height;
            // Don't open the window where there is no screen any more.
            var position = new PixelPoint(_settings.Left, _settings.Top);
            if (Screens.ScreenFromPoint(position) is not null)
            {
                Position = position;
            }
            else
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            if (_settings.Maximized) { WindowState = WindowState.Maximized; }
        }

        // Not in OnClosed, by then the window has no platform window any more and its position reads as 0,0.
        protected override void OnClosing(WindowClosingEventArgs e)
        {
            base.OnClosing(e);
            if (e is null || e.Cancel)
            {
                return;
            }

            _settings.ActiveTab = ListTabs.SelectedIndex;
            _settings.Maximized = WindowState == WindowState.Maximized;
            // A maximized window's size and position aren't the ones to restore it to.
            if (WindowState == WindowState.Normal)
            {
                _settings.Left = Position.X;
                _settings.Top = Position.Y;
                _settings.Width = Width;
                _settings.Height = Height;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            try
            {
                _playlistViewModel.SavePlaylist();
            }
            finally
            {
                _audioPlayer.Dispose();
                base.OnClosed(e);
            }
        }

        private void StartScanHandler(object? sender, StartScanEventArgs e)
        {
            ListTabs.SelectedIndex = 1;
        }
    }
}
