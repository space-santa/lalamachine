using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LibLala.Jellyfin;
using NUnit.Framework;

namespace LibLala.Unittests.Jellyfin
{
    internal sealed class FakeHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _respond;

        public FakeHandler(Func<HttpResponseMessage> respond) : this(_ => respond())
        {
        }

        public FakeHandler(Func<HttpRequestMessage, HttpResponseMessage> respond)
        {
            _respond = respond;
        }

        public Uri? RequestUri { get; private set; }
        public HttpMethod? RequestMethod { get; private set; }
        public string? RequestAuthorization { get; private set; }
        public int RequestCount { get; private set; }
        public string? RequestBody { get; private set; }
        public long? RequestContentLength { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            RequestUri = request.RequestUri;
            RequestMethod = request.Method;
            RequestAuthorization = request.Headers.Authorization?.ToString();
            RequestContentLength = request.Content?.Headers.ContentLength;
            RequestBody = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            RequestCount++;
            return _respond(request);
        }
    }

    public class JellyfinClientTest
    {
        private const string UsersJson = """[{"Name":"Bob","Id":"id-bob","HasPassword":true},{"Name":"Joe","Id":"id-joe"}]""";

        private static HttpResponseMessage Json(string json, HttpStatusCode status = HttpStatusCode.OK)
        {
            return new HttpResponseMessage(status) { Content = new StringContent(json) };
        }

        private static async Task<JellyfinException?> GetUsersFailure(Func<HttpResponseMessage> respond, string apiKey = "abc123")
        {
            using var handler = new FakeHandler(respond);
            using var httpClient = new HttpClient(handler);
            var client = new JellyfinClient("http://server:8096", apiKey, httpClient);

            return await Assert.ThrowsAsync<JellyfinException>(async () => await client.GetUsersAsync().ConfigureAwait(false)).ConfigureAwait(false);
        }

        [Test]
        public async Task GetsTheUsersOfTheServer()
        {
            using var handler = new FakeHandler(() => Json(UsersJson));
            using var httpClient = new HttpClient(handler);
            var client = new JellyfinClient("http://server:8096", "abc123", httpClient);

            var users = await client.GetUsersAsync().ConfigureAwait(false);

            Assert.That(users, Is.EqualTo(new[] { new JellyfinUser("id-bob", "Bob"), new JellyfinUser("id-joe", "Joe") }));
            Assert.That(handler.RequestMethod, Is.EqualTo(HttpMethod.Get));
        }

        [Test]
        public async Task SendsTheApiKeyTheWayJellyfinWantsIt()
        {
            using var handler = new FakeHandler(() => Json(UsersJson));
            using var httpClient = new HttpClient(handler);
            var client = new JellyfinClient("http://server:8096", "  abc123\n", httpClient);

            await client.GetUsersAsync().ConfigureAwait(false);

            Assert.That(handler.RequestAuthorization, Is.EqualTo("MediaBrowser Token=\"abc123\""));
        }

        [TestCase("http://server:8096", "http://server:8096/Users")]
        [TestCase("http://server:8096/", "http://server:8096/Users")]
        [TestCase("  http://server:8096  ", "http://server:8096/Users")]
        [TestCase("https://server/jellyfin", "https://server/jellyfin/Users")]
        [TestCase("https://server/jellyfin/", "https://server/jellyfin/Users")]
        [TestCase("http://server:8096/?a=b#/home.html", "http://server:8096/Users")]
        public async Task BuildsTheRequestFromTheAddress(string address, string expected)
        {
            using var handler = new FakeHandler(() => Json(UsersJson));
            using var httpClient = new HttpClient(handler);
            var client = new JellyfinClient(address, "abc123", httpClient);

            await client.GetUsersAsync().ConfigureAwait(false);

            Assert.That(handler.RequestUri, Is.EqualTo(new Uri(expected)));
        }

        [TestCase("")]
        [TestCase("   ")]
        [TestCase("192.168.1.10:8096")]
        [TestCase("jellyfin.local:8096")]
        [TestCase("server")]
        [TestCase("ftp://server")]
        [TestCase("/media/music")]
        public void AddressHasToBeWhole(string address)
        {
            var e = Assert.Throws<JellyfinException>(() => _ = new JellyfinClient(address, "abc123"));

            Assert.That(e?.Message, Does.Contain("whole address"));
        }

        [TestCase("")]
        [TestCase("   ")]
        [TestCase("abc\"123")]
        [TestCase("abc\\123")]
        [TestCase("abc\n123")]
        [TestCase("äbc123")]
        public void ApiKeyHasToFitTheHeader(string apiKey)
        {
            var e = Assert.Throws<JellyfinException>(() => _ = new JellyfinClient("http://server:8096", apiKey));

            Assert.That(e?.Message, Does.Contain("API key"));
        }

        [TestCase(HttpStatusCode.Unauthorized)]
        [TestCase(HttpStatusCode.Forbidden)]
        public async Task RefusedKeyIsReportedWithoutTheKey(HttpStatusCode status)
        {
            var e = await GetUsersFailure(() => Json("", status), "abc123").ConfigureAwait(false);

            Assert.That(e?.Message, Does.Contain("doesn't accept this API key"));
            Assert.That(e?.Message, Does.Not.Contain("abc123"));
        }

        [TestCase(HttpStatusCode.NotFound, "404")]
        [TestCase(HttpStatusCode.InternalServerError, "500")]
        [TestCase(HttpStatusCode.MovedPermanently, "301")]
        public async Task OtherErrorsAreReportedWithTheStatus(HttpStatusCode status, string expected)
        {
            var e = await GetUsersFailure(() => Json("", status)).ConfigureAwait(false);

            Assert.That(e?.Message, Does.Contain(expected));
        }

        [Test]
        public async Task AServerErrorIsntBlamedOnTheAddress()
        {
            var e = await GetUsersFailure(() => Json("", HttpStatusCode.InternalServerError)).ConfigureAwait(false);

            Assert.That(e?.Message, Is.EqualTo("The server answered with error 500."));
        }

        [TestCase("<html>Router login</html>")]
        [TestCase("{\"Name\":\"Bob\"}")]
        [TestCase("null")]
        [TestCase("[]")]
        [TestCase("[{\"Name\":\"Bob\"}]")]
        [TestCase("[{\"Id\":5}]")]
        public async Task AnswerThatIsntAListOfUsersIsNotJellyfin(string body)
        {
            var e = await GetUsersFailure(() => Json(body)).ConfigureAwait(false);

            Assert.That(e?.Message, Does.Contain("doesn't look like a Jellyfin server"));
        }

        [Test]
        public async Task UnreachableServerIsReported()
        {
            var e = await GetUsersFailure(() => throw new HttpRequestException("No such host is known.")).ConfigureAwait(false);

            Assert.That(e?.Message, Is.EqualTo("Can't reach the server: No such host is known."));
        }

        [Test]
        public async Task CertificateProblemIsReportedWithItsReason()
        {
            var e = await GetUsersFailure(() => throw new HttpRequestException(
                HttpRequestError.SecureConnectionError,
                "The SSL connection could not be established, see inner exception.",
                new AuthenticationException("The remote certificate is invalid."))).ConfigureAwait(false);

            Assert.That(e?.Message, Is.EqualTo("Can't connect securely to the server: The remote certificate is invalid."));
        }

        [Test]
        public async Task TimeoutIsReported()
        {
            // How HttpClient reports its own timeout, without the caller having cancelled anything.
            var e = await GetUsersFailure(() => throw new TaskCanceledException()).ConfigureAwait(false);

            Assert.That(e?.Message, Is.EqualTo("The server didn't answer in time."));
        }

        [Test]
        public void CancellingIsNotReportedAsAFailure()
        {
            using var handler = new FakeHandler(() => Json(UsersJson));
            using var httpClient = new HttpClient(handler);
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            var client = new JellyfinClient("http://server:8096", "abc123", httpClient);

            Assert.That(async () => await client.GetUsersAsync(cancellation.Token).ConfigureAwait(false), Throws.InstanceOf<OperationCanceledException>());
        }

        [Test]
        public async Task UsersWithoutANameOrIdAreLeftOut()
        {
            using var handler = new FakeHandler(() => Json("""[{"Name":"Bob","Id":"id-bob"},{"Name":"","Id":"id-x"},{"Id":"id-y"}]"""));
            using var httpClient = new HttpClient(handler);
            var client = new JellyfinClient("http://server:8096", "abc123", httpClient);

            var users = await client.GetUsersAsync().ConfigureAwait(false);

            Assert.That(users.Select(x => x.Name), Is.EqualTo(new[] { "Bob" }));
        }

        private static string ItemsPage(int total, params (string Id, string? Path)[] items)
        {
            var json = items.Select(x => JsonSerializer.Serialize(new { Name = "A title", x.Id, x.Path, Type = "Audio" }));
            return $$"""{"Items":[{{string.Join(",", json)}}],"TotalRecordCount":{{total}},"StartIndex":0}""";
        }

        private static (string Id, string? Path)[] Tracks(int first, int count)
        {
            return Enumerable.Range(first, count).Select(i => ($"id{i}", (string?)$"/media/{i}.mp3")).ToArray();
        }

        private static async Task<JellyfinAudioLibrary> GetLibrary(FakeHandler handler)
        {
            using var httpClient = new HttpClient(handler);
            var client = new JellyfinClient("http://server:8096/jellyfin", "abc123", httpClient);
            return await client.GetAudioLibraryAsync("user id").ConfigureAwait(false);
        }

        [Test]
        public async Task AsksForTheTracksOfTheUserWithTheirPaths()
        {
            using var handler = new FakeHandler(() => Json(ItemsPage(1, ("id1", "/media/1.mp3"))));

            var library = await GetLibrary(handler).ConfigureAwait(false);

            Assert.That(library.Items, Is.EqualTo(new[] { new JellyfinAudioItem("id1", "/media/1.mp3") }));
            Assert.That(library.ReportedTotal, Is.EqualTo(1));
            Assert.That(handler.RequestMethod, Is.EqualTo(HttpMethod.Get));
            Assert.That(handler.RequestAuthorization, Is.EqualTo("MediaBrowser Token=\"abc123\""));
            Assert.That(handler.RequestUri?.AbsolutePath, Is.EqualTo("/jellyfin/Items"));
            var query = handler.RequestUri?.Query;
            Assert.That(query, Does.Contain("userId=user%20id"));
            Assert.That(query, Does.Contain("recursive=true"));
            Assert.That(query, Does.Contain("includeItemTypes=Audio"));
            Assert.That(query, Does.Contain("fields=Path"));
            Assert.That(query, Does.Contain("startIndex=0"));
        }

        [Test]
        public async Task ReadsAllThePages()
        {
            var startIndexes = new List<string>();
            using var handler = new FakeHandler(request =>
            {
                var query = request.RequestUri!.Query;
                startIndexes.Add(query[(query.IndexOf("startIndex=", StringComparison.Ordinal) + "startIndex=".Length)..].Split('&')[0]);
                return startIndexes.Count == 1 ? Json(ItemsPage(503, Tracks(0, 500))) : Json(ItemsPage(503, Tracks(500, 3)));
            });

            var library = await GetLibrary(handler).ConfigureAwait(false);

            Assert.That(library.Items, Has.Count.EqualTo(503));
            Assert.That(library.ReportedTotal, Is.EqualTo(503));
            Assert.That(startIndexes, Is.EqualTo(new[] { "0", "500" }));
        }

        [Test]
        public async Task StopsWhenAPageIsEmptyEvenIfMoreWereReported()
        {
            var pages = 0;
            using var handler = new FakeHandler(() => Json(++pages == 1 ? ItemsPage(9, Tracks(0, 2)) : ItemsPage(9)));

            var library = await GetLibrary(handler).ConfigureAwait(false);

            Assert.That(handler.RequestCount, Is.EqualTo(2));
            Assert.That(library.Items, Has.Count.EqualTo(2));
            Assert.That(library.ReportedTotal, Is.EqualTo(9));
        }

        [Test]
        public async Task ATrackInTwoPagesCountsOnce()
        {
            var pages = 0;
            using var handler = new FakeHandler(() => Json(++pages == 1 ? ItemsPage(3, Tracks(0, 2)) : ItemsPage(3, Tracks(1, 1))));

            var library = await GetLibrary(handler).ConfigureAwait(false);

            Assert.That(library.Items.Select(x => x.Id), Is.EqualTo(new[] { "id0", "id1" }));
            Assert.That(library.ReportedTotal, Is.EqualTo(3));
        }

        [Test]
        public async Task TracksWithoutAPathAreLeftOutButStillCountAsReported()
        {
            using var handler = new FakeHandler(() => Json(ItemsPage(2, ("id1", "/media/1.mp3"), ("id2", null))));

            var library = await GetLibrary(handler).ConfigureAwait(false);

            Assert.That(library.Items.Select(x => x.Id), Is.EqualTo(new[] { "id1" }));
            Assert.That(library.ReportedTotal, Is.EqualTo(2));
        }

        [Test]
        public async Task NoPathsAtAllIsAFailure()
        {
            using var handler = new FakeHandler(() => Json(ItemsPage(2, ("id1", null), ("id2", null))));

            var e = await Assert.ThrowsAsync<JellyfinException>(async () => await GetLibrary(handler).ConfigureAwait(false)).ConfigureAwait(false);

            Assert.That(e?.Message, Does.Contain("doesn't say where the files are"));
        }

        [Test]
        public async Task EmptyLibraryIsNotAFailure()
        {
            using var handler = new FakeHandler(() => Json(ItemsPage(0)));

            var library = await GetLibrary(handler).ConfigureAwait(false);

            Assert.That(library.Items, Is.Empty);
            Assert.That(handler.RequestCount, Is.EqualTo(1));
        }

        [TestCase("<html>Router login</html>")]
        [TestCase("null")]
        [TestCase("[]")]
        [TestCase("{\"TotalRecordCount\":3}")]
        public async Task AnswerThatIsntAListOfItemsIsNotJellyfin(string body)
        {
            using var handler = new FakeHandler(() => Json(body));

            var e = await Assert.ThrowsAsync<JellyfinException>(async () => await GetLibrary(handler).ConfigureAwait(false)).ConfigureAwait(false);

            Assert.That(e?.Message, Does.Contain("doesn't look like a Jellyfin server"));
        }
    }
}
