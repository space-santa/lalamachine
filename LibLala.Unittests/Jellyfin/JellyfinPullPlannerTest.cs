using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibLala.Jellyfin;
using NUnit.Framework;

namespace LibLala.Unittests.Jellyfin
{
    public class JellyfinPullPlannerTest
    {
        private const string LibraryRoot = @"D:\Music";
        private const string PlaylistId = "p1";

        private FakeJellyfin _jellyfin = null!;
        private List<string> _library = null!;

        [SetUp]
        public void SetUp()
        {
            _jellyfin = new FakeJellyfin();
            _library = [];
            foreach (var name in new[] { "a", "b", "c", "d" })
            {
                _jellyfin.Tracks.Add(new JellyfinAudioItem($"id-{name}", $"/media/music/{name}.mp3"));
                _library.Add(Local(name));
            }
        }

        private static string Local(string name)
        {
            return $@"{LibraryRoot}\{name}.mp3";
        }

        private Task<JellyfinPullPlan> Pull(params string[] itemIds)
        {
            _jellyfin.Playlists[PlaylistId] = new FakeJellyfin.Playlist { Name = "Mix", Owner = FakeJellyfin.UserId, Items = itemIds.ToList() };
            return new JellyfinPullPlanner(_jellyfin, FakeJellyfin.UserId).PrepareAsync(new JellyfinPlaylist(PlaylistId, "Mix"), LibraryRoot, _library);
        }

        [Test]
        public async Task PreparingChangesNothing()
        {
            await Pull("id-b", "id-a").ConfigureAwait(false);

            Assert.That(_jellyfin.Writes, Is.Empty);
        }

        [Test]
        public async Task HasTheTracksHereInTheOrderOfThePlaylist()
        {
            var plan = await Pull("id-c", "id-a", "id-b").ConfigureAwait(false);

            Assert.That(plan.PlaylistName, Is.EqualTo("Mix"));
            Assert.That(plan.TrackPaths, Is.EqualTo(new[] { Local("c"), Local("a"), Local("b") }));
            Assert.That(plan.TrackCount, Is.EqualTo(3));
            Assert.That(plan.Unresolved, Is.Empty);
        }

        [Test]
        public async Task ATrackThatIsInItTwiceIsThereOnceWhereItComesFirst()
        {
            var plan = await Pull("id-b", "id-a", "id-b").ConfigureAwait(false);

            Assert.That(plan.TrackPaths, Is.EqualTo(new[] { Local("b"), Local("a") }));
            Assert.That(plan.TrackCount, Is.EqualTo(2));
        }

        [Test]
        public async Task ATrackTheLibraryHereDoesntHaveIsMissing()
        {
            _library.Remove(Local("b"));
            _library.Add($@"{LibraryRoot}\Old\b.mp3");

            var plan = await Pull("id-a", "id-b").ConfigureAwait(false);

            Assert.That(plan.TrackPaths, Is.EqualTo(new[] { Local("a") }));
            var missing = plan.Unresolved.Single();
            Assert.That(missing.Status, Is.EqualTo(JellyfinPullStatus.NotFound));
            Assert.That(missing.JellyfinPath, Is.EqualTo("/media/music/b.mp3"));
            Assert.That(missing.Others, Is.EqualTo(new[] { $@"{LibraryRoot}\Old\b.mp3" }));
        }

        [Test]
        public async Task ATrackInAnotherFolderOfJellyfinThanTheOthersIsMissing()
        {
            // Pushing BPM\x.mp3 wouldn't find it either, Jellyfin's tracks are in /media/music and not in /media/music/Old.
            _jellyfin.Tracks.Add(new JellyfinAudioItem("id-x", "/media/music/Old/BPM/x.mp3"));
            _library.Add($@"{LibraryRoot}\BPM\x.mp3");

            var plan = await Pull("id-x", "id-a").ConfigureAwait(false);

            Assert.That(plan.TrackPaths, Is.EqualTo(new[] { Local("a") }));
            Assert.That(plan.Unresolved.Single().Status, Is.EqualTo(JellyfinPullStatus.NotFound));
        }

        [Test]
        public async Task ATrackJellyfinHasTwiceIsMissing()
        {
            _jellyfin.Tracks.Add(new JellyfinAudioItem("id-a2", "/media/music/a.mp3"));

            var plan = await Pull("id-a2", "id-b").ConfigureAwait(false);

            Assert.That(plan.TrackPaths, Is.EqualTo(new[] { Local("b") }));
            var missing = plan.Unresolved.Single();
            Assert.That(missing.Status, Is.EqualTo(JellyfinPullStatus.AmbiguousInJellyfin));
            Assert.That(missing.Others, Is.EqualTo(new[] { "/media/music/a.mp3", "/media/music/a.mp3" }));
        }

        [Test]
        public async Task ATrackThatIsTwiceHereIsMissing()
        {
            _library.Add($@"{LibraryRoot}\A.mp3");

            var plan = await Pull("id-a", "id-b").ConfigureAwait(false);

            Assert.That(plan.TrackPaths, Is.EqualTo(new[] { Local("b") }));
            var missing = plan.Unresolved.Single();
            Assert.That(missing.Status, Is.EqualTo(JellyfinPullStatus.AmbiguousHere));
            Assert.That(missing.Others, Is.EquivalentTo(new[] { Local("a"), $@"{LibraryRoot}\A.mp3" }));
        }

        [Test]
        public async Task APathTheLibraryHasTwiceIsOneTrack()
        {
            _library.Add(Local("a"));

            var plan = await Pull("id-a").ConfigureAwait(false);

            Assert.That(plan.TrackPaths, Is.EqualTo(new[] { Local("a") }));
        }

        [Test]
        public async Task SomethingThatIsntOneOfJellyfinsAudioTracksIsMissing()
        {
            var plan = await Pull("video", "id-a").ConfigureAwait(false);

            Assert.That(plan.TrackPaths, Is.EqualTo(new[] { Local("a") }));
            var missing = plan.Unresolved.Single();
            Assert.That(missing.Status, Is.EqualTo(JellyfinPullStatus.Unknown));
            Assert.That(missing.Name, Is.EqualTo("Title of video"));
            Assert.That(missing.JellyfinPath, Is.Empty);
        }

        [Test]
        public async Task AnEmptyPlaylistDoesntReadTheLibrary()
        {
            var plan = await Pull().ConfigureAwait(false);

            Assert.That(plan.TrackCount, Is.EqualTo(0));
            Assert.That(_jellyfin.LibraryReads, Is.EqualTo(0));
        }

        [TestCase(6, 2)]
        [TestCase(3, 0)]
        public async Task SaysHowManyTracksTheServerDidntGive(int reportedTotal, int notRead)
        {
            _jellyfin.ReportedTotal = reportedTotal;

            var plan = await Pull("id-a").ConfigureAwait(false);

            Assert.That(plan.LibraryTracksNotRead, Is.EqualTo(notRead));
        }

        [Test]
        public async Task PullingAPushedPlaylistGivesTheSameTracks()
        {
            _jellyfin.Tracks.Add(new JellyfinAudioItem("id-x", "/media/music/Old/BPM/x.mp3"));
            _library.Add($@"{LibraryRoot}\BPM\x.mp3");
            var publisher = new JellyfinPlaylistPublisher(_jellyfin, FakeJellyfin.UserId);
            var pushPlan = await publisher.PrepareAsync("Mix", LibraryRoot, [Local("d"), $@"{LibraryRoot}\BPM\x.mp3", Local("b")]).ConfigureAwait(false);
            await publisher.PublishAsync(pushPlan).ConfigureAwait(false);
            var pushed = _jellyfin.Playlists.Single();

            var plan = await new JellyfinPullPlanner(_jellyfin, FakeJellyfin.UserId).PrepareAsync(new JellyfinPlaylist(pushed.Key, pushed.Value.Name), LibraryRoot, _library).ConfigureAwait(false);

            Assert.That(plan.TrackPaths, Is.EqualTo(new[] { Local("d"), Local("b") }));
        }
    }
}
