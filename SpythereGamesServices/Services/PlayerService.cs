using Microsoft.EntityFrameworkCore;

namespace SpythereGamesServices;

public class PlayerService(SpythereGamesServicesContext context) : IPlayerService
{
    public async Task<Player?> GetPlayerAsync(int id)
    {
        return await context.Players.FindAsync(id);
    }

    public async Task<Player?> GetPlayerByExternalIdAsync(string externalId)
    {
        return await context.Players.FirstOrDefaultAsync(p => p.ExternalId == externalId);
    }

    public async Task<Player> RegisterPlayerAsync(string externalId, string displayName, string platform)
    {
        var newPlayer = new Player
        {
            DisplayName = displayName,
            Platform = platform,
            ExternalId = externalId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Players.Add(newPlayer);
        await context.SaveChangesAsync();

        return newPlayer;
    }

    public async Task DeletePlayerAsync(int id)
    {
        var player = await context.Players
            .Include(p => p.Scores)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (player is not null)
        {
            context.Players.Remove(player);
            await context.SaveChangesAsync();
        }
    }
}
