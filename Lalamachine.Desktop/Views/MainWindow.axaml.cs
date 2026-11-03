using System;
using System.Collections.Generic;
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
        public string PlayingPlaylist { get => GetString(""); set => Set(value); }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "The audio player is disposed when the window closes.")]
    public partial class MainWindow : Window
    {
        private readonly VlcAudioPlayer _audioPlayer;
        private readonly PlayerViewModel _player;
        private readonly QueueViewModel _queueViewModel;
        private readonly PlaylistsViewModel _playlistsViewModel;
        private readonly ShuffleRepeatViewModel _shuffleRepeatViewModel;
        private readonly MainWindowSettings _settings;
        private readonly SettingsViewModel _settingsViewModel;
        private readonly LibraryViewModel _libraryViewModel;
        private readonly LibraryPlaylistViewModel _libraryPlaylistViewModel;
        private readonly AvaloniaDialogService _dialogs;
        private bool _isCloseConfirmed;

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

            _dialogs = new AvaloniaDialogService(this);
            _audioPlayer = new VlcAudioPlayer();
            _player = new PlayerViewModel(_audioPlayer, _dialogs);
            _settingsViewModel = new SettingsViewModel(_dialogs, context);
            _playlistsViewModel = new PlaylistsViewModel(context, _dialogs, _settingsViewModel.JellyfinPusher);
            _queueViewModel = new QueueViewModel(context, _dialogs, _playlistsViewModel);
            _shuffleRepeatViewModel = new ShuffleRepeatViewModel();
            _libraryViewModel = new LibraryViewModel(context, () => new LalaContext(optionsBuilder.Options), _dialogs);
            _libraryPlaylistViewModel = new LibraryPlaylistViewModel(_dialogs, _playlistsViewModel);

            _shuffleRepeatViewModel.ShuffleRepeatChanged += _queueViewModel.ShuffleRepeatChangedHandler;
            _shuffleRepeatViewModel.ShuffleRepeatChanged += _playlistsViewModel.ShuffleRepeatChangedHandler;
            _shuffleRepeatViewModel.ShuffleRepeatChanged += _libraryPlaylistViewModel.ShuffleRepeatChangedHandler;
            _player.ManualLoadEvent += _queueViewModel.ManualLoadHandler;

            _player.PlayNextTrackEvent += _queueViewModel.PlayNextTrackHandler;
            _player.PlayLastTrackEvent += _queueViewModel.PlayLastTrackHandler;
            _player.PlayNextPlaylistTrackEvent += _playlistsViewModel.PlayNextTrackHandler;
            _player.PlayLastPlaylistTrackEvent += _playlistsViewModel.PlayLastTrackHandler;
            _player.PlayNextLibraryTrackEvent += _libraryPlaylistViewModel.PlayNextTrackHandler;
            _player.PlayLastLibraryTrackEvent += _libraryPlaylistViewModel.PlayLastTrackHandler;

            _queueViewModel.PlayTrackEvent += _player.PlayTrackHandler;
            _playlistsViewModel.PlayTrackEvent += _player.PlayTrackHandler;
            _libraryPlaylistViewModel.PlayTrackEvent += _player.PlayTrackHandler;
            _playlistsViewModel.AddTracksToQueueEvent += _queueViewModel.AddTracksToQueueHandler;
            _libraryPlaylistViewModel.AddTracksToQueueEvent += _queueViewModel.AddTracksToQueueHandler;

            _settingsViewModel.StartScanEvent += StartScanHandler;
            _settingsViewModel.StartScanEvent += _libraryViewModel.StartScanHandler;
            _libraryViewModel.ScanningChanged += _settingsViewModel.ScanningChangedHandler;
            _libraryViewModel.ScanningChanged += _playlistsViewModel.ScanningChangedHandler;
            _libraryViewModel.DisplayLibChanged += _libraryPlaylistViewModel.DisplayChangedHandler;
            _libraryViewModel.FiltersCleared += _libraryPlaylistViewModel.ClearKeyFilterHandler;
            _libraryViewModel.AddTracksToQueueEvent += _queueViewModel.AddTracksToQueueHandler;

            Player.DataContext = _player;
            Player.ShuffleRepeatButton.DataContext = _shuffleRepeatViewModel;
            Queue.DataContext = _queueViewModel;
            Playlists.DataContext = _playlistsViewModel;
            SettingsPanel.DataContext = _settingsViewModel;
            Library.DataContext = _libraryViewModel;
            Library.LibraryPlaylist.DataContext = _libraryPlaylistViewModel;

            _settings = new MainWindowSettings();
            _playlistsViewModel.PlayingPlaylistName = _settings.PlayingPlaylist;
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

            // The computer shutting down doesn't wait for an answer.
            var unsaved = _playlistsViewModel.PlaylistsWithUnsavedOrder;
            var isPushing = _playlistsViewModel.IsPushing;
            if ((unsaved.Count > 0 || isPushing) && !_isCloseConfirmed && e.CloseReason != WindowCloseReason.OSShutdown)
            {
                e.Cancel = true;
                ConfirmClose(unsaved, isPushing);
                return;
            }

            _settings.PlayingPlaylist = _playlistsViewModel.PlayingPlaylistName;
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

        private async void ConfirmClose(IList<string> unsavedPlaylists, bool isPushing)
        {
            var unsavedWarning = unsavedPlaylists.Count > 0 ? $"The new order of {string.Join(", ", unsavedPlaylists)} isn't saved and will be lost. " : "";
            var pushWarning = isPushing ? "A playlist is being pushed to Jellyfin, closing now can leave it incomplete there. " : "";
            var isConfirmed = await _dialogs.ConfirmAsync($"{unsavedWarning}{pushWarning}Close anyway?").ConfigureAwait(true);
            if (isConfirmed)
            {
                _isCloseConfirmed = true;
                Close();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            try
            {
                _queueViewModel.SavePlaylist();
            }
            finally
            {
                _audioPlayer.Dispose();
                base.OnClosed(e);
            }
        }

        private void StartScanHandler(object? sender, StartScanEventArgs e)
        {
            ListTabs.SelectedItem = LibraryTab;
        }
    }
}
