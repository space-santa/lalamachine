using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LibLala.Jellyfin;
using NUnit.Framework;

namespace LibLala.Unittests.Jellyfin
{
    // Behaves like Jellyfin's source reads: adding keeps duplicates and checks who it is, removing goes by the track and doesn't
    // check anything. So these tests look at what's in the playlist in the end, not only at which calls were made.
    internal sealed class FakeJellyfin : IJellyfinClient
    {
        internal sealed class Playlist
        {
            public required string Name { get; init; }
            public required string Owner { get; init; }
            public List<string> Items { get; init; } = [];
        }

        public const string UserId = "me";

        public List<JellyfinAudioItem> Tracks { get; } = [];
        public Dictionary<string, Playlist> Playlists { get; } = [];
        public List<string> Writes { get; } = [];
        public string? FailOnWrite { get; set; }
        // Older versions: the ID of an entry is its own, adding leaves out what's already there.
        public bool EntriesHaveTheirOwnIds { get; set; }
        public bool AddingLeavesOutDuplicates { get; set; }
        public bool SortsWhatIsAdded { get; set; }

        public Task<IReadOnlyList<JellyfinUser>> GetUsersAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<JellyfinUser>>([new JellyfinUser(UserId, "Me")]);
        }

        public Task<JellyfinAudioLibrary> GetAudioLibraryAsync(string userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new JellyfinAudioLibrary(Tracks, Tracks.Count));
        }

        public Task<IReadOnlyList<JellyfinPlaylist>> GetPlaylistsAsync(string userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<JellyfinPlaylist>>(Playlists.Select(x => new JellyfinPlaylist(x.Key, x.Value.Name)).ToList());
        }

        public Task<IReadOnlyList<JellyfinPlaylistEntry>> GetPlaylistEntriesAsync(string playlistId, string userId, CancellationToken cancellationToken = default)
        {
            var items = Playlists[playlistId].Items;
            return Task.FromResult<IReadOnlyList<JellyfinPlaylistEntry>>(items.Select((x, i) => new JellyfinPlaylistEntry(x, EntriesHaveTheirOwnIds ? $"entry{i}-{x}" : x)).ToList());
        }

        public Task<string> CreatePlaylistAsync(string name, string userId, IReadOnlyList<string> itemIds, CancellationToken cancellationToken = default)
        {
            Write("create");
            var id = $"new{Playlists.Count}";
            Playlists[id] = new Playlist { Name = name, Owner = userId, Items = itemIds.ToList() };
            return Task.FromResult(id);
        }

        public Task AddToPlaylistAsync(string playlistId, IReadOnlyList<string> itemIds, string userId, CancellationToken cancellationToken = default)
        {
            var playlist = Playlists[playlistId];
            if (playlist.Owner != userId)
            {
                throw new JellyfinException("Jellyfin doesn't allow this user to change that playlist.");
            }

            Write("add");
            playlist.Items.AddRange(AddingLeavesOutDuplicates ? itemIds.Except(playlist.Items) : itemIds);
            if (SortsWhatIsAdded)
            {
                playlist.Items.Sort(StringComparer.Ordinal);
            }
            return Task.CompletedTask;
        }

        public Task RemoveFromPlaylistAsync(string playlistId, IReadOnlyList<string> entryIds, CancellationToken cancellationToken = default)
        {
            Write("remove");
            var playlist = Playlists[playlistId];
            if (EntriesHaveTheirOwnIds)
            {
                var kept = playlist.Items.Where((x, i) => !entryIds.Contains($"entry{i}-{x}")).ToList();
                playlist.Items.Clear();
                playlist.Items.AddRange(kept);
            }
            else
            {
                playlist.Items.RemoveAll(entryIds.Contains);
            }
            return Task.CompletedTask;
        }

        private void Write(string operation)
        {
            // Only the second time it comes up for "add", the first one is the check for permission.
            if (FailOnWrite == operation && (operation != "add" || Writes.Contains("add")))
            {
                throw new JellyfinException("Can't reach the server: gone.");
            }
            Writes.Add(operation);
        }
    }

    public class JellyfinPlaylistPublisherTest
    {
        private const string LibraryRoot = @"D:\Music";

        private FakeJellyfin _jellyfin = null!;
        private JellyfinPlaylistPublisher _publisher = null!;

        [SetUp]
        public void SetUp()
        {
            _jellyfin = new FakeJellyfin();
            foreach (var name in new[] { "a", "b", "c", "d" })
            {
                _jellyfin.Tracks.Add(new JellyfinAudioItem($"id-{name}", $"/media/music/{name}.mp3"));
            }
            _publisher = new JellyfinPlaylistPublisher(_jellyfin, FakeJellyfin.UserId);
        }

        private static string Local(string name)
        {
            return $@"{LibraryRoot}\{name}.mp3";
        }

        private static IEnumerable<string> Locals(params string[] names)
        {
            return names.Select(Local);
        }

        private Task<JellyfinPushPlan> Prepare(string playlist, params string[] names)
        {
            return _publisher.PrepareAsync(playlist, LibraryRoot, Locals(names));
        }

        private async Task Push(string playlist, params string[] names)
        {
            await _publisher.PublishAsync(await Prepare(playlist, names).ConfigureAwait(false)).ConfigureAwait(false);
        }

        [Test]
        public async Task PreparingChangesNothing()
        {
            _jellyfin.Playlists["p1"] = new FakeJellyfin.Playlist { Name = "Mix", Owner = FakeJellyfin.UserId, Items = ["id-a"] };

            await Prepare("Mix", "b", "a").ConfigureAwait(false);

            Assert.That(_jellyfin.Writes, Is.Empty);
            Assert.That(_jellyfin.Playlists["p1"].Items, Is.EqualTo(new[] { "id-a" }));
        }

        [Test]
        public async Task PlanHasTheFoundTracksInOrderAndTheRestSeparately()
        {
            var plan = await Prepare("Mix", "c", "missing", "a").ConfigureAwait(false);

            Assert.That(plan.ItemIds, Is.EqualTo(new[] { "id-c", "id-a" }));
            Assert.That(plan.Unresolved.Single().RelativePath, Is.EqualTo("missing.mp3"));
            Assert.That(plan.ExistingPlaylistId, Is.Null);
        }

        [Test]
        public async Task ATrackFoundTwiceIsInThePlanOnce()
        {
            var plan = await _publisher.PrepareAsync("Mix", LibraryRoot, [Local("a"), Local("A"), Local("b")]).ConfigureAwait(false);

            Assert.That(plan.ItemIds, Is.EqualTo(new[] { "id-a", "id-b" }));
        }

        [Test]
        public async Task PlanFindsTheExistingPlaylistWhateverTheCase()
        {
            _jellyfin.Playlists["p1"] = new FakeJellyfin.Playlist { Name = "MIX", Owner = FakeJellyfin.UserId, Items = ["id-a", "id-b"] };

            var plan = await Prepare("Mix", "a").ConfigureAwait(false);

            Assert.That(plan.ExistingPlaylistId, Is.EqualTo("p1"));
            Assert.That(plan.ExistingTrackCount, Is.EqualTo(2));
        }

        [Test]
        public void TwoPlaylistsWithTheNameAreNotAGuess()
        {
            _jellyfin.Playlists["p1"] = new FakeJellyfin.Playlist { Name = "Mix", Owner = FakeJellyfin.UserId };
            _jellyfin.Playlists["p2"] = new FakeJellyfin.Playlist { Name = "Mix", Owner = FakeJellyfin.UserId };

            Assert.That(async () => await Prepare("Mix", "a").ConfigureAwait(false), Throws.TypeOf<JellyfinException>().With.Message.Contains("2 playlists called Mix"));
            Assert.That(_jellyfin.Writes, Is.Empty);
        }

        [Test]
        public async Task ANewPlaylistHasTheTracksInOrder()
        {
            await Push("Mix", "c", "a", "d").ConfigureAwait(false);

            var created = _jellyfin.Playlists.Values.Single();
            Assert.That(created.Name, Is.EqualTo("Mix"));
            Assert.That(created.Owner, Is.EqualTo(FakeJellyfin.UserId));
            Assert.That(created.Items, Is.EqualTo(new[] { "id-c", "id-a", "id-d" }));
        }

        [Test]
        public async Task ReplacingKeepsTheIdAndGivesExactlyTheNewOrder()
        {
            _jellyfin.Playlists["p1"] = new FakeJellyfin.Playlist { Name = "Mix", Owner = FakeJellyfin.UserId, Items = ["id-a", "id-b", "id-c"] };

            // Tracks in both, a track gone, a track new, in another order. Adding before removing would lose the ones in both.
            await Push("Mix", "d", "c", "a").ConfigureAwait(false);

            Assert.That(_jellyfin.Playlists.Keys, Is.EqualTo(new[] { "p1" }));
            Assert.That(_jellyfin.Playlists["p1"].Items, Is.EqualTo(new[] { "id-d", "id-c", "id-a" }));
        }

        [Test]
        public async Task PushingTwiceChangesNothingTheSecondTime()
        {
            await Push("Mix", "c", "a").ConfigureAwait(false);
            await Push("Mix", "c", "a").ConfigureAwait(false);

            Assert.That(_jellyfin.Playlists.Values.Single().Items, Is.EqualTo(new[] { "id-c", "id-a" }));
        }

        [Test]
        public async Task ReplacingWorksWhenEntriesHaveTheirOwnIds()
        {
            _jellyfin.EntriesHaveTheirOwnIds = true;
            _jellyfin.Playlists["p1"] = new FakeJellyfin.Playlist { Name = "Mix", Owner = FakeJellyfin.UserId, Items = ["id-a", "id-b"] };

            await Push("Mix", "b", "a").ConfigureAwait(false);

            Assert.That(_jellyfin.Playlists["p1"].Items, Is.EqualTo(new[] { "id-b", "id-a" }));
        }

        [Test]
        public async Task ReplacingWorksWhenAddingLeavesOutDuplicates()
        {
            _jellyfin.AddingLeavesOutDuplicates = true;
            _jellyfin.Playlists["p1"] = new FakeJellyfin.Playlist { Name = "Mix", Owner = FakeJellyfin.UserId, Items = ["id-a", "id-b"] };

            await Push("Mix", "b", "a").ConfigureAwait(false);

            Assert.That(_jellyfin.Playlists["p1"].Items, Is.EqualTo(new[] { "id-b", "id-a" }));
        }

        [Test]
        public async Task AnotherUsersPlaylistStaysUntouched()
        {
            _jellyfin.Playlists["theirs"] = new FakeJellyfin.Playlist { Name = "Mix", Owner = "someone else", Items = ["id-a", "id-b"] };
            var plan = await Prepare("Mix", "c").ConfigureAwait(false);

            await Assert.ThatAsync(async () => await _publisher.PublishAsync(plan).ConfigureAwait(false), Throws.TypeOf<JellyfinException>().With.Message.EqualTo("Jellyfin doesn't allow this user to change that playlist.")).ConfigureAwait(false);

            Assert.That(_jellyfin.Playlists["theirs"].Items, Is.EqualTo(new[] { "id-a", "id-b" }));
            Assert.That(_jellyfin.Writes, Is.Empty);
        }

        [Test]
        public async Task FailingAfterRemovingSaysThatItCanBeIncomplete()
        {
            _jellyfin.Playlists["p1"] = new FakeJellyfin.Playlist { Name = "Mix", Owner = FakeJellyfin.UserId, Items = ["id-a", "id-b"] };
            var plan = await Prepare("Mix", "b", "a").ConfigureAwait(false);
            _jellyfin.FailOnWrite = "add";

            await Assert.ThatAsync(async () => await _publisher.PublishAsync(plan).ConfigureAwait(false), Throws.TypeOf<JellyfinException>().With.Message.Contains("can be incomplete now")).ConfigureAwait(false);
            Assert.That(_jellyfin.Writes, Does.Contain("remove"));

            // Pushing again puts it right.
            _jellyfin.FailOnWrite = null;
            await Push("Mix", "b", "a").ConfigureAwait(false);
            Assert.That(_jellyfin.Playlists["p1"].Items, Is.EqualTo(new[] { "id-b", "id-a" }));
        }

        [Test]
        public async Task AServerThatDoesntKeepTheOrderIsReported()
        {
            _jellyfin.SortsWhatIsAdded = true;
            _jellyfin.Playlists["p1"] = new FakeJellyfin.Playlist { Name = "Mix", Owner = FakeJellyfin.UserId, Items = ["id-a"] };
            var plan = await Prepare("Mix", "c", "b").ConfigureAwait(false);

            await Assert.ThatAsync(async () => await _publisher.PublishAsync(plan).ConfigureAwait(false), Throws.TypeOf<JellyfinException>().With.Message.Contains("didn't come out as it should: its tracks are in another order")).ConfigureAwait(false);
        }

        [Test]
        public async Task APlanWithoutTracksIsNeverPublished()
        {
            _jellyfin.Playlists["p1"] = new FakeJellyfin.Playlist { Name = "Mix", Owner = FakeJellyfin.UserId, Items = ["id-a"] };
            var plan = await Prepare("Mix", "missing").ConfigureAwait(false);

            Assert.That(plan.ItemIds, Is.Empty);
            await Assert.ThatAsync(async () => await _publisher.PublishAsync(plan).ConfigureAwait(false), Throws.ArgumentException).ConfigureAwait(false);
            Assert.That(_jellyfin.Writes, Is.Empty);
            Assert.That(_jellyfin.Playlists["p1"].Items, Is.EqualTo(new[] { "id-a" }));
        }
    }
}
