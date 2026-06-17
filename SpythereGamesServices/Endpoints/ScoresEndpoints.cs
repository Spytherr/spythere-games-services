using Microsoft.EntityFrameworkCore;

namespace SpythereGamesServices;

public static class ScoresEndpoints
{
    public static void MapScoresEndpoints(this WebApplication app)
    {
        // GET /api/games/{gameKey}/scores/top
        app.MapGet("/api/games/{gameKey}/scores/top", async (string gameKey, ILeaderboardService leaderboardService, int count = 10) =>
        {
            var topScores = await leaderboardService.GetTopScoresAsync(gameKey, count);
            if (topScores is null) return Results.NotFound(new { Message = "Game not found" });

            return Results.Ok(topScores);
        })
        .WithName("GetTopScores");

        // POST /api/games/{gameKey}/scores
        app.MapPost("/api/games/{gameKey}/scores", async (string gameKey, SubmitScoreRequest request, ILeaderboardService leaderboardService) =>
        {
            var errorMessage = await leaderboardService.SubmitScoreAsync(
                gameKey, 
                request.ExternalId, 
                request.ScoreValue
            );

            if (errorMessage is null)
            {
                return Results.Ok(new { Message = "Score submitted successfully" });
            }
            else
            {
                return Results.NotFound(new { Message = errorMessage });
            }
        })
        .WithName("SubmitScore");

        // GET /api/games/{gameKey}/scores/player/{playerId}
        app.MapGet("/api/games/{gameKey}/scores/player/{playerId}", async (string gameKey, string playerId, ILeaderboardService leaderboardService) =>
        {
            var bestScore = await leaderboardService.GetPlayerBestScoreAsync(gameKey, playerId);
            if (bestScore is null) return Results.NotFound(new { Message = "No scores found for this player or game" });

            return Results.Ok(bestScore);
        })
        .WithName("GetPlayerBestScore");
    }
}
