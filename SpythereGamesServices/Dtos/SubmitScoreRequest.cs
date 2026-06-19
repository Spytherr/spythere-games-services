namespace SpythereGamesServices;

public record SubmitScoreRequest(
    string AuthCode,
    long ScoreValue
);
