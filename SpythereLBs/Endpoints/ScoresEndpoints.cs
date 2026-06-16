using System;

namespace SpythereLBs;

public static class ScoresEndpoints
{

    public static void MapScoresEndpoints(this WebApplication app)
    {
        app.MapGet("/scores", () =>
        {
            var leaderboard = new List<LeaderboardEntryResponse>
            {
                new LeaderboardEntryResponse(1, "Player1", 1000, "Android"),
                new LeaderboardEntryResponse(2, "Player2", 900, "IOS"),
                new LeaderboardEntryResponse(3, "Player3", 800, "Android")
            };

            return leaderboard;
        })
        .WithName("GetScores");

        app.MapPost("/scores", (SubmitScoreRequest request) =>
        {
            // Here you would typically process the score submission, e.g., save it to a database
            // For this example, we'll just return a success message
            return Results.Ok(new { Message = "Score submitted successfully", PlayerId = request.ExternalId });
        })
        .WithName("SubmitScore");

        app.MapGet("/scores/{playerId}", (string playerId) =>
        {
            // Here you would typically retrieve the player's score from a database
            // For this example, we'll just return a mock score
            var playerScore = new LeaderboardEntryResponse(1, playerId, 1000, "IOS");
            return Results.Ok(playerScore);
        })
        .WithName("GetScoresPlayerScore");
    }

}
