using System.Text.Json;
using Google.Apis.Auth;

namespace SpythereGamesServices;

public class GoogleAuthService(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<GoogleAuthService> logger) : IGoogleAuthService
{
    public async Task<GooglePlayerInfo?> VerifyAuthCodeAsync(string authCode, CancellationToken ct = default)
    {
        var clientId = configuration["GoogleAuth:ClientId"];
        var clientSecret = configuration["GoogleAuth:ClientSecret"];

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            throw new InvalidOperationException("GoogleAuth:ClientId or GoogleAuth:ClientSecret is not configured");
        }

        var httpClient = httpClientFactory.CreateClient();

        var tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["code"] = authCode,
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["redirect_uri"] = "",
            ["grant_type"] = "authorization_code"
        });

        var response = await httpClient.PostAsync("https://oauth2.googleapis.com/token", tokenRequest, ct);
        if (!response.IsSuccessStatusCode)
        {
            var errBody = await response.Content.ReadAsStringAsync(ct);
            logger.LogWarning("Google token exchange failed with status {StatusCode}. Body: {Body}", response.StatusCode, errBody);
            return null;
        }

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);

        if (!json.TryGetProperty("access_token", out var accessTokenElement))
        {
            logger.LogWarning("Google token response did not contain an access_token");
            return null;
        }

        var accessToken = accessTokenElement.GetString();
        if (string.IsNullOrEmpty(accessToken))
        {
            logger.LogWarning("Google token response contained an empty access_token");
            return null;
        }

        try
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://games.googleapis.com/games/v1/players/me");
            requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var playerResponse = await httpClient.SendAsync(requestMessage, ct);
            if (!playerResponse.IsSuccessStatusCode)
            {
                var errBody = await playerResponse.Content.ReadAsStringAsync(ct);
                logger.LogWarning("Failed to fetch player info from Play Games API. Status: {StatusCode}. Body: {Body}", playerResponse.StatusCode, errBody);
                return null;
            }

            var playerJson = await playerResponse.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
            if (!playerJson.TryGetProperty("playerId", out var playerIdElement))
            {
                logger.LogWarning("Play Games API response missing playerId");
                return null;
            }

            string playerId = playerIdElement.GetString() ?? "";
            string displayName = "Player";
            if (playerJson.TryGetProperty("displayName", out var nameElement))
            {
                displayName = nameElement.GetString() ?? "Player";
            }

            logger.LogInformation("Successfully verified player {PlayerId} via Play Games API", playerId);

            return new GooglePlayerInfo(
                ExternalId: playerId,
                DisplayName: displayName
            );
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Exception while verifying player via Play Games API");
            return null;
        }
    }
}
