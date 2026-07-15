using Microsoft.EntityFrameworkCore;

namespace SpythereGamesServices;

public static class GamesEndpoints
{
    public static void MapGamesEndpoints(this WebApplication app)
    {
        // GET /api/games — lista wszystkich gier (publiczny, dla strony portfolio)
        app.MapGet("/api/games", async (SpythereGamesServicesContext context, CancellationToken ct) =>
        {
            var games = await context.Games
                .AsNoTracking()
                .Select(g => new GameResponse(g.Id, g.Key, g.Name, g.Description))
                .ToListAsync(ct);

            return Results.Ok(games);
        })
        .WithName("GetAllGames");

        // GET /api/games/{gameKey} — szczegóły jednej gry (publiczny)
        app.MapGet("/api/games/{gameKey}", async (string gameKey, SpythereGamesServicesContext context, CancellationToken ct) =>
        {
            var game = await context.Games.AsNoTracking().FirstOrDefaultAsync(g => g.Key == gameKey, ct);
            if (game is null) return Results.NotFound(new { Message = "Game not found" });

            return Results.Ok(new GameResponse(game.Id, game.Key, game.Name, game.Description));
        })
        .WithName("GetGame");
    }
}
