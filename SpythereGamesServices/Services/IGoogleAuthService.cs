namespace SpythereGamesServices;

public interface IGoogleAuthService
{
    Task<GooglePlayerInfo?> VerifyAuthCodeAsync(string authCode);
}
