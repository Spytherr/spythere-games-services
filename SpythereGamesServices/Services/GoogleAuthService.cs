using System.Text.Json;
using Google.Apis.Auth;

namespace SpythereGamesServices;

public class GoogleAuthService(IConfiguration configuration, IHttpClientFactory httpClientFactory) : IGoogleAuthService
{
    public async Task<GooglePlayerInfo?> VerifyAuthCodeAsync(string authCode)
    {
        var clientId = configuration["GoogleAuth:ClientId"];
        var clientSecret = configuration["GoogleAuth:ClientSecret"];

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            return null;
        }

        try
        {
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
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            
            if (!json.TryGetProperty("id_token", out var idTokenElement))
                return null;

            var idToken = idTokenElement.GetString();
            if (string.IsNullOrEmpty(idToken)) return null;

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
        catch (Exception ex)
        {
            Console.WriteLine($"[GoogleAuth] Verification failed: {ex.Message}");
            return null;
        }
    }
}
