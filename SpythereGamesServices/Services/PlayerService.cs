using Microsoft.EntityFrameworkCore;

namespace SpythereGamesServices;

public class PlayerService(SpythereGamesServicesContext context) : IPlayerService
{
    public async Task<Player?> GetPlayerAsync(int id, CancellationToken ct = default)
    {
        return await context.Players.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<Player?> GetPlayerByExternalIdAsync(string externalId, CancellationToken ct = default)
    {
        return await context.Players.AsNoTracking().FirstOrDefaultAsync(p => p.ExternalId == externalId, ct);
    }

    public async Task<Player> RegisterPlayerAsync(string externalId, string displayName, string platform, CancellationToken ct = default)
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
        await context.SaveChangesAsync(ct);

        return newPlayer;
    }

    public async Task<bool> DeletePlayerAsync(int id, CancellationToken ct = default)
    {
        var rowsDeleted = await context.Players
            .Where(p => p.Id == id)
            .ExecuteDeleteAsync(ct);

        return rowsDeleted > 0;
    }
}
