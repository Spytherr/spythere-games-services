namespace SpythereGamesServices;

public interface ILeaderboardService
{
    Task<List<LeaderboardEntryResponse>?> GetTopScoresAsync(string gameKey, int count = 10);
    Task<string?> SubmitScoreAsync(string gameKey, string externalId, long scoreValue);
    Task<LeaderboardEntryResponse?> GetPlayerBestScoreAsync(string gameKey, string externalId);
}
