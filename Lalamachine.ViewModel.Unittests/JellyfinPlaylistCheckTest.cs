using System.Linq;
using LibLala.Jellyfin;
using NUnit.Framework;

namespace Lalamachine.ViewModel.Unittests
{
    public class JellyfinPlaylistCheckTest
    {
        private const string LibraryPath = @"D:\Music";

        private static JellyfinAudioLibrary Library(params string[] paths)
        {
            return new JellyfinAudioLibrary(paths.Select((path, index) => new JellyfinAudioItem($"id{index}", path)).ToList(), paths.Length);
        }

        private static string Local(string relativePath)
        {
            return $@"{LibraryPath}\{relativePath}";
        }

        private static string Report(JellyfinAudioLibrary library, params PlaylistPaths[] playlists)
        {
            return JellyfinPlaylistCheck.Report(LibraryPath, library, "Bob", playlists);
        }

        [Test]
        public void CountsThePerPlaylistAndAltogether()
        {
            var report = Report(
                Library("/media/music/a.mp3", "/media/music/b.mp3"),
                new PlaylistPaths("House", [Local("a.mp3"), Local("b.mp3")]),
                new PlaylistPaths("Techno", [Local("a.mp3"), Local("missing.mp3")]));

            Assert.That(report, Does.Contain("Jellyfin has 2 tracks for Bob."));
            Assert.That(report, Does.Contain("House: 2 of 2"));
            Assert.That(report, Does.Contain("Techno: 1 of 2"));
            // a.mp3 is in both, it counts once.
            Assert.That(report, Does.Contain("Altogether 2 of 3 different tracks found."));
        }

        [Test]
        public void SaysWhereInJellyfinTheTracksWereFound()
        {
            var report = Report(
                Library("/media/music/a.mp3", "/media/music/b.mp3", "/other/c.mp3"),
                new PlaylistPaths("House", [Local("a.mp3"), Local("b.mp3"), Local("c.mp3")]));

            Assert.That(report, Does.Contain("In /media/music/: 2"));
            Assert.That(report, Does.Not.Contain("In /other/"));
            Assert.That(report, Does.Contain("House: 2 of 3"));
            Assert.That(report, Does.Contain("same file name in Jellyfin: /other/c.mp3"));
        }

        [Test]
        public void TheFolderGoesByTheTracksOfAllThePlaylists()
        {
            var report = Report(
                Library("/media/music/a.mp3", "/media/music/b.mp3", "/media/music/Old/c.mp3"),
                new PlaylistPaths("House", [Local("a.mp3"), Local("b.mp3")]),
                new PlaylistPaths("Techno", [Local("c.mp3")]));

            Assert.That(report, Does.Contain("Techno: 0 of 1"));
            Assert.That(report, Does.Contain("Altogether 2 of 3 different tracks found."));
        }

        [Test]
        public void ListsWhatIsNotFoundWithTheFilesOfTheSameName()
        {
            var report = Report(
                Library("/media/music/Compilation/a.mp3"),
                new PlaylistPaths("House", [Local(@"Artist\a.mp3")]));

            Assert.That(report, Does.Contain("Not found (1):"));
            Assert.That(report, Does.Contain("Artist/a.mp3"));
            Assert.That(report, Does.Contain("same file name in Jellyfin: /media/music/Compilation/a.mp3"));
        }

        [Test]
        public void ListsWhatIsThereMoreThanOnceWithAllItsPlaces()
        {
            var report = Report(
                Library("/media/music/a.mp3", "/backup/music/a.mp3"),
                new PlaylistPaths("House", [Local("a.mp3")]));

            Assert.That(report, Does.Contain("More than once in Jellyfin (1):"));
            Assert.That(report, Does.Contain("/media/music/a.mp3"));
            Assert.That(report, Does.Contain("/backup/music/a.mp3"));
            Assert.That(report, Does.Contain("House: 0 of 1"));
        }

        [Test]
        public void ListsWhatIsOutsideTheLibraryFolder()
        {
            var report = Report(Library(), new PlaylistPaths("House", [@"E:\Old\a.mp3"]));

            Assert.That(report, Does.Contain($"Outside the library folder {LibraryPath} (1):"));
            Assert.That(report, Does.Contain(@"E:\Old\a.mp3"));
        }

        [Test]
        public void ALongListGetsCutShort()
        {
            var tracks = Enumerable.Range(1000, 75).Select(i => Local($"{i}.mp3")).ToList();

            var report = Report(Library(), new PlaylistPaths("House", tracks));

            Assert.That(report, Does.Contain("Not found (75):"));
            Assert.That(report, Does.Contain("1049.mp3"));
            Assert.That(report, Does.Not.Contain("1050.mp3"));
            Assert.That(report, Does.Contain("...and 25 more."));
            Assert.That(report, Does.Contain("House: 0 of 75"));
        }

        [Test]
        public void MentionsWhenTheServerReportedMoreThanItGave()
        {
            var report = Report(new JellyfinAudioLibrary([new JellyfinAudioItem("id", "/media/a.mp3")], 3), new PlaylistPaths("House", [Local("a.mp3")]));

            Assert.That(report, Does.Contain("It reports 3"));
        }

        [Test]
        public void DoesntMentionTheTotalWhenItAddsUp()
        {
            var report = Report(Library("/media/a.mp3"), new PlaylistPaths("House", [Local("a.mp3")]));

            Assert.That(report, Does.Not.Contain("It reports"));
        }

        [Test]
        public void LeavesOutEverythingThereIsNothingToSayAbout()
        {
            var report = Report(
                Library("/media/a.mp3"),
                new PlaylistPaths("House", [Local("a.mp3")]),
                new PlaylistPaths("Empty", []));

            Assert.That(report, Does.Not.Contain("Empty"));
            Assert.That(report, Does.Not.Contain("Not found"));
            Assert.That(report, Does.Not.Contain("More than once"));
            Assert.That(report, Does.Not.Contain("Outside"));
        }

        [Test]
        public void SaysSoWhenThePlaylistsHaveNoTracks()
        {
            var report = Report(Library("/media/a.mp3"), new PlaylistPaths("Empty", []));

            Assert.That(report, Does.Contain("The playlists have no tracks."));
            Assert.That(report, Does.Not.Contain("Altogether"));
        }

        [Test]
        public void SaysThatNothingChanged()
        {
            Assert.That(Report(Library()), Does.EndWith("Nothing was changed, neither in Jellyfin nor here."));
        }
    }
}
