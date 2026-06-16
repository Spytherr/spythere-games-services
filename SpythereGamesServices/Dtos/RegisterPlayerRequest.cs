namespace SpythereGamesServices;

public record RegisterPlayerRequest(
    string ExternalId,
    string DisplayName,
    string Platform
);

