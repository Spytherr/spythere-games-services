namespace SpythereLBs;

public record RegisterPlayerRequest(
    string DisplayName,
    ScoresDto Scores,
    string Platform,
    string ExternalId
);
public record ScoresDto(
    int ChvChScore
);

