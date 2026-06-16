namespace SpythereLBs;

public record LeaderboardEntryResponse(
    int Rank,
    string DisplayName,
    long ScoreValue,
    string Platform
);

