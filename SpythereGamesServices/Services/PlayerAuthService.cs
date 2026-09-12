namespace SpythereGamesServices;

public class PlayerAuthService(IGoogleAuthService googleAuth, GameCenterAuthService gameCenterAuth) : IPlayerAuthService
{
    public async Task<VerifiedPlayerInfo?> VerifyAsync(AuthCredentials credentials, CancellationToken ct = default)
    {
        if (credentials.GameCenter is not null || PlatformNames.IsIos(credentials.Platform))
        {
            if (credentials.GameCenter is null) return null;
            return await gameCenterAuth.VerifyAsync(credentials.GameCenter, ct);
        }

        if (string.IsNullOrEmpty(credentials.AuthCode)) return null;
        return await googleAuth.VerifyAuthCodeAsync(credentials.AuthCode, ct);
    }
}
