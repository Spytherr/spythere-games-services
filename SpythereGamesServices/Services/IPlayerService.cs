namespace SpythereGamesServices;

public interface IPlayerService
{
    Task<Player?> GetPlayerAsync(int id);
    Task<Player?> GetPlayerByExternalIdAsync(string externalId);
    Task<Player> RegisterPlayerAsync(string externalId, string displayName, string platform);
    Task DeletePlayerAsync(int id);
}
