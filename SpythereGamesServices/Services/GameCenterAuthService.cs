using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.Extensions.Caching.Memory;

namespace SpythereGamesServices;

public class GameCenterAuthService(
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory,
    IMemoryCache cache,
    ILogger<GameCenterAuthService> logger)
{
    public async Task<VerifiedPlayerInfo?> VerifyAsync(GameCenterAuthPayload payload, CancellationToken ct = default)
    {
        if (!configuration.GetValue<bool>("GameCenter:Enabled"))
        {
            logger.LogWarning("Game Center verification attempted but GameCenter:Enabled is false");
            return null;
        }

        var allowedBundleIds = configuration.GetSection("GameCenter:AllowedBundleIds").Get<string[]>() ?? [];
        if (allowedBundleIds.Length == 0 || !allowedBundleIds.Contains(payload.BundleId))
        {
            logger.LogWarning("Game Center bundle ID {BundleId} is not in the allowed list", payload.BundleId);
            return null;
        }

        var maxAgeMinutes = configuration.GetValue("GameCenter:MaxTimestampAgeMinutes", 10);
        var signatureTime = DateTimeOffset.FromUnixTimeMilliseconds((long)payload.Timestamp);
        if (Math.Abs((DateTimeOffset.UtcNow - signatureTime).TotalMinutes) > maxAgeMinutes)
        {
            logger.LogWarning("Game Center signature timestamp is stale or in the future: {Timestamp}", payload.Timestamp);
            return null;
        }

        var certBytes = await GetCertificateAsync(payload.PublicKeyUrl, ct);
        if (certBytes is null) return null;

        try
        {
            using var cert = X509CertificateLoader.LoadCertificate(certBytes);
            using var rsa = cert.GetRSAPublicKey();
            if (rsa is null)
            {
                logger.LogWarning("Game Center certificate does not contain an RSA public key");
                return null;
            }

            var salt = Convert.FromBase64String(payload.Salt);
            var signature = Convert.FromBase64String(payload.Signature);

            var playerIdBytes = Encoding.UTF8.GetBytes(payload.PlayerId);
            var bundleIdBytes = Encoding.UTF8.GetBytes(payload.BundleId);
            var timestampBytes = new byte[8];
            BinaryPrimitives.WriteUInt64BigEndian(timestampBytes, payload.Timestamp);

            var data = new byte[playerIdBytes.Length + bundleIdBytes.Length + timestampBytes.Length + salt.Length];
            Buffer.BlockCopy(playerIdBytes, 0, data, 0, playerIdBytes.Length);
            Buffer.BlockCopy(bundleIdBytes, 0, data, playerIdBytes.Length, bundleIdBytes.Length);
            Buffer.BlockCopy(timestampBytes, 0, data, playerIdBytes.Length + bundleIdBytes.Length, timestampBytes.Length);
            Buffer.BlockCopy(salt, 0, data, playerIdBytes.Length + bundleIdBytes.Length + timestampBytes.Length, salt.Length);

            var isValid = rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            if (!isValid)
            {
                logger.LogWarning("Game Center signature verification failed for player {PlayerId}", payload.PlayerId);
                return null;
            }

            logger.LogInformation("Successfully verified Game Center player {PlayerId}", payload.PlayerId);
            return new VerifiedPlayerInfo(payload.PlayerId, payload.DisplayName ?? "Player", PlatformNames.Ios);
        }
        catch (Exception ex) when (ex is FormatException or CryptographicException)
        {
            logger.LogWarning(ex, "Game Center verification failed for player {PlayerId}", payload.PlayerId);
            return null;
        }
    }

    private async Task<byte[]?> GetCertificateAsync(string publicKeyUrl, CancellationToken ct)
    {
        if (!Uri.TryCreate(publicKeyUrl, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
        {
            logger.LogWarning("Invalid Game Center public key URL: {Url}", publicKeyUrl);
            return null;
        }

        var allowedHosts = configuration.GetSection("GameCenter:AllowedCertHosts").Get<string[]>() ?? ["static.gc.apple.com"];
        if (!allowedHosts.Contains(uri.Host, StringComparer.OrdinalIgnoreCase))
        {
            logger.LogWarning("Game Center public key host {Host} is not in the allowed list", uri.Host);
            return null;
        }

        var cacheKey = $"gc-cert:{publicKeyUrl}";
        if (cache.TryGetValue<byte[]>(cacheKey, out var cached))
            return cached;

        var httpClient = httpClientFactory.CreateClient();
        var response = await httpClient.GetAsync(uri, ct);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Failed to download Game Center certificate. Status: {StatusCode}", response.StatusCode);
            return null;
        }

        var bytes = await response.Content.ReadAsByteArrayAsync(ct);
        cache.Set(cacheKey, bytes, TimeSpan.FromHours(24));
        return bytes;
    }
}
