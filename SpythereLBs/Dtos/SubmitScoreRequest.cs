namespace SpythereGamesServices;

public record SubmitScoreRequest(
    string ExternalId,
    string Platform,
    long ScoreValue
);

