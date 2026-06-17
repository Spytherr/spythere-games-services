using Microsoft.EntityFrameworkCore;

namespace SpythereGamesServices;

public static class ScoresEndpoints
{
    public static void MapScoresEndpoints(this WebApplication app)
    {
        // GET /api/games/{gameKey}/scores/top
        app.MapGet("/api/games/{gameKey}/scores/top", async (string gameKey, SpythereGamesServicesContext context, int count = 10) =>
        {
            var game = await context.Games.FirstOrDefaultAsync(g => g.Key == gameKey);
            if (game is null) return Results.NotFound(new { Message = "Game not found" });

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

            return Results.Ok(topScores);
        })
        .WithName("GetTopScores");

        // POST /api/games/{gameKey}/scores
        app.MapPost("/api/games/{gameKey}/scores", async (string gameKey, SubmitScoreRequest request, SpythereGamesServicesContext context) =>
        {
            var game = await context.Games.FirstOrDefaultAsync(g => g.Key == gameKey);
            if (game is null) return Results.NotFound(new { Message = "Game not found" });

            var player = await context.Players.FirstOrDefaultAsync(p => p.ExternalId == request.ExternalId);
            if (player is null) return Results.NotFound(new { Message = "Player not found. Register first." });

            var newScore = new Score
            {
                PlayerId = player.Id,
                GameId = game.Id,
                Value = request.ScoreValue,
                SubmittedAt = DateTime.UtcNow
            };

            context.Scores.Add(newScore);
            await context.SaveChangesAsync();

            return Results.Ok(new { Message = "Score submitted successfully" });
        })
        .WithName("SubmitScore");

        // GET /api/games/{gameKey}/scores/player/{playerId}
        app.MapGet("/api/games/{gameKey}/scores/player/{playerId}", async (string gameKey, string playerId, SpythereGamesServicesContext context) =>
        {
            var game = await context.Games.FirstOrDefaultAsync(g => g.Key == gameKey);
            if (game is null) return Results.NotFound(new { Message = "Game not found" });

            var player = await context.Players.FirstOrDefaultAsync(p => p.ExternalId == playerId);
            if (player is null) return Results.NotFound(new { Message = "Player not found" });

            var bestScore = await context.Scores
                .Where(s => s.GameId == game.Id && s.PlayerId == player.Id)
                .OrderByDescending(s => s.Value)
                .FirstOrDefaultAsync();

            if (bestScore is null) return Results.NotFound(new { Message = "No scores found for this player" });

            // We don't have a fast way to get rank without window functions, but for now we just return the score
            var response = new LeaderboardEntryResponse(0, player.DisplayName, bestScore.Value, player.Platform);
            return Results.Ok(response);
        })
        .WithName("GetPlayerBestScore");
    }
}
