namespace SpythereGamesServices;

public record RegisterPlayerRequest(
    string AuthCode,
    string Platform
);
