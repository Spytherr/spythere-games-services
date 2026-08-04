using Microsoft.EntityFrameworkCore;

namespace SpythereGamesServices;

public static class ScoresEndpoints
{
    public static void MapScoresEndpoints(this WebApplication app)
    {
        app.MapGet("/api/games/{gameKey}/scores/top", async (string gameKey, ILeaderboardService leaderboardService, CancellationToken ct, int count = 10) =>
        {
            var topScores = await leaderboardService.GetTopScoresAsync(gameKey, count, ct);
            if (topScores is null) return Results.NotFound(new { Message = "Game not found" });

            return Results.Ok(topScores);
        })
        .WithName("GetTopScores");

        app.MapPost("/api/games/{gameKey}/scores", async (string gameKey, SubmitScoreRequest request, ILeaderboardService leaderboardService, IGoogleAuthService googleAuth, CancellationToken ct) =>
        {
            if (request.ScoreValue < 0 || request.ScoreValue > 10_000_000)
            {
                return Results.BadRequest(new { Message = "Score value is out of allowed bounds" });
            }

            var playerInfo = await googleAuth.VerifyAuthCodeAsync(request.AuthCode, ct);
            if (playerInfo is null)
                return Results.Unauthorized();

            var errorMessage = await leaderboardService.SubmitScoreAsync(
                gameKey, 
                playerInfo.ExternalId,
                playerInfo.DisplayName,
                request.ScoreValue,
                ct
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

        app.MapGet("/api/games/{gameKey}/scores/player/{externalId}", async (string gameKey, string externalId, ILeaderboardService leaderboardService, CancellationToken ct) =>
        {
            var bestScore = await leaderboardService.GetPlayerBestScoreAsync(gameKey, externalId, ct);
            if (bestScore is null) return Results.NotFound(new { Message = "No scores found for this player or game" });

            return Results.Ok(bestScore);
        })
        .WithName("GetPlayerBestScore");
    }
}
