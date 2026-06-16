using System;

namespace SpythereGamesServices;

public static class PlayersEndpoints
{

    public static void MapPlayersEndpoints(this WebApplication app)
    {
        app.MapGet("/players/{playerId}", (string playerId) =>
        {
            // Here you would typically retrieve the player's information from a database
            // For this example, we'll just return a mock player info
            var playerInfo = new { PlayerId = playerId, DisplayName = "Player1", Platform = "PC" };
            return Results.Ok(playerInfo);
        })
        .WithName("GetPlayerInfo");

        app.MapPost("/players", (RegisterPlayerRequest request) =>
        {
            // Here you would typically process the player registration, e.g., save it to a database
            // For this example, we'll just return a success message
            return Results.Ok(new { Message = "Player registered successfully", PlayerId = request.ExternalId });
        })
        .WithName("RegisterPlayer");
    }

}
