namespace SpythereGamesServices;

public interface IPlayerAuthService
{
    Task<VerifiedPlayerInfo?> VerifyAsync(AuthCredentials credentials, CancellationToken ct = default);
}
