using SpythereGamesServices;
using SpythereGamesServices.Tests.Helpers;

namespace SpythereGamesServices.Tests.Services;

public class PlayerServiceTests : IDisposable
{
    private readonly SpythereGamesServicesContext _context;
    private readonly PlayerService _service;

    public PlayerServiceTests()
    {
        _context = TestDbContextFactory.Create();
        _service = new PlayerService(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task RegisterPlayerAsync_CreatesNewPlayer()
    {
        var player = await _service.RegisterPlayerAsync("ext-1", "Alice", "android");

        Assert.NotEqual(0, player.Id);
        Assert.Equal("Alice", player.DisplayName);
        Assert.Equal("android", player.Platform);
        Assert.Equal("ext-1", player.ExternalId);
        Assert.True(player.CreatedAt <= DateTime.UtcNow);
        Assert.True(player.UpdatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public async Task RegisterPlayerAsync_SetsUtcTimestamps()
    {
        var before = DateTime.UtcNow;
        var player = await _service.RegisterPlayerAsync("ext-ts", "Bob", "ios");
        var after = DateTime.UtcNow;

        Assert.InRange(player.CreatedAt, before, after);
        Assert.InRange(player.UpdatedAt, before, after);
    }

    [Fact]
    public async Task GetPlayerAsync_ReturnsPlayer_WhenExists()
    {
        var created = await _service.RegisterPlayerAsync("ext-2", "Bob", "ios");

        var found = await _service.GetPlayerAsync(created.Id);

        Assert.NotNull(found);
        Assert.Equal(created.Id, found.Id);
        Assert.Equal("Bob", found.DisplayName);
    }

    [Fact]
    public async Task GetPlayerAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _service.GetPlayerAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetPlayerByExternalIdAsync_ReturnsPlayer_WhenExists()
    {
        await _service.RegisterPlayerAsync("ext-3", "Charlie", "web");

        var found = await _service.GetPlayerByExternalIdAsync("ext-3");

        Assert.NotNull(found);
        Assert.Equal("Charlie", found.DisplayName);
        Assert.Equal("ext-3", found.ExternalId);
    }

    [Fact]
    public async Task GetPlayerByExternalIdAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _service.GetPlayerByExternalIdAsync("nonexistent");

        Assert.Null(result);
    }

    [Fact]
    public async Task DeletePlayerAsync_RemovesPlayer_WhenExists()
    {
        var player = await _service.RegisterPlayerAsync("ext-4", "Dave", "android");

        await _service.DeletePlayerAsync(player.Id);

        var found = await _service.GetPlayerAsync(player.Id);
        Assert.Null(found);
    }

    [Fact]
    public async Task DeletePlayerAsync_DoesNotThrow_WhenPlayerNotFound()
    {
        await _service.DeletePlayerAsync(999);
    }

    [Fact]
    public async Task DeletePlayerAsync_CascadeDeletesScores()
    {
        var player = await _service.RegisterPlayerAsync("ext-5", "Eve", "ios");

        _context.Scores.Add(new Score
        {
            PlayerId = player.Id,
            GameId = 1,
            Value = 100,
            SubmittedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        await _service.DeletePlayerAsync(player.Id);

        Assert.Empty(_context.Scores.Where(s => s.PlayerId == player.Id));
    }

    [Fact]
    public async Task RegisterPlayerAsync_MultiplePlayers_HaveDistinctIds()
    {
        var p1 = await _service.RegisterPlayerAsync("ext-a", "A", "android");
        var p2 = await _service.RegisterPlayerAsync("ext-b", "B", "ios");

        Assert.NotEqual(p1.Id, p2.Id);
    }
}
