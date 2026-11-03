using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using LibLala.Jellyfin;
using NUnit.Framework;

namespace LibLala.Unittests.Jellyfin
{
    public class JellyfinClientPlaylistTest
    {
        private static HttpResponseMessage Json(string json, HttpStatusCode status = HttpStatusCode.OK)
        {
            return new HttpResponseMessage(status) { Content = new StringContent(json) };
        }

        private static JellyfinClient Client(HttpClient httpClient)
        {
            return new JellyfinClient("http://server:8096", "abc123", httpClient);
        }

        [Test]
        public async Task ListsThePlaylistsTheUserCanSee()
        {
            using var handler = new FakeHandler(() => Json("""{"Items":[{"Id":"p1","Name":"House","Type":"Playlist"},{"Id":"p2","Name":"Techno"}],"TotalRecordCount":2}"""));
            using var httpClient = new HttpClient(handler);

            var playlists = await Client(httpClient).GetPlaylistsAsync("user id").ConfigureAwait(false);

            Assert.That(playlists, Is.EqualTo(new[] { new JellyfinPlaylist("p1", "House"), new JellyfinPlaylist("p2", "Techno") }));
            Assert.That(handler.RequestMethod, Is.EqualTo(HttpMethod.Get));
            Assert.That(handler.RequestUri?.AbsolutePath, Is.EqualTo("/Items"));
            Assert.That(handler.RequestUri?.Query, Does.Contain("userId=user%20id"));
            Assert.That(handler.RequestUri?.Query, Does.Contain("includeItemTypes=Playlist"));
            Assert.That(handler.RequestUri?.Query, Does.Contain("recursive=true"));
        }

        [TestCase("<html></html>")]
        [TestCase("{\"TotalRecordCount\":0}")]
        public async Task PlaylistsAnswerThatIsntAListIsNotJellyfin(string body)
        {
            using var handler = new FakeHandler(() => Json(body));
            using var httpClient = new HttpClient(handler);

            var e = await Assert.ThrowsAsync<JellyfinException>(async () => await Client(httpClient).GetPlaylistsAsync("u").ConfigureAwait(false)).ConfigureAwait(false);

            Assert.That(e?.Message, Does.Contain("doesn't look like a Jellyfin server"));
        }

        [Test]
        public async Task ReadsTheEntriesOfAPlaylist()
        {
            using var handler = new FakeHandler(() => Json("""{"Items":[{"Id":"t1","PlaylistItemId":"e1"},{"Id":"t2"}],"TotalRecordCount":2}"""));
            using var httpClient = new HttpClient(handler);

            var entries = await Client(httpClient).GetPlaylistEntriesAsync("p 1", "u1").ConfigureAwait(false);

            // Without an entry ID it goes by the track, like current versions do.
            Assert.That(entries, Is.EqualTo(new[] { new JellyfinPlaylistEntry("t1", "e1"), new JellyfinPlaylistEntry("t2", "t2") }));
            Assert.That(handler.RequestUri?.AbsolutePath, Is.EqualTo("/Playlists/p%201/Items"));
            Assert.That(handler.RequestUri?.Query, Does.Contain("userId=u1"));
        }

        [Test]
        public async Task CreatesAPrivatePlaylistForTheUser()
        {
            using var handler = new FakeHandler(() => Json("""{"Id":"new-id"}"""));
            using var httpClient = new HttpClient(handler);

            var id = await Client(httpClient).CreatePlaylistAsync("2026 - House", "u1", ["t2", "t1"]).ConfigureAwait(false);

            Assert.That(id, Is.EqualTo("new-id"));
            Assert.That(handler.RequestMethod, Is.EqualTo(HttpMethod.Post));
            Assert.That(handler.RequestUri?.AbsolutePath, Is.EqualTo("/Playlists"));
            Assert.That(handler.RequestAuthorization, Is.EqualTo("MediaBrowser Token=\"abc123\""));
            // Known, so that it doesn't go out in chunks.
            Assert.That(handler.RequestContentLength, Is.GreaterThan(0));
            using var body = JsonDocument.Parse(handler.RequestBody!);
            // GetProperty looks at the case, these are the names in Jellyfin's API.
            var root = body.RootElement;
            Assert.That(root.GetProperty("Name").GetString(), Is.EqualTo("2026 - House"));
            Assert.That(root.GetProperty("UserId").GetString(), Is.EqualTo("u1"));
            Assert.That(root.GetProperty("MediaType").GetString(), Is.EqualTo("Audio"));
            Assert.That(root.GetProperty("IsPublic").GetBoolean(), Is.False);
            Assert.That(root.GetProperty("Ids").EnumerateArray().Select(x => x.GetString()), Is.EqualTo(new[] { "t2", "t1" }));
        }

        [TestCase("{}")]
        [TestCase("{\"Id\":\"\"}")]
        [TestCase("<html></html>")]
        public async Task CreateAnswerWithoutAnIdIsNotJellyfin(string body)
        {
            using var handler = new FakeHandler(() => Json(body));
            using var httpClient = new HttpClient(handler);

            var e = await Assert.ThrowsAsync<JellyfinException>(async () => await Client(httpClient).CreatePlaylistAsync("x", "u", ["t"]).ConfigureAwait(false)).ConfigureAwait(false);

            Assert.That(e?.Message, Does.Contain("doesn't look like a Jellyfin server"));
        }

        private static List<string> IdsIn(Uri uri, string parameter)
        {
            var start = uri.Query.IndexOf($"{parameter}=", StringComparison.Ordinal) + parameter.Length + 1;
            return uri.Query[start..].Split('&')[0].Split(',').ToList();
        }

        [Test]
        public async Task AddsInOrderInBatchesThatFitInAUrl()
        {
            var ids = Enumerable.Range(0, 250).Select(i => $"{i:x32}").ToList();
            var sent = new List<string>();
            using var handler = new FakeHandler(request =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(request.RequestUri!.AbsolutePath, Is.EqualTo("/Playlists/p1/Items"));
                Assert.That(request.RequestUri.Query, Does.Contain("userId=u1"));
                Assert.That(request.RequestUri.AbsoluteUri.Length, Is.LessThan(8000));
                sent.AddRange(IdsIn(request.RequestUri, "ids"));
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            });
            using var httpClient = new HttpClient(handler);

            await Client(httpClient).AddToPlaylistAsync("p1", ids, "u1").ConfigureAwait(false);

            Assert.That(handler.RequestCount, Is.EqualTo(3));
            Assert.That(sent, Is.EqualTo(ids));
        }

        [Test]
        public async Task RemovesInBatchesThatFitInAUrl()
        {
            var ids = Enumerable.Range(0, 250).Select(i => $"{i:x32}").ToList();
            var sent = new List<string>();
            using var handler = new FakeHandler(request =>
            {
                Assert.That(request.Method, Is.EqualTo(HttpMethod.Delete));
                Assert.That(request.RequestUri!.AbsolutePath, Is.EqualTo("/Playlists/p1/Items"));
                Assert.That(request.RequestUri.AbsoluteUri.Length, Is.LessThan(8000));
                sent.AddRange(IdsIn(request.RequestUri, "entryIds"));
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            });
            using var httpClient = new HttpClient(handler);

            await Client(httpClient).RemoveFromPlaylistAsync("p1", ids).ConfigureAwait(false);

            Assert.That(handler.RequestCount, Is.EqualTo(3));
            Assert.That(sent, Is.EqualTo(ids));
        }

        [Test]
        public async Task NothingToAddOrRemoveSendsNothing()
        {
            using var handler = new FakeHandler(() => new HttpResponseMessage(HttpStatusCode.NoContent));
            using var httpClient = new HttpClient(handler);

            await Client(httpClient).AddToPlaylistAsync("p1", [], "u1").ConfigureAwait(false);
            await Client(httpClient).RemoveFromPlaylistAsync("p1", []).ConfigureAwait(false);

            Assert.That(handler.RequestCount, Is.Zero);
        }

        [Test]
        public async Task ForbiddenChangeIsAboutTheUserNotTheKey()
        {
            using var handler = new FakeHandler(() => new HttpResponseMessage(HttpStatusCode.Forbidden));
            using var httpClient = new HttpClient(handler);

            var e = await Assert.ThrowsAsync<JellyfinException>(async () => await Client(httpClient).AddToPlaylistAsync("p1", ["t1"], "u1").ConfigureAwait(false)).ConfigureAwait(false);

            Assert.That(e?.Message, Does.Contain("doesn't allow this user to change"));
        }

        [Test]
        public async Task RefusedKeyOnAChangeIsStillAboutTheKey()
        {
            using var handler = new FakeHandler(() => new HttpResponseMessage(HttpStatusCode.Unauthorized));
            using var httpClient = new HttpClient(handler);

            var e = await Assert.ThrowsAsync<JellyfinException>(async () => await Client(httpClient).CreatePlaylistAsync("x", "u1", ["t1"]).ConfigureAwait(false)).ConfigureAwait(false);

            Assert.That(e?.Message, Is.EqualTo("The server doesn't accept this API key."));
        }
    }
}
