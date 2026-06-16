using System;

namespace SpythereLBs;

public static class ChvChEndpoints
{

    public static void MapChvChEndpoints(this WebApplication app)
    {
        app.MapGet("/chvchscores", () =>
        {
            var leaderboard = new List<ChvChLeaderboardEntryResponse>
            {
                new ChvChLeaderboardEntryResponse(1, "Player1", 1000, "PC"),
                new ChvChLeaderboardEntryResponse(2, "Player2", 900, "Console"),
                new ChvChLeaderboardEntryResponse(3, "Player3", 800, "Mobile")
            };

            return leaderboard;
        })
        .WithName("GetChvchScores");

        app.MapPost("/chvchscores", (ChvChSubmitScoreRequest request) =>
        {
            // Here you would typically process the score submission, e.g., save it to a database
            // For this example, we'll just return a success message
            return Results.Ok(new { Message = "Score submitted successfully", PlayerId = request.PlayerId });
        })
        .WithName("SubmitChvchScore");

        app.MapGet("/chvchscores/{playerId}", (string playerId) =>
        {
            // Here you would typically retrieve the player's score from a database
            // For this example, we'll just return a mock score
            var playerScore = new ChvChLeaderboardEntryResponse(1, playerId, 1000, "PC");
            return Results.Ok(playerScore);
        })
        .WithName("GetChvchPlayerScore");
    }

}
