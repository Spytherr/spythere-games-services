namespace SpythereLBs;

public record RegisterPlayerRequest(
    string ExternalId,
    string DisplayName,
    string Platform
);

