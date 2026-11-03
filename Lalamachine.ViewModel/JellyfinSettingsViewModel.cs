using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using LibLala.Jellyfin;
using Settings;

namespace Lalamachine.ViewModel
{
    public class JellyfinSettings : AppSettingsBase
    {
        public JellyfinSettings() : this(new PrivateSettingsFile("Lalamachine.Desktop", "JellyfinSettings")) { }

        public JellyfinSettings(SettingsFile settingsFile) : base(settingsFile) { }

        public string ServerAddress { get => GetString(""); set => Set(value); }
        public string ApiKey { get => GetSecret(); set => SetSecret(value); }

        // The user the playlists belong to, an API key isn't tied to one. The name is there to show it without asking the server.
        public string UserId { get => GetString(""); set => Set(value); }
        public string UserName { get => GetString(""); set => Set(value); }
    }

    // What is typed in here only gets stored by the save command. Other settings write through, but this way half a key
    // doesn't end up on disk, and neither does anything from a server that turned out to be the wrong one.
    public sealed class JellyfinSettingsViewModel : BaseNotifyPropertyChanged
    {
        private readonly JellyfinSettings _settings;
        private readonly Func<string, string, IJellyfinClient> _createClient;
        private readonly IDialogService _dialogs;
        private readonly Func<IEnumerable<PlaylistPaths>> _playlists;
        private readonly Func<string> _libraryPath;
        private string _serverAddress;
        private string _apiKey;
        private JellyfinUser? _selectedUser;
        private string _status = "";
        private bool _isBusy;

        public JellyfinSettingsViewModel(JellyfinSettings settings, Func<string, string, IJellyfinClient> createClient, IDialogService dialogs, Func<IEnumerable<PlaylistPaths>> playlists, Func<string> libraryPath)
        {
            ArgumentNullException.ThrowIfNull(settings);

            _settings = settings;
            _createClient = createClient;
            _dialogs = dialogs;
            _playlists = playlists;
            _libraryPath = libraryPath;
            _serverAddress = settings.ServerAddress;
            _apiKey = settings.ApiKey;
            _testConnectionCommand = new DelegateCommand(OnTestConnection);
            _saveCommand = new DelegateCommand(OnSave);
            _checkPlaylistsCommand = new DelegateCommand(OnCheckPlaylists);

            // The list only has the saved user until the connection gets tested, the app doesn't talk to the server for this.
            Users = new ObservableCollection<JellyfinUser>();
            if (settings.UserId.Length > 0)
            {
                _selectedUser = new JellyfinUser(settings.UserId, settings.UserName);
                Users.Add(_selectedUser);
            }
        }

        public ObservableCollection<JellyfinUser> Users { get; }

        public string ServerAddress
        {
            get => _serverAddress;
            set
            {
                _serverAddress = value;
                NotifyPropertyChanged();
            }
        }

        public string ApiKey
        {
            get => _apiKey;
            set
            {
                _apiKey = value;
                NotifyPropertyChanged();
            }
        }

        public JellyfinUser? SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                NotifyPropertyChanged();
            }
        }

        public string Status
        {
            get => _status;
            private set
            {
                _status = value;
                NotifyPropertyChanged();
            }
        }

        // Talking to the server, the form waits for that.
        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                _isBusy = value;
                NotifyPropertyChanged();
            }
        }

        private readonly DelegateCommand _testConnectionCommand;
        public ICommand TestConnectionCommand => _testConnectionCommand;
        private async void OnTestConnection(object? obj)
        {
            if (IsBusy) { return; }

            IsBusy = true;
            Status = "Connecting...";
            try
            {
                var users = await _createClient(ServerAddress, ApiKey).GetUsersAsync().ConfigureAwait(true);
                ShowUsers(users);
                Status = SelectedUser is null ? "Connected. Choose your user, then save." : "Connected. Save to keep this.";
            }
            catch (JellyfinException e)
            {
                Status = e.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ShowUsers(IReadOnlyList<JellyfinUser> users)
        {
            // First, emptying the list makes the UI clear the selection. Goes by the ID, the user can have a new name by now.
            var selectedId = SelectedUser?.Id;
            Users.Clear();
            foreach (var user in users)
            {
                Users.Add(user);
            }
            SelectedUser = users.FirstOrDefault(x => x.Id == selectedId) ?? (users.Count == 1 ? users[0] : null);
        }

        private readonly DelegateCommand _saveCommand;
        public ICommand SaveCommand => _saveCommand;
        private void OnSave(object? obj)
        {
            if (IsBusy) { return; }

            _settings.ServerAddress = ServerAddress.Trim();
            _settings.ApiKey = ApiKey.Trim();
            _settings.UserId = SelectedUser?.Id ?? "";
            _settings.UserName = SelectedUser?.Name ?? "";
            Status = "Saved.";
        }

        private readonly DelegateCommand _checkPlaylistsCommand;
        public ICommand CheckPlaylistsCommand => _checkPlaylistsCommand;
        private async void OnCheckPlaylists(object? obj)
        {
            if (IsBusy) { return; }

            if (SelectedUser is not { } user)
            {
                Status = "Test the connection and choose your user first.";
                return;
            }

            var libraryPath = _libraryPath();
            if (libraryPath.Length == 0)
            {
                Status = "Choose the library folder first, the check goes by the paths below it.";
                return;
            }

            IsBusy = true;
            Status = "Reading Jellyfin's library...";
            string report;
            try
            {
                var library = await _createClient(ServerAddress, ApiKey).GetAudioLibraryAsync(user.Id).ConfigureAwait(true);
                report = JellyfinPlaylistCheck.Report(libraryPath, library, user.Name, _playlists());
                Status = "";
            }
            catch (JellyfinException e)
            {
                Status = e.Message;
                return;
            }
            finally
            {
                IsBusy = false;
            }

            await _dialogs.ShowMessageAsync(report).ConfigureAwait(true);
        }
    }
}
