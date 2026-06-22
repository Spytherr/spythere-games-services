using Microsoft.EntityFrameworkCore;

namespace SpythereGamesServices;

public static class GamesEndpoints
{
    public static void MapGamesEndpoints(this WebApplication app)
    {
        // GET /api/games — lista wszystkich gier (publiczny, dla strony portfolio)
        app.MapGet("/api/games", async (SpythereGamesServicesContext context) =>
        {
            var games = await context.Games
                .Select(g => new GameResponse(g.Id, g.Name, g.Description, g.IconUrl))
                .ToListAsync();

            return Results.Ok(games);
        })
        .WithName("GetAllGames");

        // GET /api/games/{gameKey} — szczegóły jednej gry (publiczny)
        app.MapGet("/api/games/{gameKey}", async (string gameKey, SpythereGamesServicesContext context) =>
        {
            var game = await context.FindGameByKeyAsync(gameKey);
            if (game is null) return Results.NotFound(new MessageResponse("Game not found"));

            return Results.Ok(new GameResponse(game.Id, game.Name, game.Description, game.IconUrl));
        })
        .WithName("GetGame");
    }
}
