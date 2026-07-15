namespace SpythereGamesServices;

public interface IPlayerService
{
    Task<Player?> GetPlayerAsync(int id, CancellationToken ct = default);
    Task<Player?> GetPlayerByExternalIdAsync(string externalId, CancellationToken ct = default);
    Task<Player> RegisterPlayerAsync(string externalId, string displayName, string platform, CancellationToken ct = default);
    Task<bool> DeletePlayerAsync(int id, CancellationToken ct = default);
}
