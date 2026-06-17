using Microsoft.EntityFrameworkCore;

namespace SpythereGamesServices;

public static class PlayersEndpoints
{
    public static void MapPlayersEndpoints(this WebApplication app)
    {
        // GET /api/players/{id}
        app.MapGet("/api/players/{id}", async (int id, SpythereGamesServicesContext context) =>
        {
            var player = await context.Players.FindAsync(id);
            if (player is null) return Results.NotFound();
            
            return Results.Ok(new { player.Id, player.DisplayName, player.Platform });
        })
        .WithName("GetPlayer");

        // POST /api/players
        app.MapPost("/api/players", async (RegisterPlayerRequest request, SpythereGamesServicesContext context) =>
        {
            var existingPlayer = await context.Players
                .FirstOrDefaultAsync(p => p.ExternalId == request.ExternalId);

            if (existingPlayer is not null)
            {
                return Results.Ok(new { Message = "Player already exists", PlayerId = existingPlayer.Id });
            }

            var newPlayer = new Player
            {
                DisplayName = request.DisplayName,
                Platform = request.Platform,
                ExternalId = request.ExternalId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.Players.Add(newPlayer);
            await context.SaveChangesAsync();

            return Results.Created($"/api/players/{newPlayer.Id}", new { Message = "Player registered successfully", PlayerId = newPlayer.Id });
        })
        .WithName("RegisterPlayer");
    }
}
