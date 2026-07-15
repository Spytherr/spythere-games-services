using Microsoft.EntityFrameworkCore;

namespace SpythereGamesServices;

public class PlayerService(SpythereGamesServicesContext context) : IPlayerService
{
    public async Task<Player?> GetPlayerAsync(int id)
    {
        return await context.Players.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Player?> GetPlayerByExternalIdAsync(string externalId)
    {
        return await context.Players.AsNoTracking().FirstOrDefaultAsync(p => p.ExternalId == externalId);
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

    public async Task<bool> DeletePlayerAsync(int id)
    {
        var rowsDeleted = await context.Players
            .Where(p => p.Id == id)
            .ExecuteDeleteAsync();

        return rowsDeleted > 0;
    }
}
