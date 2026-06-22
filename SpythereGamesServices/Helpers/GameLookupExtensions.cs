using Microsoft.EntityFrameworkCore;

namespace SpythereGamesServices;

public static class GameLookupExtensions
{
    public static async Task<Game?> FindGameByKeyAsync(this SpythereGamesServicesContext context, string gameKey)
    {
        return await context.Games.FirstOrDefaultAsync(g => g.Key == gameKey);
    }
}
