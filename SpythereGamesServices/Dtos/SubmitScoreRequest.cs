namespace SpythereGamesServices;

public record SubmitScoreRequest(
    string? AuthCode,
    string? Platform,
    long ScoreValue,
    GameCenterAuthPayload? GameCenter
);
