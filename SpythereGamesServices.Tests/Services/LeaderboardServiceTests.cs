using SpythereGamesServices;
using SpythereGamesServices.Tests.Helpers;

namespace SpythereGamesServices.Tests.Services;

public class LeaderboardServiceTests : IDisposable
{
    private readonly SpythereGamesServicesContext _context;
    private readonly LeaderboardService _service;

    public LeaderboardServiceTests()
    {
        _context = TestDbContextFactory.Create();
        _service = new LeaderboardService(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private async Task<Player> SeedPlayer(string externalId, string name, string platform = "android")
    {
        var player = new Player
        {
            ExternalId = externalId,
            DisplayName = name,
            Platform = platform,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Players.Add(player);
        await _context.SaveChangesAsync();
        return player;
    }

    private async Task<Game> SeedGame(string key, string name)
    {
        var game = new Game
        {
            Key = key,
            Name = name,
            Description = "Test game",
            IconUrl = "",
            CreatedAt = DateTime.UtcNow
        };
        _context.Games.Add(game);
        await _context.SaveChangesAsync();
        return game;
    }

    // --- SubmitScoreAsync ---

    [Fact]
    public async Task SubmitScoreAsync_ReturnsError_WhenGameNotFound()
    {
        await SeedPlayer("p1", "Alice");

        var result = await _service.SubmitScoreAsync("nonexistent-game", "p1", 100);

        Assert.Equal("Game not found", result);
    }

    [Fact]
    public async Task SubmitScoreAsync_ReturnsError_WhenPlayerNotFound()
    {
        await SeedGame("game1", "Game 1");

        var result = await _service.SubmitScoreAsync("game1", "unknown-player", 100);

        Assert.Equal("Player not found. Register first.", result);
    }

    [Fact]
    public async Task SubmitScoreAsync_CreatesNewScore_WhenFirstSubmission()
    {
        var game = await SeedGame("game1", "Game 1");
        var player = await SeedPlayer("p1", "Alice");

        var result = await _service.SubmitScoreAsync("game1", "p1", 500);

        Assert.Null(result);
        var score = _context.Scores.Single(s => s.PlayerId == player.Id && s.GameId == game.Id);
        Assert.Equal(500, score.Value);
    }

    [Fact]
    public async Task SubmitScoreAsync_UpdatesScore_WhenNewScoreIsHigher()
    {
        var game = await SeedGame("game1", "Game 1");
        var player = await SeedPlayer("p1", "Alice");

        await _service.SubmitScoreAsync("game1", "p1", 500);
        await _service.SubmitScoreAsync("game1", "p1", 800);

        var score = _context.Scores.Single(s => s.PlayerId == player.Id && s.GameId == game.Id);
        Assert.Equal(800, score.Value);
    }

    [Fact]
    public async Task SubmitScoreAsync_DoesNotUpdateScore_WhenNewScoreIsLower()
    {
        var game = await SeedGame("game1", "Game 1");
        var player = await SeedPlayer("p1", "Alice");

        await _service.SubmitScoreAsync("game1", "p1", 500);
        await _service.SubmitScoreAsync("game1", "p1", 200);

        var score = _context.Scores.Single(s => s.PlayerId == player.Id && s.GameId == game.Id);
        Assert.Equal(500, score.Value);
    }

    [Fact]
    public async Task SubmitScoreAsync_DoesNotUpdateScore_WhenNewScoreIsEqual()
    {
        var game = await SeedGame("game1", "Game 1");
        var player = await SeedPlayer("p1", "Alice");

        await _service.SubmitScoreAsync("game1", "p1", 500);
        await _service.SubmitScoreAsync("game1", "p1", 500);

        var score = _context.Scores.Single(s => s.PlayerId == player.Id && s.GameId == game.Id);
        Assert.Equal(500, score.Value);
    }

    [Fact]
    public async Task SubmitScoreAsync_ReturnsNull_OnSuccess()
    {
        await SeedGame("game1", "Game 1");
        await SeedPlayer("p1", "Alice");

        var result = await _service.SubmitScoreAsync("game1", "p1", 100);

        Assert.Null(result);
    }

    // --- GetTopScoresAsync ---

    [Fact]
    public async Task GetTopScoresAsync_ReturnsNull_WhenGameNotFound()
    {
        var result = await _service.GetTopScoresAsync("nonexistent");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetTopScoresAsync_ReturnsEmptyList_WhenNoScores()
    {
        await SeedGame("game1", "Game 1");

        var result = await _service.GetTopScoresAsync("game1");

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetTopScoresAsync_ReturnsScoresOrderedDescending()
    {
        await SeedGame("game1", "Game 1");
        var p1 = await SeedPlayer("p1", "Alice");
        var p2 = await SeedPlayer("p2", "Bob");
        var p3 = await SeedPlayer("p3", "Charlie");

        await _service.SubmitScoreAsync("game1", "p1", 300);
        await _service.SubmitScoreAsync("game1", "p2", 500);
        await _service.SubmitScoreAsync("game1", "p3", 100);

        var result = await _service.GetTopScoresAsync("game1");

        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Equal("Bob", result[0].DisplayName);
        Assert.Equal(500, result[0].ScoreValue);
        Assert.Equal("Alice", result[1].DisplayName);
        Assert.Equal("Charlie", result[2].DisplayName);
    }

    [Fact]
    public async Task GetTopScoresAsync_RespectsCountParameter()
    {
        await SeedGame("game1", "Game 1");
        await SeedPlayer("p1", "Alice");
        await SeedPlayer("p2", "Bob");
        await SeedPlayer("p3", "Charlie");

        await _service.SubmitScoreAsync("game1", "p1", 300);
        await _service.SubmitScoreAsync("game1", "p2", 500);
        await _service.SubmitScoreAsync("game1", "p3", 100);

        var result = await _service.GetTopScoresAsync("game1", 2);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetTopScoresAsync_AssignsCorrectRanks()
    {
        await SeedGame("game1", "Game 1");
        await SeedPlayer("p1", "Alice");
        await SeedPlayer("p2", "Bob");

        await _service.SubmitScoreAsync("game1", "p1", 300);
        await _service.SubmitScoreAsync("game1", "p2", 500);

        var result = await _service.GetTopScoresAsync("game1");

        Assert.NotNull(result);
        Assert.Equal(1, result[0].Rank);
        Assert.Equal(2, result[1].Rank);
    }

    [Fact]
    public async Task GetTopScoresAsync_IncludesPlatformInfo()
    {
        await SeedGame("game1", "Game 1");
        await SeedPlayer("p1", "Alice", "android");

        await _service.SubmitScoreAsync("game1", "p1", 300);

        var result = await _service.GetTopScoresAsync("game1");

        Assert.NotNull(result);
        Assert.Equal("android", result[0].Platform);
    }

    // --- GetPlayerBestScoreAsync ---

    [Fact]
    public async Task GetPlayerBestScoreAsync_ReturnsNull_WhenGameNotFound()
    {
        await SeedPlayer("p1", "Alice");

        var result = await _service.GetPlayerBestScoreAsync("nonexistent", "p1");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetPlayerBestScoreAsync_ReturnsNull_WhenPlayerNotFound()
    {
        await SeedGame("game1", "Game 1");

        var result = await _service.GetPlayerBestScoreAsync("game1", "unknown");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetPlayerBestScoreAsync_ReturnsNull_WhenNoScore()
    {
        await SeedGame("game1", "Game 1");
        await SeedPlayer("p1", "Alice");

        var result = await _service.GetPlayerBestScoreAsync("game1", "p1");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetPlayerBestScoreAsync_ReturnsCorrectScore()
    {
        await SeedGame("game1", "Game 1");
        await SeedPlayer("p1", "Alice", "ios");

        await _service.SubmitScoreAsync("game1", "p1", 750);

        var result = await _service.GetPlayerBestScoreAsync("game1", "p1");

        Assert.NotNull(result);
        Assert.Equal("Alice", result.DisplayName);
        Assert.Equal(750, result.ScoreValue);
        Assert.Equal("ios", result.Platform);
    }

    [Fact]
    public async Task GetPlayerBestScoreAsync_ComputesCorrectRank()
    {
        await SeedGame("game1", "Game 1");
        await SeedPlayer("p1", "Alice");
        await SeedPlayer("p2", "Bob");
        await SeedPlayer("p3", "Charlie");

        await _service.SubmitScoreAsync("game1", "p1", 300);
        await _service.SubmitScoreAsync("game1", "p2", 500);
        await _service.SubmitScoreAsync("game1", "p3", 100);

        var aliceResult = await _service.GetPlayerBestScoreAsync("game1", "p1");
        var bobResult = await _service.GetPlayerBestScoreAsync("game1", "p2");
        var charlieResult = await _service.GetPlayerBestScoreAsync("game1", "p3");

        Assert.NotNull(bobResult);
        Assert.Equal(1, bobResult.Rank);

        Assert.NotNull(aliceResult);
        Assert.Equal(2, aliceResult.Rank);

        Assert.NotNull(charlieResult);
        Assert.Equal(3, charlieResult.Rank);
    }

    [Fact]
    public async Task GetPlayerBestScoreAsync_RankIsScopedToGame()
    {
        var game1 = await SeedGame("game1", "Game 1");
        var game2 = await SeedGame("game2", "Game 2");
        await SeedPlayer("p1", "Alice");
        await SeedPlayer("p2", "Bob");

        await _service.SubmitScoreAsync("game1", "p1", 300);
        await _service.SubmitScoreAsync("game1", "p2", 500);

        await _service.SubmitScoreAsync("game2", "p1", 900);

        var aliceGame1 = await _service.GetPlayerBestScoreAsync("game1", "p1");
        var aliceGame2 = await _service.GetPlayerBestScoreAsync("game2", "p1");

        Assert.NotNull(aliceGame1);
        Assert.Equal(2, aliceGame1.Rank);

        Assert.NotNull(aliceGame2);
        Assert.Equal(1, aliceGame2.Rank);
    }
}
