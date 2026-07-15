namespace SpythereGamesServices;

public interface ILeaderboardService
{
    Task<List<LeaderboardEntryResponse>?> GetTopScoresAsync(string gameKey, int count = 10, CancellationToken ct = default);
    Task<string?> SubmitScoreAsync(string gameKey, string externalId, long scoreValue, CancellationToken ct = default);
    Task<LeaderboardEntryResponse?> GetPlayerBestScoreAsync(string gameKey, string externalId, CancellationToken ct = default);
}
