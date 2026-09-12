namespace SpythereGamesServices;

public interface IGoogleAuthService
{
    Task<VerifiedPlayerInfo?> VerifyAuthCodeAsync(string authCode, CancellationToken ct = default);
}
