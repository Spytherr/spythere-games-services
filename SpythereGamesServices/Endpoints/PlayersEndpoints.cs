using Microsoft.AspNetCore.Mvc;

namespace SpythereGamesServices;

public static class PlayersEndpoints
{
    public static void MapPlayersEndpoints(this WebApplication app)
    {
        app.MapGet("/api/players/{id}", async (int id, IPlayerService playerService, CancellationToken ct) =>
        {
            var player = await playerService.GetPlayerAsync(id, ct);
            if (player is null) return Results.NotFound();
            
            return Results.Ok(new { player.Id, player.DisplayName, player.Platform });
        })
        .WithName("GetPlayer");

        app.MapPost("/api/players", async (RegisterPlayerRequest request, IPlayerService playerService, IGoogleAuthService googleAuth, CancellationToken ct) =>
        {
            var playerInfo = await googleAuth.VerifyAuthCodeAsync(request.AuthCode, ct);
            if (playerInfo is null)
                return Results.Unauthorized();

            var existingPlayer = await playerService.GetPlayerByExternalIdAsync(playerInfo.ExternalId, ct);

            if (existingPlayer is not null)
            {
                return Results.Ok(new { Message = "Player already exists", PlayerId = existingPlayer.Id, existingPlayer.ExternalId });
            }

            var newPlayer = await playerService.RegisterPlayerAsync(
                playerInfo.ExternalId, 
                playerInfo.DisplayName, 
                request.Platform,
                ct
            );

            return Results.Created($"/api/players/{newPlayer.Id}", new { Message = "Player registered successfully", PlayerId = newPlayer.Id, newPlayer.ExternalId });
        })
        .WithName("RegisterPlayer");

        app.MapDelete("/api/players/me", async ([FromBody] DeletePlayerRequest request, IPlayerService playerService, IGoogleAuthService googleAuth, CancellationToken ct) =>
        {
            var playerInfo = await googleAuth.VerifyAuthCodeAsync(request.AuthCode, ct);
            if (playerInfo is null)
                return Results.Unauthorized();

            var player = await playerService.GetPlayerByExternalIdAsync(playerInfo.ExternalId, ct);
            if (player is null)
                return Results.NotFound(new { Message = "Player not found" });

            await playerService.DeletePlayerAsync(player.Id, ct);

            return Results.NoContent();
        })
        .WithName("DeletePlayer");
    }
}
