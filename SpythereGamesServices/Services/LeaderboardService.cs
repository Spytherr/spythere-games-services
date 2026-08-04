using Microsoft.EntityFrameworkCore;

namespace SpythereGamesServices;

public class LeaderboardService(SpythereGamesServicesContext context) : ILeaderboardService
{
    public async Task<List<LeaderboardEntryResponse>?> GetTopScoresAsync(string gameKey, int count = 10, CancellationToken ct = default)
    {
        var game = await context.Games.AsNoTracking().FirstOrDefaultAsync(g => g.Key == gameKey, ct);
        if (game is null) return null;

        var topScores = await context.Scores.AsNoTracking()
            .Where(s => s.GameId == game.Id)
            .Join(
                context.Players.AsNoTracking(),
                score => score.PlayerId,
                player => player.Id,
                (score, player) => new { score, player }
            )
            .OrderByDescending(x => x.score.Value)
            .Take(count)
            .Select(x => new
            {
                x.player.DisplayName,
                x.score.Value,
                x.player.Platform
            })
            .ToListAsync(ct);

        var result = topScores.Select((entry, index) => new LeaderboardEntryResponse(
            index + 1,
            entry.DisplayName,
            entry.Value,
            entry.Platform
        )).ToList();

        return result;
    }

    public async Task<string?> SubmitScoreAsync(string gameKey, string externalId, string displayName, long scoreValue, CancellationToken ct = default)
    {
        var game = await context.Games.FirstOrDefaultAsync(g => g.Key == gameKey, ct);
        if (game is null) return "Game not found";

        var player = await context.Players.FirstOrDefaultAsync(p => p.ExternalId == externalId, ct);
        if (player is null) return "Player not found. Register first.";

        if (player.DisplayName != displayName)
        {
            player.DisplayName = displayName;
            player.UpdatedAt = DateTime.UtcNow;
        }

        var existingScore = await context.Scores
            .FirstOrDefaultAsync(s => s.PlayerId == player.Id && s.GameId == game.Id, ct);

        if (existingScore is not null)
        {
            if (scoreValue > existingScore.Value)
            {
                existingScore.Value = scoreValue;
                existingScore.SubmittedAt = DateTime.UtcNow;
            }
        }
        else
        {
            var newScore = new Score
            {
                PlayerId = player.Id,
                GameId = game.Id,
                Value = scoreValue,
                SubmittedAt = DateTime.UtcNow
            };

            context.Scores.Add(newScore);
        }

        await context.SaveChangesAsync(ct);
        return null;
    }

    public async Task<LeaderboardEntryResponse?> GetPlayerBestScoreAsync(string gameKey, string externalId, CancellationToken ct = default)
    {
        var game = await context.Games.AsNoTracking().FirstOrDefaultAsync(g => g.Key == gameKey, ct);
        if (game is null) return null;

        var player = await context.Players.AsNoTracking().FirstOrDefaultAsync(p => p.ExternalId == externalId, ct);
        if (player is null) return null;

        var playerScore = await context.Scores.AsNoTracking()
            .FirstOrDefaultAsync(s => s.GameId == game.Id && s.PlayerId == player.Id, ct);

        if (playerScore is null) return null;

        var rank = await context.Scores
            .CountAsync(s => s.GameId == game.Id && s.Value > playerScore.Value, ct) + 1;

        return new LeaderboardEntryResponse(rank, player.DisplayName, playerScore.Value, player.Platform);
    }
}
