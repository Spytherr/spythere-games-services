namespace SpythereGamesServices;

public static class GoogleAuthHelper
{
    public static async Task<(GooglePlayerInfo? PlayerInfo, IResult? ErrorResult)> VerifyOrUnauthorizedAsync(
        IGoogleAuthService googleAuth, string authCode)
    {
        var playerInfo = await googleAuth.VerifyAuthCodeAsync(authCode);
        if (playerInfo is null)
            return (null, Results.Unauthorized());

        return (playerInfo, null);
    }
}
