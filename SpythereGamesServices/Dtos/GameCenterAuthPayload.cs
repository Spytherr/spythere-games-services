namespace SpythereGamesServices;

public record GameCenterAuthPayload(
    string PlayerId,
    string BundleId,
    string PublicKeyUrl,
    string Signature,
    string Salt,
    ulong Timestamp,
    string? DisplayName
);
