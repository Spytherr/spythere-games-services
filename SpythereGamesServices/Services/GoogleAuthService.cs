using System.Text.Json;
using Google.Apis.Auth;

namespace SpythereGamesServices;

public class GoogleAuthService(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<GoogleAuthService> logger) : IGoogleAuthService
{
    public async Task<GooglePlayerInfo?> VerifyAuthCodeAsync(string authCode)
    {
        var clientId = configuration["GoogleAuth:ClientId"];
        var clientSecret = configuration["GoogleAuth:ClientSecret"];

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            throw new InvalidOperationException("GoogleAuth:ClientId or GoogleAuth:ClientSecret is not configured");
        }

        var httpClient = httpClientFactory.CreateClient();

        // Wymień auth code na tokeny przez Google OAuth2
        var tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["code"] = authCode,
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["redirect_uri"] = "",
            ["grant_type"] = "authorization_code"
        });

        var response = await httpClient.PostAsync("https://oauth2.googleapis.com/token", tokenRequest);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Google token exchange failed with status {StatusCode}", response.StatusCode);
            return null;
        }

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        if (!json.TryGetProperty("id_token", out var idTokenElement))
        {
            logger.LogWarning("Google token response did not contain an id_token");
            return null;
        }

        var idToken = idTokenElement.GetString();
        if (string.IsNullOrEmpty(idToken))
        {
            logger.LogWarning("Google token response contained an empty id_token");
            return null;
        }

        try
        {
            // Zweryfikuj ID token kryptograficznie
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { clientId }
                });

            return new GooglePlayerInfo(
                ExternalId: payload.Subject,
                DisplayName: payload.Name ?? "Player"
            );
        }
        catch (InvalidJwtException ex)
        {
            logger.LogWarning(ex, "Google ID token validation failed");
            return null;
        }
    }
}
