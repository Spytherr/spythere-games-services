using Microsoft.AspNetCore.Mvc;

namespace SpythereGamesServices;

public static class PlayersEndpoints
{
    public static void MapPlayersEndpoints(this WebApplication app)
    {
        // GET /api/players/{id} — publiczny (dla strony portfolio)
        app.MapGet("/api/players/{id}", async (int id, IPlayerService playerService) =>
        {
            var player = await playerService.GetPlayerAsync(id);
            if (player is null) return Results.NotFound();
            
            return Results.Ok(new { player.Id, player.DisplayName, player.Platform });
        })
        .WithName("GetPlayer");

        // POST /api/players — rejestracja (wymaga API Key + Google Auth)
        app.MapPost("/api/players", async (RegisterPlayerRequest request, IPlayerService playerService, IGoogleAuthService googleAuth) =>
        {
            // Weryfikuj tożsamość przez Google
            var playerInfo = await googleAuth.VerifyAuthCodeAsync(request.AuthCode);
            if (playerInfo is null)
                return Results.Unauthorized();

            var existingPlayer = await playerService.GetPlayerByExternalIdAsync(playerInfo.ExternalId);

            if (existingPlayer is not null)
            {
                return Results.Ok(new { Message = "Player already exists", PlayerId = existingPlayer.Id });
            }

            var newPlayer = await playerService.RegisterPlayerAsync(
                playerInfo.ExternalId, 
                playerInfo.DisplayName, 
                request.Platform
            );

            return Results.Created($"/api/players/{newPlayer.Id}", new { Message = "Player registered successfully", PlayerId = newPlayer.Id });
        })
        .WithName("RegisterPlayer");

        // DELETE /api/players/me — usunięcie danych gracza (RODO)
        app.MapDelete("/api/players/me", async ([FromBody] DeletePlayerRequest request, IPlayerService playerService, IGoogleAuthService googleAuth) =>
        {
            // Weryfikuj tożsamość — tylko gracz może usunąć SWOJE dane
            var playerInfo = await googleAuth.VerifyAuthCodeAsync(request.AuthCode);
            if (playerInfo is null)
                return Results.Unauthorized();

            var player = await playerService.GetPlayerByExternalIdAsync(playerInfo.ExternalId);
            if (player is null)
                return Results.NotFound(new { Message = "Player not found" });

            await playerService.DeletePlayerAsync(player.Id);

            return Results.NoContent();
        })
        .WithName("DeletePlayer");
    }
}
