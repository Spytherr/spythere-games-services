using Microsoft.EntityFrameworkCore;

namespace SpythereGamesServices;

public class LeaderboardService(SpythereGamesServicesContext context) : ILeaderboardService
{
    public async Task<List<LeaderboardEntryResponse>?> GetTopScoresAsync(string gameKey, int count = 10)
    {
        var game = await context.Games.FirstOrDefaultAsync(g => g.Key == gameKey);
        if (game is null) return null;

        var topScores = await context.Scores
            .Where(s => s.GameId == game.Id)
            .OrderByDescending(s => s.Value)
            .Take(count)
            .Select((s, index) => new LeaderboardEntryResponse(
                index + 1,
                context.Players.First(p => p.Id == s.PlayerId).DisplayName,
                s.Value,
                context.Players.First(p => p.Id == s.PlayerId).Platform
            ))
            .ToListAsync();

        return topScores;
    }

    public async Task<string?> SubmitScoreAsync(string gameKey, string externalId, long scoreValue)
    {
        var game = await context.Games.FirstOrDefaultAsync(g => g.Key == gameKey);
        if (game is null) return "Game not found";

        var player = await context.Players.FirstOrDefaultAsync(p => p.ExternalId == externalId);
        if (player is null) return "Player not found. Register first.";

        var newScore = new Score
        {
            PlayerId = player.Id,
            GameId = game.Id,
            Value = scoreValue,
            SubmittedAt = DateTime.UtcNow
        };

        context.Scores.Add(newScore);
        await context.SaveChangesAsync();
        
        return null;
    }

    public async Task<LeaderboardEntryResponse?> GetPlayerBestScoreAsync(string gameKey, string externalId)
    {
        var game = await context.Games.FirstOrDefaultAsync(g => g.Key == gameKey);
        if (game is null) return null;

        var player = await context.Players.FirstOrDefaultAsync(p => p.ExternalId == externalId);
        if (player is null) return null;

        var bestScore = await context.Scores
            .Where(s => s.GameId == game.Id && s.PlayerId == player.Id)
            .OrderByDescending(s => s.Value)
            .FirstOrDefaultAsync();

        if (bestScore is null) return null;

        return new LeaderboardEntryResponse(0, player.DisplayName, bestScore.Value, player.Platform);
    }
}
