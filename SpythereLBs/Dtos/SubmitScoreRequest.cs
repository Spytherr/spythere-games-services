namespace SpythereLBs;

public record SubmitScoreRequest(
    string ExternalId,
    string Platform,
    long ScoreValue
);

