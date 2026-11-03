using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibLala.Jellyfin;
using NSubstitute;
using NUnit.Framework;
using Settings;

namespace Lalamachine.ViewModel.Unittests
{
    public class JellyfinPlaylistPusherTest
    {
        private const string LibraryPath = @"D:\Music";

        private JellyfinSettings _settings = null!;
        private IJellyfinClient _client = null!;
        private IDialogService _dialogs = null!;
        private List<(string ServerAddress, string ApiKey)> _createdClients = null!;
        private string _libraryPath = "";
        // What the server has in the one playlist there is, or would have after a push.
        private List<string> _serverEntries = null!;

        [SetUp]
        public void SetUp()
        {
            var settingsFile = Substitute.For<SettingsFile>("Joe", "Bob");
            settingsFile.Load().Returns(new Dictionary<string, string>());
            _settings = new JellyfinSettings(settingsFile)
            {
                ServerAddress = "http://server:8096",
                ApiKey = "abc123",
                UserId = "u1",
                UserName = "Bob",
            };
            _libraryPath = LibraryPath;
            _createdClients = [];
            _serverEntries = [];
            _client = Substitute.For<IJellyfinClient>();
            _dialogs = Substitute.For<IDialogService>();

            ServerHas();
            ServerHasPlaylists();
            _client.GetPlaylistEntriesAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(_ =>
                Task.FromResult<IReadOnlyList<JellyfinPlaylistEntry>>(_serverEntries.Select(x => new JellyfinPlaylistEntry(x, x)).ToList()));
            _client.CreatePlaylistAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IReadOnlyList<string>>()).Returns(x =>
            {
                _serverEntries = x.ArgAt<IReadOnlyList<string>>(2).ToList();
                return Task.FromResult("new-id");
            });
            _client.AddToPlaylistAsync(Arg.Any<string>(), Arg.Any<IReadOnlyList<string>>(), Arg.Any<string>()).Returns(x =>
            {
                _serverEntries.AddRange(x.ArgAt<IReadOnlyList<string>>(1));
                return Task.CompletedTask;
            });
            _client.RemoveFromPlaylistAsync(Arg.Any<string>(), Arg.Any<IReadOnlyList<string>>()).Returns(x =>
            {
                _serverEntries.RemoveAll(x.ArgAt<IReadOnlyList<string>>(1).Contains);
                return Task.CompletedTask;
            });
        }

        private JellyfinPlaylistPusher CreatePusher()
        {
            return new JellyfinPlaylistPusher(_settings, (serverAddress, apiKey) =>
            {
                _createdClients.Add((serverAddress, apiKey));
                return _client;
            }, _dialogs, () => _libraryPath);
        }

        private void ServerHas(params string[] names)
        {
            var items = names.Select(x => new JellyfinAudioItem($"id-{x}", $"/media/music/{x}.mp3")).ToList();
            _client.GetAudioLibraryAsync(Arg.Any<string>()).Returns(Task.FromResult(new JellyfinAudioLibrary(items, items.Count)));
        }

        private void ServerHasPlaylists(params string[] names)
        {
            _client.GetPlaylistsAsync(Arg.Any<string>()).Returns(Task.FromResult<IReadOnlyList<JellyfinPlaylist>>(names.Select(x => new JellyfinPlaylist($"pl-{x}", x)).ToList()));
        }

        private static string[] Paths(params string[] names)
        {
            return names.Select(x => $@"{LibraryPath}\{x}.mp3").ToArray();
        }

        private void AnswerQuestionWith(bool answer)
        {
            _dialogs.ConfirmAsync(Arg.Any<string>()).Returns(Task.FromResult(answer));
        }

        private void NothingWasWritten()
        {
            _client.DidNotReceive().CreatePlaylistAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IReadOnlyList<string>>());
            _client.DidNotReceive().AddToPlaylistAsync(Arg.Any<string>(), Arg.Any<IReadOnlyList<string>>(), Arg.Any<string>());
            _client.DidNotReceive().RemoveFromPlaylistAsync(Arg.Any<string>(), Arg.Any<IReadOnlyList<string>>());
        }

        [TestCase("ServerAddress")]
        [TestCase("ApiKey")]
        [TestCase("UserId")]
        public async Task WithoutCompleteSettingsItAsksToSetThemUp(string missing)
        {
            switch (missing)
            {
                case "ServerAddress": _settings.ServerAddress = ""; break;
                case "ApiKey": _settings.ApiKey = ""; break;
                default: _settings.UserId = ""; break;
            }

            await CreatePusher().PushAsync("Mix", Paths("a")).ConfigureAwait(false);

            await _dialogs.Received(1).ShowMessageAsync(Arg.Is<string>(x => x.Contains("Set up Jellyfin"))).ConfigureAwait(false);
            Assert.That(_createdClients, Is.Empty);
        }

        [Test]
        public async Task WithoutALibraryFolderItAsksForOne()
        {
            _libraryPath = "";

            await CreatePusher().PushAsync("Mix", Paths("a")).ConfigureAwait(false);

            await _dialogs.Received(1).ShowMessageAsync(Arg.Is<string>(x => x.Contains("library folder"))).ConfigureAwait(false);
            Assert.That(_createdClients, Is.Empty);
        }

        [Test]
        public async Task AnEmptyPlaylistIsntPushed()
        {
            await CreatePusher().PushAsync("Mix", []).ConfigureAwait(false);

            await _dialogs.Received(1).ShowMessageAsync(Arg.Is<string>(x => x.Contains("no tracks"))).ConfigureAwait(false);
            Assert.That(_createdClients, Is.Empty);
        }

        [Test]
        public async Task UsesTheSavedSettings()
        {
            ServerHas("a");

            await CreatePusher().PushAsync("Mix", Paths("a")).ConfigureAwait(false);

            Assert.That(_createdClients, Is.EqualTo(new[] { ("http://server:8096", "abc123") }));
            await _client.Received().GetAudioLibraryAsync("u1").ConfigureAwait(false);
        }

        [Test]
        public async Task ANewPlaylistWithEverythingNeedsNoQuestion()
        {
            ServerHas("a", "b");

            await CreatePusher().PushAsync("Mix", Paths("b", "a")).ConfigureAwait(false);

            await _dialogs.DidNotReceive().ConfirmAsync(Arg.Any<string>()).ConfigureAwait(false);
            Assert.That(_serverEntries, Is.EqualTo(new[] { "id-b", "id-a" }));
            await _dialogs.Received(1).ShowMessageAsync("Created the playlist Mix in Jellyfin with 2 tracks, on http://server:8096 for Bob.").ConfigureAwait(false);
        }

        [Test]
        public async Task ReplacingAsksFirstAndSaysWhatGetsLost()
        {
            ServerHas("a", "b");
            ServerHasPlaylists("Mix");
            _serverEntries = ["id-a", "id-x", "id-y"];
            AnswerQuestionWith(true);

            await CreatePusher().PushAsync("Mix", Paths("b", "a")).ConfigureAwait(false);

            await _dialogs.Received(1).ConfirmAsync(Arg.Is<string>(x => x.Contains("Replace the 3 tracks of the playlist Mix in Jellyfin with these 2, on http://server:8096 for Bob?") && x.Contains("get lost"))).ConfigureAwait(false);
            Assert.That(_serverEntries, Is.EqualTo(new[] { "id-b", "id-a" }));
            await _dialogs.Received(1).ShowMessageAsync("The playlist Mix in Jellyfin has these 2 tracks now, on http://server:8096 for Bob.").ConfigureAwait(false);
        }

        [Test]
        public async Task SayingNoToReplacingChangesNothing()
        {
            ServerHas("a");
            ServerHasPlaylists("Mix");
            _serverEntries = ["id-x"];
            AnswerQuestionWith(false);

            await CreatePusher().PushAsync("Mix", Paths("a")).ConfigureAwait(false);

            NothingWasWritten();
            Assert.That(_serverEntries, Is.EqualTo(new[] { "id-x" }));
            await _dialogs.DidNotReceive().ShowMessageAsync(Arg.Any<string>()).ConfigureAwait(false);
        }

        [Test]
        public async Task MissingTracksAreAskedAboutAndReported()
        {
            ServerHas("a", "c");
            AnswerQuestionWith(true);

            await CreatePusher().PushAsync("Mix", Paths("c", "gone", "a")).ConfigureAwait(false);

            await _dialogs.Received(1).ConfirmAsync(Arg.Is<string>(x => x.Contains("with 2 of its 3 tracks, on http://server:8096 for Bob?") && x.Contains("Not found (1):") && x.Contains("gone.mp3"))).ConfigureAwait(false);
            Assert.That(_serverEntries, Is.EqualTo(new[] { "id-c", "id-a" }));
            await _dialogs.Received(1).ShowMessageAsync("Created the playlist Mix in Jellyfin with 2 tracks, on http://server:8096 for Bob. 1 tracks were left out.").ConfigureAwait(false);
        }

        private void ServerReportsMoreThanItGives(params string[] names)
        {
            var items = names.Select(x => new JellyfinAudioItem($"id-{x}", $"/media/music/{x}.mp3")).ToList();
            _client.GetAudioLibraryAsync(Arg.Any<string>()).Returns(Task.FromResult(new JellyfinAudioLibrary(items, items.Count + 2)));
        }

        [Test]
        public async Task AnIncompleteReadIsMentionedWhenATrackIsNotFound()
        {
            ServerReportsMoreThanItGives("a");
            AnswerQuestionWith(false);

            await CreatePusher().PushAsync("Mix", Paths("a", "gone")).ConfigureAwait(false);

            await _dialogs.Received(1).ConfirmAsync(Arg.Is<string>(x => x.Contains("2 of the tracks Jellyfin reports couldn't be read") && x.Contains("Not found (1):"))).ConfigureAwait(false);
        }

        [Test]
        public async Task AnIncompleteReadIsMentionedWhenNoTrackIsFound()
        {
            ServerReportsMoreThanItGives("a");

            await CreatePusher().PushAsync("Mix", Paths("gone")).ConfigureAwait(false);

            await _dialogs.Received(1).ShowMessageAsync(Arg.Is<string>(x => x.Contains("2 of the tracks Jellyfin reports couldn't be read"))).ConfigureAwait(false);
        }

        [Test]
        public async Task AnIncompleteReadDoesntMatterWhenEverythingIsFound()
        {
            ServerReportsMoreThanItGives("a");

            await CreatePusher().PushAsync("Mix", Paths("a")).ConfigureAwait(false);

            await _dialogs.DidNotReceive().ConfirmAsync(Arg.Any<string>()).ConfigureAwait(false);
            await _dialogs.Received(1).ShowMessageAsync("Created the playlist Mix in Jellyfin with 1 tracks, on http://server:8096 for Bob.").ConfigureAwait(false);
        }

        [Test]
        public async Task AnIncompleteReadIsNotPartOfTheQuestionWhenEverythingIsFound()
        {
            ServerReportsMoreThanItGives("a");
            ServerHasPlaylists("Mix");
            AnswerQuestionWith(false);

            await CreatePusher().PushAsync("Mix", Paths("a")).ConfigureAwait(false);

            await _dialogs.Received(1).ConfirmAsync(Arg.Is<string>(x => !x.Contains("couldn't be read"))).ConfigureAwait(false);
        }

        [Test]
        public async Task SayingNoAboutMissingTracksCreatesNothing()
        {
            ServerHas("a");
            AnswerQuestionWith(false);

            await CreatePusher().PushAsync("Mix", Paths("a", "gone")).ConfigureAwait(false);

            NothingWasWritten();
        }

        [Test]
        public async Task NothingFoundChangesNothingNotEvenAnExistingPlaylist()
        {
            ServerHas("other");
            ServerHasPlaylists("Mix");
            _serverEntries = ["id-x"];

            await CreatePusher().PushAsync("Mix", Paths("gone")).ConfigureAwait(false);

            NothingWasWritten();
            await _dialogs.DidNotReceive().ConfirmAsync(Arg.Any<string>()).ConfigureAwait(false);
            await _dialogs.Received(1).ShowMessageAsync(Arg.Is<string>(x => x.Contains("None of the tracks of Mix are in Jellyfin") && x.Contains("gone.mp3"))).ConfigureAwait(false);
        }

        [Test]
        public async Task AFailureIsShownAsAMessage()
        {
            _client.GetAudioLibraryAsync(Arg.Any<string>()).Returns(Task.FromException<JellyfinAudioLibrary>(new JellyfinException("The server didn't answer in time.")));

            await CreatePusher().PushAsync("Mix", Paths("a")).ConfigureAwait(false);

            await _dialogs.Received(1).ShowMessageAsync("The server didn't answer in time.").ConfigureAwait(false);
            NothingWasWritten();
        }

        [Test]
        public async Task AnAddressThatCantBeUsedIsShownAsAMessage()
        {
            _settings.ServerAddress = "server:8096";
            var pusher = new JellyfinPlaylistPusher(_settings, (serverAddress, apiKey) => new JellyfinClient(serverAddress, apiKey), _dialogs, () => _libraryPath);

            await pusher.PushAsync("Mix", Paths("a")).ConfigureAwait(false);

            await _dialogs.Received(1).ShowMessageAsync(Arg.Is<string>(x => x.Contains("whole address"))).ConfigureAwait(false);
        }
    }
}
