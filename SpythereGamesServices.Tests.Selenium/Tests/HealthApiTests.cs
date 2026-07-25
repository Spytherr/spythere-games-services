using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using NUnit.Framework;

namespace SpythereGamesServices.Tests.Selenium.Tests;
[TestFixture]
[Category("RequiresBackend")]
public class HealthApiTests
{
    private HttpClient _httpClient = null!;
    private string _apiBaseUrl = null!;

    [SetUp]
    public void SetUp()
    {
        _apiBaseUrl = Environment.GetEnvironmentVariable("API_URL")
                      ?? "http://localhost:5028";

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_apiBaseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient.Dispose();
    }

    [Test]
    public async Task HealthEndpoint_Get_Returns200WhenHealthy()
    {
        var response = await _httpClient.GetAsync("/api/health");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            $"GET /api/health should return 200 OK. " +
            $"Current status: {(int)response.StatusCode} {response.StatusCode}. " +
            $"Check whether the backend and database are running.");
    }

    [Test]
    public async Task HealthEndpoint_Get_ResponseBodyContainsHealthyStatus()
    {
        var response = await _httpClient.GetAsync("/api/health");
        var body = await response.Content.ReadAsStringAsync();

        using var jsonDoc = JsonDocument.Parse(body);
        var root = jsonDoc.RootElement;

        Assert.That(root.TryGetProperty("Status", out var statusProperty), Is.True,
            $"The response body should contain the 'Status' property. Body: {body}");

        Assert.That(statusProperty.GetString(), Is.EqualTo("Healthy"),
            $"The 'Status' property should have the value 'Healthy'. Body: {body}");
    }

    [Test]
    public async Task HealthEndpoint_Head_Returns200WithNoBody()
    {
        var request = new HttpRequestMessage(HttpMethod.Head, "/api/health");

        var response = await _httpClient.SendAsync(request);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
                "HEAD /api/health should return 200 OK");

            var contentLength = response.Content.Headers.ContentLength;
            Assert.That(contentLength is null or 0, Is.True,
                "The HEAD response should not contain a body");
        });
    }

    [Test]
    public async Task GamesEndpoint_Get_Returns200WithGamesList()
    {
        var response = await _httpClient.GetAsync("/api/games");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            $"GET /api/games should return 200 OK. Status: {response.StatusCode}");

        var body = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(body);

        Assert.That(jsonDoc.RootElement.ValueKind, Is.EqualTo(JsonValueKind.Array),
            $"GET /api/games should return a JSON array. Body: {body}");
    }

    [Test]
    public async Task GamesEndpoint_EachGame_HasRequiredFields()
    {
        var response = await _httpClient.GetAsync("/api/games");
        var body = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(body);
        var games = jsonDoc.RootElement.EnumerateArray().ToList();

        if (games.Count == 0)
        {
            Assert.Ignore("No games found in the database — cannot validate the response structure");
            return;
        }

        foreach (var game in games)
        {
            var gameJson = game.ToString();

            Assert.Multiple(() =>
            {
                Assert.That(game.TryGetProperty("Id", out _), Is.True,
                    $"Each game should have the 'Id' property. Game JSON: {gameJson}");

                Assert.That(game.TryGetProperty("Key", out _), Is.True,
                    $"Each game should have the 'Key' property. Game JSON: {gameJson}");

                Assert.That(game.TryGetProperty("Name", out _), Is.True,
                    $"Each game should have the 'Name' property. Game JSON: {gameJson}");
            });
        }
    }

    [Test]
    public async Task GamesEndpoint_WithInvalidKey_Returns404()
    {
        const string invalidGameKey = "this-game-definitely-does-not-exist-xyz123";

        var response = await _httpClient.GetAsync($"/api/games/{invalidGameKey}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound),
            $"GET /api/games/{invalidGameKey} should return 404 Not Found. " +
            $"Current status: {(int)response.StatusCode}");
    }

    [Test]
    public async Task ScoresEndpoint_PostWithoutApiKey_Returns401()
    {
        const string gameKey = "test-game";
        var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"/api/games/{gameKey}/scores", content);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized),
            "POST without X-Api-Key should return 401 Unauthorized. " +
            "Check whether ApiKeyMiddleware is active.");
    }

    [Test]
    public async Task PlayersEndpoint_PostWithoutApiKey_Returns401()
    {
        var content = new StringContent(
            "{\"AuthCode\":\"fake\",\"Platform\":\"PC\"}",
            System.Text.Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync("/api/players", content);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized),
            "POST /api/players without X-Api-Key should return 401 Unauthorized");
    }

    [Test]
    public async Task HealthEndpoint_Response_HasJsonContentType()
    {
        var response = await _httpClient.GetAsync("/api/health");

        var contentType = response.Content.Headers.ContentType?.MediaType;
        Assert.That(contentType, Is.EqualTo("application/json"),
            $"Endpoint /api/health should return Content-Type: application/json. " +
            $"Current: {contentType}");
    }

    [Test]
    public async Task ScoresEndpoint_GetTopScores_Returns200OrNotFound()
    {
        var gamesResponse = await _httpClient.GetAsync("/api/games");

        if (gamesResponse.StatusCode != HttpStatusCode.OK)
        {
            Assert.Ignore("Unable to retrieve the list of games — test skipped");
            return;
        }

        var gamesBody = await gamesResponse.Content.ReadAsStringAsync();
        using var gamesDoc = JsonDocument.Parse(gamesBody);
        var games = gamesDoc.RootElement.EnumerateArray().ToList();

        if (games.Count == 0)
        {
            Assert.Ignore("No games found in the database — test skipped");
            return;
        }

        games[0].TryGetProperty("Key", out var keyProperty);
        var gameKey = keyProperty.GetString();

        Assert.That(gameKey, Is.Not.Null.And.Not.Empty,
            "The first game should have a non-empty 'Key' property");

        var scoresResponse = await _httpClient.GetAsync($"/api/games/{gameKey}/scores/top");

        Assert.That(
            scoresResponse.StatusCode == HttpStatusCode.OK ||
            scoresResponse.StatusCode == HttpStatusCode.NotFound,
            Is.True,
            $"GET /api/games/{gameKey}/scores/top should return 200 or 404, " +
            $"not {(int)scoresResponse.StatusCode} {scoresResponse.StatusCode}");
    }
}
