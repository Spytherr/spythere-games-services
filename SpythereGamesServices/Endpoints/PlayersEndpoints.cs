using Microsoft.EntityFrameworkCore;

namespace SpythereGamesServices;

public static class PlayersEndpoints
{
    public static void MapPlayersEndpoints(this WebApplication app)
    {
        // GET /api/players/{id}
        app.MapGet("/api/players/{id}", async (int id, IPlayerService playerService) =>
        {
            var player = await playerService.GetPlayerAsync(id);
            if (player is null) return Results.NotFound();
            
            return Results.Ok(new { player.Id, player.DisplayName, player.Platform });
        })
        .WithName("GetPlayer");

        // POST /api/players
        app.MapPost("/api/players", async (RegisterPlayerRequest request, IPlayerService playerService) =>
        {
            var existingPlayer = await playerService.GetPlayerByExternalIdAsync(request.ExternalId);

            if (existingPlayer is not null)
            {
                return Results.Ok(new { Message = "Player already exists", PlayerId = existingPlayer.Id });
            }

            var newPlayer = await playerService.RegisterPlayerAsync(
                request.ExternalId, 
                request.DisplayName, 
                request.Platform
            );

            return Results.Created($"/api/players/{newPlayer.Id}", new { Message = "Player registered successfully", PlayerId = newPlayer.Id });
        })
        .WithName("RegisterPlayer");
    }
}
