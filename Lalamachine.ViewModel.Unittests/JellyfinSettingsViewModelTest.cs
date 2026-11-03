using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibLala.Jellyfin;
using NSubstitute;
using NUnit.Framework;
using Settings;

namespace Lalamachine.ViewModel.Unittests
{
    public class JellyfinSettingsViewModelTest
    {
        private static readonly JellyfinUser s_bob = new JellyfinUser("id-bob", "Bob");
        private static readonly JellyfinUser s_joe = new JellyfinUser("id-joe", "Joe");

        private SettingsFile _settingsFile = null!;
        private JellyfinSettings _settings = null!;
        private IJellyfinClient _client = null!;

        [SetUp]
        public void SetUp()
        {
            _settingsFile = Substitute.For<SettingsFile>("Joe", "Bob");
            _settingsFile.Load().Returns(new Dictionary<string, string>());
            _settings = new JellyfinSettings(_settingsFile);
            _client = Substitute.For<IJellyfinClient>();
        }

        private JellyfinSettingsViewModel CreateViewModel()
        {
            return new JellyfinSettingsViewModel(_settings, (_, _) => _client);
        }

        private void ServerHasUsers(params JellyfinUser[] users)
        {
            _client.GetUsersAsync().Returns(Task.FromResult<IReadOnlyList<JellyfinUser>>(users));
        }

        [Test]
        public void ShowsWhatIsSavedWithoutAskingTheServer()
        {
            _settings.ServerAddress = "http://server:8096";
            _settings.ApiKey = "abc123";
            _settings.UserId = "id-bob";
            _settings.UserName = "Bob";

            var viewModel = CreateViewModel();

            Assert.That(viewModel.ServerAddress, Is.EqualTo("http://server:8096"));
            Assert.That(viewModel.ApiKey, Is.EqualTo("abc123"));
            Assert.That(viewModel.SelectedUser, Is.EqualTo(s_bob));
            Assert.That(viewModel.Users, Is.EqualTo(new[] { s_bob }));
            _client.DidNotReceive().GetUsersAsync();
        }

        [Test]
        public void NothingSavedShowsNoUser()
        {
            var viewModel = CreateViewModel();

            Assert.That(viewModel.SelectedUser, Is.Null);
            Assert.That(viewModel.Users, Is.Empty);
            Assert.That(viewModel.Status, Is.Empty);
        }

        [Test]
        public void TestingAsksTheServerWhatIsTypedIn()
        {
            var created = new List<(string ServerAddress, string ApiKey)>();
            ServerHasUsers(s_bob);
            var viewModel = new JellyfinSettingsViewModel(_settings, (serverAddress, apiKey) =>
            {
                created.Add((serverAddress, apiKey));
                return _client;
            })
            {
                ServerAddress = "http://server:8096",
                ApiKey = "abc123",
            };

            viewModel.TestConnectionCommand.Execute(null);

            Assert.That(created, Is.EqualTo(new[] { ("http://server:8096", "abc123") }));
        }

        [Test]
        public void TestingListsTheUsersAndLeavesTheChoiceWhenThereAreSeveral()
        {
            ServerHasUsers(s_bob, s_joe);
            var viewModel = CreateViewModel();

            viewModel.TestConnectionCommand.Execute(null);

            Assert.That(viewModel.Users, Is.EqualTo(new[] { s_bob, s_joe }));
            Assert.That(viewModel.SelectedUser, Is.Null);
            Assert.That(viewModel.Status, Does.Contain("Choose your user"));
            Assert.That(viewModel.IsTesting, Is.False);
        }

        [Test]
        public void TestingSelectsTheOnlyUser()
        {
            ServerHasUsers(s_bob);
            var viewModel = CreateViewModel();

            viewModel.TestConnectionCommand.Execute(null);

            Assert.That(viewModel.SelectedUser, Is.EqualTo(s_bob));
            Assert.That(viewModel.Status, Does.Contain("Save"));
        }

        [Test]
        public void TestingKeepsTheChosenUserEvenWithANewName()
        {
            _settings.UserId = "id-joe";
            _settings.UserName = "Old name of Joe";
            ServerHasUsers(s_bob, s_joe);
            var viewModel = CreateViewModel();

            viewModel.TestConnectionCommand.Execute(null);

            Assert.That(viewModel.SelectedUser, Is.EqualTo(s_joe));
        }

        [Test]
        public void TestingDropsAUserTheServerDoesntHave()
        {
            _settings.UserId = "id-gone";
            _settings.UserName = "Gone";
            ServerHasUsers(s_bob, s_joe);
            var viewModel = CreateViewModel();

            viewModel.TestConnectionCommand.Execute(null);

            Assert.That(viewModel.SelectedUser, Is.Null);
            Assert.That(viewModel.Users, Is.EqualTo(new[] { s_bob, s_joe }));
        }

        [Test]
        public void TestingDoesntSave()
        {
            ServerHasUsers(s_bob);
            _settingsFile.ClearReceivedCalls();
            var viewModel = CreateViewModel();
            viewModel.ServerAddress = "http://server:8096";
            viewModel.ApiKey = "abc123";

            viewModel.TestConnectionCommand.Execute(null);

            _settingsFile.DidNotReceive().Save(Arg.Any<Dictionary<string, string>>());
        }

        [Test]
        public void FailureShowsItsMessageAndKeepsTheUser()
        {
            _settings.UserId = "id-bob";
            _settings.UserName = "Bob";
            _client.GetUsersAsync().Returns(Task.FromException<IReadOnlyList<JellyfinUser>>(new JellyfinException("The server doesn't accept this API key.")));
            var viewModel = CreateViewModel();

            viewModel.TestConnectionCommand.Execute(null);

            Assert.That(viewModel.Status, Is.EqualTo("The server doesn't accept this API key."));
            Assert.That(viewModel.SelectedUser, Is.EqualTo(s_bob));
            Assert.That(viewModel.IsTesting, Is.False);
        }

        [Test]
        public void AddressThatCantBeUsedShowsItsMessage()
        {
            // Not the fake here, the real client refuses the address when it is created.
            var viewModel = new JellyfinSettingsViewModel(_settings, (serverAddress, apiKey) => new JellyfinClient(serverAddress, apiKey))
            {
                ServerAddress = "server:8096",
                ApiKey = "abc123",
            };

            viewModel.TestConnectionCommand.Execute(null);

            Assert.That(viewModel.Status, Does.Contain("whole address"));
            Assert.That(viewModel.IsTesting, Is.False);
        }

        [Test]
        public void WhileTestingNeitherAnotherTestNorSavingHappens()
        {
            var answer = new TaskCompletionSource<IReadOnlyList<JellyfinUser>>();
            _client.GetUsersAsync().Returns(answer.Task);
            _settingsFile.ClearReceivedCalls();
            var viewModel = CreateViewModel();
            viewModel.TestConnectionCommand.Execute(null);

            Assert.That(viewModel.IsTesting, Is.True);
            Assert.That(viewModel.Status, Is.EqualTo("Connecting..."));
            viewModel.TestConnectionCommand.Execute(null);
            viewModel.SaveCommand.Execute(null);

            _client.Received(1).GetUsersAsync();
            _settingsFile.DidNotReceive().Save(Arg.Any<Dictionary<string, string>>());

            answer.SetResult([s_bob]);
            Assert.That(viewModel.IsTesting, Is.False);
            Assert.That(viewModel.SelectedUser, Is.EqualTo(s_bob));
        }

        [Test]
        public void TypingDoesntSave()
        {
            _settingsFile.ClearReceivedCalls();
            var viewModel = CreateViewModel();

            viewModel.ServerAddress = "http://ser";
            viewModel.ApiKey = "abc";

            _settingsFile.DidNotReceive().Save(Arg.Any<Dictionary<string, string>>());
            Assert.That(_settings.ServerAddress, Is.Empty);
            Assert.That(_settings.ApiKey, Is.Empty);
        }

        [Test]
        public void SavingStoresEverythingWithoutSurroundingSpaces()
        {
            var viewModel = CreateViewModel();
            viewModel.ServerAddress = " http://server:8096 ";
            viewModel.ApiKey = " abc123\n";
            viewModel.SelectedUser = s_bob;

            viewModel.SaveCommand.Execute(null);

            Assert.That(_settings.ServerAddress, Is.EqualTo("http://server:8096"));
            Assert.That(_settings.ApiKey, Is.EqualTo("abc123"));
            Assert.That(_settings.UserId, Is.EqualTo("id-bob"));
            Assert.That(_settings.UserName, Is.EqualTo("Bob"));
            Assert.That(viewModel.Status, Is.EqualTo("Saved."));
        }

        [Test]
        public void SavingWithoutAUserClearsTheOldOne()
        {
            _settings.UserId = "id-bob";
            _settings.UserName = "Bob";
            var viewModel = CreateViewModel();
            viewModel.SelectedUser = null;

            viewModel.SaveCommand.Execute(null);

            Assert.That(_settings.UserId, Is.Empty);
            Assert.That(_settings.UserName, Is.Empty);
        }

        [Test]
        public void SavedValuesComeBackInANewViewModel()
        {
            var viewModel = CreateViewModel();
            viewModel.ServerAddress = "http://server:8096";
            viewModel.ApiKey = "abc123";
            viewModel.SelectedUser = s_joe;
            viewModel.SaveCommand.Execute(null);

            var nextViewModel = CreateViewModel();

            Assert.That(nextViewModel.ServerAddress, Is.EqualTo("http://server:8096"));
            Assert.That(nextViewModel.ApiKey, Is.EqualTo("abc123"));
            Assert.That(nextViewModel.Users.Single(), Is.EqualTo(s_joe));
        }
    }
}
