using Microsoft.EntityFrameworkCore;

namespace SpythereGamesServices;

public class LeaderboardService(SpythereGamesServicesContext context, IPlayerService playerService) : ILeaderboardService
{
    public async Task<List<LeaderboardEntryResponse>?> GetTopScoresAsync(string gameKey, int count = 10)
    {
        var game = await context.FindGameByKeyAsync(gameKey);
        if (game is null) return null;

        // Ponieważ każdy gracz ma max 1 wynik per gra (upsert w SubmitScore),
        // wystarczy proste zapytanie z Join — bez GroupBy.
        var topScores = await context.Scores
            .Where(s => s.GameId == game.Id)
            .Join(
                context.Players,
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
            .ToListAsync();

        // Rank obliczamy po stronie C# (po materializacji) — EF Core nie obsługuje indexu w Select
        var result = topScores.Select((entry, index) => new LeaderboardEntryResponse(
            index + 1,
            entry.DisplayName,
            entry.Value,
            entry.Platform
        )).ToList();

        return result;
    }

    public async Task<string?> SubmitScoreAsync(string gameKey, string externalId, long scoreValue)
    {
        var game = await context.FindGameByKeyAsync(gameKey);
        if (game is null) return "Game not found";

        var player = await playerService.GetPlayerByExternalIdAsync(externalId);
        if (player is null) return "Player not found. Register first.";

        // Sprawdź czy gracz ma już wynik w tej grze
        var existingScore = await context.Scores
            .FirstOrDefaultAsync(s => s.PlayerId == player.Id && s.GameId == game.Id);

        if (existingScore is not null)
        {
            // Aktualizuj tylko jeśli nowy wynik jest lepszy
            if (scoreValue > existingScore.Value)
            {
                existingScore.Value = scoreValue;
                existingScore.SubmittedAt = DateTime.UtcNow;
            }
            // Jeśli gorszy lub równy — nic nie robimy (jak Google Play)
        }
        else
        {
            // Pierwszy wynik tego gracza w tej grze
            var newScore = new Score
            {
                PlayerId = player.Id,
                GameId = game.Id,
                Value = scoreValue,
                SubmittedAt = DateTime.UtcNow
            };

            context.Scores.Add(newScore);
        }

        await context.SaveChangesAsync();
        return null;
    }

    public async Task<LeaderboardEntryResponse?> GetPlayerBestScoreAsync(string gameKey, string externalId)
    {
        var game = await context.FindGameByKeyAsync(gameKey);
        if (game is null) return null;

        var player = await playerService.GetPlayerByExternalIdAsync(externalId);
        if (player is null) return null;

        var playerScore = await context.Scores
            .FirstOrDefaultAsync(s => s.GameId == game.Id && s.PlayerId == player.Id);

        if (playerScore is null) return null;

        // Oblicz prawdziwy rank — ile graczy ma wyższy wynik + 1
        var rank = await context.Scores
            .CountAsync(s => s.GameId == game.Id && s.Value > playerScore.Value) + 1;

        return new LeaderboardEntryResponse(rank, player.DisplayName, playerScore.Value, player.Platform);
    }
}
