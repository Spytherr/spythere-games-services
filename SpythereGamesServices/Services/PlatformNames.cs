namespace SpythereGamesServices;

public static class PlatformNames
{
    public const string Android = "android";
    public const string Ios = "ios";

    public static string Normalize(string? platform) => (platform ?? "").Trim().ToLowerInvariant() switch
    {
        "" or Android or "google" => Android,
        Ios or "gamecenter" or "apple" => Ios,
        var other => other
    };

    public static bool IsIos(string? platform) => Normalize(platform) == Ios;
}
