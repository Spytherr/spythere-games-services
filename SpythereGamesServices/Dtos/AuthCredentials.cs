namespace SpythereGamesServices;

public record AuthCredentials(
    string? Platform,
    string? AuthCode,
    GameCenterAuthPayload? GameCenter
);
