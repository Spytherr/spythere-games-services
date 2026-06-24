namespace SpythereGamesServices;

public record SubmitScoreByExternalIdRequest(
    string ExternalId,
    long ScoreValue
);
