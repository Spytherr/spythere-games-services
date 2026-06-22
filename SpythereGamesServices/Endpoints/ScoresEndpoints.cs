namespace SpythereGamesServices;

public static class ScoresEndpoints
{
    public static void MapScoresEndpoints(this WebApplication app)
    {
        // GET /api/games/{gameKey}/scores/top — publiczny (dla strony portfolio)
        app.MapGet("/api/games/{gameKey}/scores/top", async (string gameKey, ILeaderboardService leaderboardService, int count = 10) =>
        {
            var topScores = await leaderboardService.GetTopScoresAsync(gameKey, count);
            if (topScores is null) return Results.NotFound(new MessageResponse("Game not found"));

            return Results.Ok(topScores);
        })
        .WithName("GetTopScores");

        // POST /api/games/{gameKey}/scores — wymaga API Key + Google Auth
        app.MapPost("/api/games/{gameKey}/scores", async (string gameKey, SubmitScoreRequest request, ILeaderboardService leaderboardService, IGoogleAuthService googleAuth) =>
        {
            var (playerInfo, error) = await GoogleAuthHelper.VerifyOrUnauthorizedAsync(googleAuth, request.AuthCode);
            if (error is not null) return error;

            var errorMessage = await leaderboardService.SubmitScoreAsync(
                gameKey, 
                playerInfo!.ExternalId,
                request.ScoreValue
            );

            if (errorMessage is null)
            {
                return Results.Ok(new MessageResponse("Score submitted successfully"));
            }
            else
            {
                return Results.NotFound(new MessageResponse(errorMessage));
            }
        })
        .WithName("SubmitScore");

        // GET /api/games/{gameKey}/scores/player/{externalId} — publiczny
        app.MapGet("/api/games/{gameKey}/scores/player/{externalId}", async (string gameKey, string externalId, ILeaderboardService leaderboardService) =>
        {
            var bestScore = await leaderboardService.GetPlayerBestScoreAsync(gameKey, externalId);
            if (bestScore is null) return Results.NotFound(new MessageResponse("No scores found for this player or game"));

            return Results.Ok(bestScore);
        })
        .WithName("GetPlayerBestScore");
    }
}
