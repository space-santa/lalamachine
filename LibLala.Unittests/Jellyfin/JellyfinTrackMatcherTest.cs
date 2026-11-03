using System.Linq;
using System.Text;
using LibLala.Jellyfin;
using NUnit.Framework;

namespace LibLala.Unittests.Jellyfin
{
    public class JellyfinTrackMatcherTest
    {
        private static JellyfinTrackMatcher Matcher(string libraryRoot, params string[] jellyfinPaths)
        {
            return new JellyfinTrackMatcher(libraryRoot, jellyfinPaths.Select((path, index) => new JellyfinAudioItem($"id{index}", path)));
        }

        [TestCase(@"D:\Music", @"D:\Music\Artist\Album\01 Track.mp3", "/media/music/Artist/Album/01 Track.mp3")]
        [TestCase(@"D:\Music\", @"D:\Music\Artist\Album\01 Track.mp3", "/media/music/Artist/Album/01 Track.mp3")]
        [TestCase(@"D:\", @"D:\Artist\Album\01 Track.mp3", "/media/music/Artist/Album/01 Track.mp3")]
        [TestCase(@"\\nas\music", @"\\nas\music\Artist\01 Track.mp3", "/media/music/Artist/01 Track.mp3")]
        [TestCase("/Users/bob/Music", "/Users/bob/Music/Artist/01 Track.mp3", "/media/music/Artist/01 Track.mp3")]
        [TestCase(@"D:\Music", @"D:\Music\Artist\01 Track.mp3", @"E:\Media\Artist\01 Track.mp3")]
        public void FindsTheTrackUnderAnotherRoot(string libraryRoot, string trackPath, string jellyfinPath)
        {
            var match = Matcher(libraryRoot, "/media/music/Other/Other.mp3", jellyfinPath).Match(trackPath);

            Assert.That(match.Status, Is.EqualTo(JellyfinMatchStatus.Found));
            Assert.That(match.Candidates.Single().Id, Is.EqualTo("id1"));
        }

        [Test]
        public void TellsWhichJellyfinFolderItFoundTheTrackIn()
        {
            var match = Matcher(@"D:\Music", @"E:\Media\Artist\01.mp3").Match(@"D:\Music\Artist\01.mp3");

            Assert.That(match.JellyfinRoot, Is.EqualTo("E:/Media/"));
            Assert.That(match.RelativePath, Is.EqualTo("Artist/01.mp3"));
        }

        [Test]
        public void CaseDoesntCount()
        {
            var match = Matcher(@"d:\MUSIC", "/media/music/artist/ALBUM/01 track.MP3").Match(@"D:\Music\Artist\Album\01 Track.mp3");

            Assert.That(match.Status, Is.EqualTo(JellyfinMatchStatus.Found));
        }

        [Test]
        public void HowAnAccentIsStoredDoesntCount()
        {
            var composed = "Café".Normalize(NormalizationForm.FormC);
            var decomposed = "Café".Normalize(NormalizationForm.FormD);
            // Without ICU, e.g. in .NET's invariant mode, there is no normalizing. Real installs have it on every platform.
            Assume.That(decomposed, Is.Not.EqualTo(composed));

            var match = Matcher("/Users/bob/Music", $"/media/music/{composed}/01.mp3").Match($"/Users/bob/Music/{decomposed}/01.mp3");

            Assert.That(match.Status, Is.EqualTo(JellyfinMatchStatus.Found));
        }

        [Test]
        public void TheWholeFolderNameHasToMatch()
        {
            var match = Matcher(@"D:\Music", "/media/music/The Artist/01.mp3").Match(@"D:\Music\Artist\01.mp3");

            Assert.That(match.Status, Is.EqualTo(JellyfinMatchStatus.NotFound));
        }

        [Test]
        public void SameFileNameInAnotherFolderIsNotFoundButShown()
        {
            var match = Matcher(@"D:\Music", "/media/music/Compilation/01.mp3", "/media/music/Unrelated/02.mp3").Match(@"D:\Music\Artist\01.mp3");

            Assert.That(match.Status, Is.EqualTo(JellyfinMatchStatus.NotFound));
            Assert.That(match.RelativePath, Is.EqualTo("Artist/01.mp3"));
            Assert.That(match.Candidates.Select(x => x.Path), Is.EqualTo(new[] { "/media/music/Compilation/01.mp3" }));
        }

        [Test]
        public void TwoTracksWithThePathIsAmbiguousNotAGuess()
        {
            var match = Matcher(@"D:\Music", "/media/music/Artist/01.mp3", "/backup/music/Artist/01.mp3").Match(@"D:\Music\Artist\01.mp3");

            Assert.That(match.Status, Is.EqualTo(JellyfinMatchStatus.Ambiguous));
            Assert.That(match.Candidates, Has.Count.EqualTo(2));
        }

        [TestCase(@"E:\Old\Artist\01.mp3")]
        [TestCase(@"D:\Music2\Artist\01.mp3")]
        public void TrackBelowAnotherFolderIsOutsideTheLibrary(string trackPath)
        {
            var match = Matcher(@"D:\Music", "/media/music/Artist/01.mp3").Match(trackPath);

            Assert.That(match.Status, Is.EqualTo(JellyfinMatchStatus.OutsideLibrary));
            Assert.That(match.TrackPath, Is.EqualTo(trackPath));
        }

        [Test]
        public void EmptyJellyfinFindsNothing()
        {
            var match = Matcher(@"D:\Music").Match(@"D:\Music\Artist\01.mp3");

            Assert.That(match.Status, Is.EqualTo(JellyfinMatchStatus.NotFound));
            Assert.That(match.Candidates, Is.Empty);
        }

        [Test]
        public void AMatchOutsideTheFolderOfTheOthersIsNotFoundButShown()
        {
            var matches = Matcher(@"D:\Music", "/media/music/a.mp3", "/media/music/b.mp3", "/media/music/Old/BPM/x.mp3")
                .MatchAll([@"D:\Music\a.mp3", @"D:\Music\BPM\x.mp3", @"D:\Music\b.mp3"]);

            Assert.That(matches.Select(x => x.Status), Is.EqualTo(new[] { JellyfinMatchStatus.Found, JellyfinMatchStatus.NotFound, JellyfinMatchStatus.Found }));
            Assert.That(matches[1].RelativePath, Is.EqualTo("BPM/x.mp3"));
            Assert.That(matches[1].Candidates.Select(x => x.Path), Is.EqualTo(new[] { "/media/music/Old/BPM/x.mp3" }));
            Assert.That(matches[1].JellyfinRoot, Is.Empty);
        }

        [Test]
        public void TheFolderOfTheOthersDecidesBetweenSeveralMatches()
        {
            var matches = Matcher(@"D:\Music", "/media/music/a.mp3", "/media/music/Old/x.mp3", "/media/music/x.mp3")
                .MatchAll([@"D:\Music\a.mp3", @"D:\Music\x.mp3"]);

            Assert.That(matches[1].Status, Is.EqualTo(JellyfinMatchStatus.Found));
            Assert.That(matches[1].Candidates.Single().Id, Is.EqualTo("id2"));
            Assert.That(matches[1].JellyfinRoot, Is.EqualTo("/media/music/"));
        }

        [Test]
        public void WithAsManyInTwoFoldersTheShorterOneCounts()
        {
            // In this order as well, the first one doesn't decide.
            var matches = Matcher(@"D:\Music", "/media/music/Old/b.mp3", "/media/music/a.mp3")
                .MatchAll([@"D:\Music\b.mp3", @"D:\Music\a.mp3"]);

            Assert.That(matches.Select(x => x.Status), Is.EqualTo(new[] { JellyfinMatchStatus.NotFound, JellyfinMatchStatus.Found }));
        }

        [Test]
        public void CaseDoesntCountForTheFolderEither()
        {
            var matches = Matcher(@"D:\Music", "/media/music/a.mp3", "/MEDIA/music/b.mp3")
                .MatchAll([@"D:\Music\a.mp3", @"D:\Music\b.mp3"]);

            Assert.That(matches.Select(x => x.Status), Is.All.EqualTo(JellyfinMatchStatus.Found));
        }

        [Test]
        public void WithoutAFolderToGoBySeveralMatchesStayAmbiguous()
        {
            var matches = Matcher(@"D:\Music", "/media/music/a.mp3", "/backup/music/a.mp3")
                .MatchAll([@"D:\Music\a.mp3"]);

            Assert.That(matches.Single().Status, Is.EqualTo(JellyfinMatchStatus.Ambiguous));
        }

        [Test]
        public void ASingleTrackHasNothingToBeComparedWith()
        {
            var matches = Matcher(@"D:\Music", "/media/music/Old/x.mp3").MatchAll([@"D:\Music\x.mp3"]);

            Assert.That(matches.Single().Status, Is.EqualTo(JellyfinMatchStatus.Found));
        }

        [Test]
        public void TheFolderDoesntChangeWhatWasntFound()
        {
            var matches = Matcher(@"D:\Music", "/media/music/a.mp3", "/media/music/Other/gone.mp3")
                .MatchAll([@"D:\Music\a.mp3", @"D:\Music\Artist\gone.mp3", @"E:\Old\a.mp3"]);

            Assert.That(matches.Select(x => x.Status), Is.EqualTo(new[] { JellyfinMatchStatus.Found, JellyfinMatchStatus.NotFound, JellyfinMatchStatus.OutsideLibrary }));
            Assert.That(matches[1].Candidates.Select(x => x.Path), Is.EqualTo(new[] { "/media/music/Other/gone.mp3" }));
        }
    }
}
