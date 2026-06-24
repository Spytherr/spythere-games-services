using Microsoft.EntityFrameworkCore;

namespace SpythereGamesServices;

public static class ScoresEndpoints
{
    public static void MapScoresEndpoints(this WebApplication app)
    {
        // GET /api/games/{gameKey}/scores/top — publiczny (dla strony portfolio)
        app.MapGet("/api/games/{gameKey}/scores/top", async (string gameKey, ILeaderboardService leaderboardService, int count = 10) =>
        {
            var topScores = await leaderboardService.GetTopScoresAsync(gameKey, count);
            if (topScores is null) return Results.NotFound(new { Message = "Game not found" });

            return Results.Ok(topScores);
        })
        .WithName("GetTopScores");

        // POST /api/games/{gameKey}/scores — wymaga API Key + Google Auth
        app.MapPost("/api/games/{gameKey}/scores", async (string gameKey, SubmitScoreRequest request, ILeaderboardService leaderboardService, IGoogleAuthService googleAuth) =>
        {
            // Weryfikuj tożsamość gracza
            var playerInfo = await googleAuth.VerifyAuthCodeAsync(request.AuthCode);
            if (playerInfo is null)
                return Results.Unauthorized();

            var errorMessage = await leaderboardService.SubmitScoreAsync(
                gameKey, 
                playerInfo.ExternalId,  // ExternalId z weryfikacji Google, nie od klienta
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

        // PUT /api/games/{gameKey}/scores — submit score by externalId (no auth code exchange needed)
        // Used by registered players to submit scores without the overhead of Google token exchange.
        // Protected by API Key middleware.
        app.MapPut("/api/games/{gameKey}/scores", async (string gameKey, SubmitScoreByExternalIdRequest request, ILeaderboardService leaderboardService) =>
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
        .WithName("SubmitScoreByExternalId");

        // GET /api/games/{gameKey}/scores/player/{externalId} — publiczny
        app.MapGet("/api/games/{gameKey}/scores/player/{externalId}", async (string gameKey, string externalId, ILeaderboardService leaderboardService) =>
        {
            var bestScore = await leaderboardService.GetPlayerBestScoreAsync(gameKey, externalId);
            if (bestScore is null) return Results.NotFound(new { Message = "No scores found for this player or game" });

            return Results.Ok(bestScore);
        })
        .WithName("GetPlayerBestScore");
    }
}
