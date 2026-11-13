using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibLala.Jellyfin;
using NSubstitute;
using NUnit.Framework;
using Settings;

namespace Lalamachine.ViewModel.Unittests
{
    public class JellyfinPlaylistPullerTest
    {
        private const string LibraryPath = @"D:\Music";

        // The playlists here.
        private sealed class FakePlaylists : ILocalPlaylists
        {
            public Dictionary<string, LocalPlaylist> Existing { get; } = [];
            public List<(string Name, string[] TrackPaths)> Replaced { get; } = [];

            public LocalPlaylist? Find(string name)
            {
                return Existing.Values.FirstOrDefault(x => x.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            }

            public void Replace(string name, IReadOnlyList<string> trackPaths)
            {
                Replaced.Add((name, trackPaths.ToArray()));
            }
        }

        private JellyfinSettings _settings = null!;
        private IJellyfinClient _client = null!;
        private IDialogService _dialogs = null!;
        private FakePlaylists _playlists = null!;
        private List<(string ServerAddress, string ApiKey)> _createdClients = null!;
        private string _libraryPath = "";
        private List<string> _libraryTracks = null!;
        // The tracks of each playlist in Jellyfin, by its name.
        private Dictionary<string, string[]> _serverPlaylists = null!;

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
            _libraryTracks = [];
            _createdClients = [];
            _serverPlaylists = [];
            _playlists = new FakePlaylists();
            _client = Substitute.For<IJellyfinClient>();
            _dialogs = Substitute.For<IDialogService>();

            ServerHas();
            _client.GetPlaylistsAsync(Arg.Any<string>()).Returns(_ =>
                Task.FromResult<IReadOnlyList<JellyfinPlaylist>>(_serverPlaylists.Keys.Select(x => new JellyfinPlaylist($"pl-{x}", x)).ToList()));
            _client.GetPlaylistEntriesAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(x =>
                Task.FromResult<IReadOnlyList<JellyfinPlaylistEntry>>(_serverPlaylists[x.ArgAt<string>(0)["pl-".Length..]].Select(id => new JellyfinPlaylistEntry(id, id, $"Title of {id}")).ToList()));
        }

        private JellyfinPlaylistPuller CreatePuller()
        {
            return new JellyfinPlaylistPuller(_settings, (serverAddress, apiKey) =>
            {
                _createdClients.Add((serverAddress, apiKey));
                return _client;
            }, _dialogs, () => _libraryPath, () => _libraryTracks);
        }

        private Task Pull()
        {
            return CreatePuller().PullAsync(_playlists);
        }

        // Each track is in Jellyfin's library, and the ones that aren't hidden are here as well.
        private void ServerHas(params string[] names)
        {
            var items = names.Select(x => new JellyfinAudioItem($"id-{x}", $"/media/music/{x}.mp3")).ToList();
            _client.GetAudioLibraryAsync(Arg.Any<string>()).Returns(Task.FromResult(new JellyfinAudioLibrary(items, items.Count)));
            _libraryTracks.AddRange(Paths(names));
        }

        private void ServerHasPlaylist(string name, params string[] tracks)
        {
            _serverPlaylists[name] = tracks.Select(x => $"id-{x}").ToArray();
        }

        private void NotHere(string name)
        {
            _libraryTracks.Remove(Path(name));
        }

        private static string Path(string name)
        {
            return $@"{LibraryPath}\{name}.mp3";
        }

        private static string[] Paths(params string[] names)
        {
            return names.Select(Path).ToArray();
        }

        private void UserChooses(string name)
        {
            _dialogs.ChooseAsync(Arg.Any<string>(), Arg.Any<IReadOnlyList<string>>()).Returns(x =>
                Task.FromResult<int?>(x.ArgAt<IReadOnlyList<string>>(1).ToList().IndexOf(name)));
        }

        private void AnswerQuestionWith(bool answer)
        {
            _dialogs.ConfirmAsync(Arg.Any<string>()).Returns(Task.FromResult(answer));
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

            await Pull().ConfigureAwait(false);

            await _dialogs.Received(1).ShowMessageAsync(Arg.Is<string>(x => x.Contains("Set up Jellyfin"))).ConfigureAwait(false);
            Assert.That(_createdClients, Is.Empty);
        }

        [Test]
        public async Task WithoutALibraryFolderItAsksForOne()
        {
            _libraryPath = "";

            await Pull().ConfigureAwait(false);

            await _dialogs.Received(1).ShowMessageAsync(Arg.Is<string>(x => x.Contains("library folder"))).ConfigureAwait(false);
            Assert.That(_createdClients, Is.Empty);
        }

        [Test]
        public async Task OffersJellyfinsPlaylistsByNameWithTheSavedSettings()
        {
            ServerHasPlaylist("techno");
            ServerHasPlaylist("Ambient");
            ServerHasPlaylist("Mix");

            await Pull().ConfigureAwait(false);

            Assert.That(_createdClients, Is.EqualTo(new[] { ("http://server:8096", "abc123") }));
            await _client.Received(1).GetPlaylistsAsync("u1").ConfigureAwait(false);
            await _dialogs.Received(1).ChooseAsync(
                "Which playlist to pull from Jellyfin, on http://server:8096 for Bob?",
                Arg.Is<IReadOnlyList<string>>(x => x.SequenceEqual(new[] { "Ambient", "Mix", "techno" }))).ConfigureAwait(false);
        }

        [Test]
        public async Task WithoutPlaylistsInJellyfinThereIsNothingToChoose()
        {
            await Pull().ConfigureAwait(false);

            await _dialogs.DidNotReceive().ChooseAsync(Arg.Any<string>(), Arg.Any<IReadOnlyList<string>>()).ConfigureAwait(false);
            await _dialogs.Received(1).ShowMessageAsync("Jellyfin has no playlists that Bob can see, on http://server:8096.").ConfigureAwait(false);
        }

        [Test]
        public async Task NotChoosingChangesNothing()
        {
            ServerHas("a");
            ServerHasPlaylist("Mix", "a");
            _dialogs.ChooseAsync(Arg.Any<string>(), Arg.Any<IReadOnlyList<string>>()).Returns(Task.FromResult<int?>(null));

            await Pull().ConfigureAwait(false);

            Assert.That(_playlists.Replaced, Is.Empty);
            await _client.DidNotReceive().GetAudioLibraryAsync(Arg.Any<string>()).ConfigureAwait(false);
            await _dialogs.DidNotReceive().ShowMessageAsync(Arg.Any<string>()).ConfigureAwait(false);
        }

        [Test]
        public async Task ANewPlaylistWithEverythingNeedsNoQuestion()
        {
            ServerHas("a", "b", "c");
            ServerHasPlaylist("Mix", "b", "a");
            UserChooses("Mix");

            await Pull().ConfigureAwait(false);

            await _dialogs.DidNotReceive().ConfirmAsync(Arg.Any<string>()).ConfigureAwait(false);
            Assert.That(_playlists.Replaced, Is.EqualTo(new[] { ("Mix", Paths("b", "a")) }));
            await _dialogs.Received(1).ShowMessageAsync("Created the playlist Mix with 2 tracks from Jellyfin, on http://server:8096 for Bob.").ConfigureAwait(false);
        }

        [Test]
        public async Task ReplacingAsksFirstAndSaysWhatGetsLost()
        {
            ServerHas("a", "b");
            ServerHasPlaylist("Mix", "b", "a");
            _playlists.Existing["mix"] = new LocalPlaylist("mix", 3, false);
            UserChooses("Mix");
            AnswerQuestionWith(true);

            await Pull().ConfigureAwait(false);

            await _dialogs.Received(1).ConfirmAsync(Arg.Is<string>(x =>
                x.Contains("Replace the 3 tracks of the playlist mix here with the 2 it has in Jellyfin, on http://server:8096 for Bob?")
                && x.Contains("get lost")
                && !x.Contains("hearts"))).ConfigureAwait(false);
            // It keeps its name here.
            Assert.That(_playlists.Replaced, Is.EqualTo(new[] { ("mix", Paths("b", "a")) }));
            await _dialogs.Received(1).ShowMessageAsync("The playlist mix here has these 2 tracks from Jellyfin now, on http://server:8096 for Bob.").ConfigureAwait(false);
        }

        [Test]
        public async Task SayingNoToReplacingChangesNothing()
        {
            ServerHas("a");
            ServerHasPlaylist("Mix", "a");
            _playlists.Existing["Mix"] = new LocalPlaylist("Mix", 1, false);
            UserChooses("Mix");
            AnswerQuestionWith(false);

            await Pull().ConfigureAwait(false);

            Assert.That(_playlists.Replaced, Is.Empty);
            await _dialogs.DidNotReceive().ShowMessageAsync(Arg.Any<string>()).ConfigureAwait(false);
        }

        [Test]
        public async Task ReplacingLikedSaysThatTheHeartsChange()
        {
            ServerHas("a");
            ServerHasPlaylist("Liked", "a");
            _playlists.Existing["Liked"] = new LocalPlaylist(LibLala.Constants.LIKEDPLAYLISTNAME, 4, false);
            UserChooses("Liked");
            AnswerQuestionWith(false);

            await Pull().ConfigureAwait(false);

            await _dialogs.Received(1).ConfirmAsync(Arg.Is<string>(x => x.Contains("hearts"))).ConfigureAwait(false);
        }

        [Test]
        public async Task ANewOrderThatIsntSavedIsntReplaced()
        {
            ServerHas("a");
            ServerHasPlaylist("Mix", "a");
            _playlists.Existing["Mix"] = new LocalPlaylist("Mix", 1, true);
            UserChooses("Mix");
            AnswerQuestionWith(true);

            await Pull().ConfigureAwait(false);

            Assert.That(_playlists.Replaced, Is.Empty);
            await _dialogs.DidNotReceive().ConfirmAsync(Arg.Any<string>()).ConfigureAwait(false);
            await _dialogs.Received(1).ShowMessageAsync(Arg.Is<string>(x => x.Contains("The playlist Mix here has a new order that isn't saved"))).ConfigureAwait(false);
        }

        [Test]
        public async Task MissingTracksAreAskedAboutAndReported()
        {
            ServerHas("a", "b", "c");
            NotHere("b");
            ServerHasPlaylist("Mix", "c", "b", "a");
            UserChooses("Mix");
            AnswerQuestionWith(true);

            await Pull().ConfigureAwait(false);

            await _dialogs.Received(1).ConfirmAsync(Arg.Is<string>(x =>
                x.Contains("Create the playlist Mix here with 2 of the 3 tracks it has in Jellyfin, on http://server:8096 for Bob?")
                && x.Contains($@"Not in the library here, below {LibraryPath} (1):")
                && x.Contains("/media/music/b.mp3"))).ConfigureAwait(false);
            Assert.That(_playlists.Replaced, Is.EqualTo(new[] { ("Mix", Paths("c", "a")) }));
            await _dialogs.Received(1).ShowMessageAsync("Created the playlist Mix with 2 tracks from Jellyfin, on http://server:8096 for Bob. 1 tracks were left out.").ConfigureAwait(false);
        }

        [Test]
        public async Task SayingNoAboutMissingTracksCreatesNothing()
        {
            ServerHas("a", "b");
            NotHere("b");
            ServerHasPlaylist("Mix", "a", "b");
            UserChooses("Mix");
            AnswerQuestionWith(false);

            await Pull().ConfigureAwait(false);

            Assert.That(_playlists.Replaced, Is.Empty);
        }

        [Test]
        public async Task ATrackThatIsntAudioIsListedByItsTitle()
        {
            ServerHas("a");
            _serverPlaylists["Mix"] = ["id-a", "video"];
            UserChooses("Mix");
            AnswerQuestionWith(false);

            await Pull().ConfigureAwait(false);

            await _dialogs.Received(1).ConfirmAsync(Arg.Is<string>(x => x.Contains("Not among Jellyfin's audio tracks (1):") && x.Contains("Title of video"))).ConfigureAwait(false);
        }

        [Test]
        public async Task AnIncompleteReadIsMentionedWhenATrackIsntAmongTheAudioTracks()
        {
            var items = new List<JellyfinAudioItem> { new("id-a", "/media/music/a.mp3") };
            _client.GetAudioLibraryAsync(Arg.Any<string>()).Returns(Task.FromResult(new JellyfinAudioLibrary(items, 3)));
            _libraryTracks.Add(Path("a"));
            _serverPlaylists["Mix"] = ["id-a", "id-unread"];
            UserChooses("Mix");
            AnswerQuestionWith(false);

            await Pull().ConfigureAwait(false);

            await _dialogs.Received(1).ConfirmAsync(Arg.Is<string>(x => x.Contains("2 of the tracks Jellyfin reports couldn't be read"))).ConfigureAwait(false);
        }

        [Test]
        public async Task NothingFoundChangesNothingNotEvenAnExistingPlaylist()
        {
            ServerHas("gone");
            NotHere("gone");
            ServerHasPlaylist("Mix", "gone");
            _playlists.Existing["Mix"] = new LocalPlaylist("Mix", 1, false);
            UserChooses("Mix");

            await Pull().ConfigureAwait(false);

            Assert.That(_playlists.Replaced, Is.Empty);
            await _dialogs.DidNotReceive().ConfirmAsync(Arg.Any<string>()).ConfigureAwait(false);
            await _dialogs.Received(1).ShowMessageAsync(Arg.Is<string>(x => x.Contains("None of the tracks of Mix in Jellyfin are in the library here") && x.Contains("/media/music/gone.mp3"))).ConfigureAwait(false);
        }

        [Test]
        public async Task AnEmptyPlaylistChangesNothing()
        {
            ServerHasPlaylist("Mix");
            _playlists.Existing["Mix"] = new LocalPlaylist("Mix", 1, false);
            UserChooses("Mix");

            await Pull().ConfigureAwait(false);

            Assert.That(_playlists.Replaced, Is.Empty);
            await _dialogs.Received(1).ShowMessageAsync("The playlist Mix in Jellyfin has no tracks, so nothing was changed.").ConfigureAwait(false);
        }

        [Test]
        public async Task ThePlaylistCantHaveTheNameOfTheQueue()
        {
            ServerHas("a");
            ServerHasPlaylist(LibLala.Constants.MISCPLAYLISTNAME, "a");
            UserChooses(LibLala.Constants.MISCPLAYLISTNAME);

            await Pull().ConfigureAwait(false);

            Assert.That(_playlists.Replaced, Is.Empty);
            await _dialogs.Received(1).ShowMessageAsync(Arg.Is<string>(x => x.Contains("can't be called"))).ConfigureAwait(false);
        }

        [Test]
        public async Task AFailureIsShownAsAMessage()
        {
            ServerHas("a");
            ServerHasPlaylist("Mix", "a");
            UserChooses("Mix");
            _client.GetAudioLibraryAsync(Arg.Any<string>()).Returns(Task.FromException<JellyfinAudioLibrary>(new JellyfinException("The server didn't answer in time.")));

            await Pull().ConfigureAwait(false);

            await _dialogs.Received(1).ShowMessageAsync("The server didn't answer in time.").ConfigureAwait(false);
            Assert.That(_playlists.Replaced, Is.Empty);
        }

        [Test]
        public async Task AnAddressThatCantBeUsedIsShownAsAMessage()
        {
            _settings.ServerAddress = "server:8096";
            var puller = new JellyfinPlaylistPuller(_settings, (serverAddress, apiKey) => new JellyfinClient(serverAddress, apiKey), _dialogs, () => _libraryPath, () => _libraryTracks);

            await puller.PullAsync(_playlists).ConfigureAwait(false);

            await _dialogs.Received(1).ShowMessageAsync(Arg.Is<string>(x => x.Contains("whole address"))).ConfigureAwait(false);
        }
    }
}
