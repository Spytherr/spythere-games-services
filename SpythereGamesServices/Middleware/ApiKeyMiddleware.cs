using System.Security.Cryptography;
using System.Text;

namespace SpythereGamesServices;

public class ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
{
    private const string API_KEY_HEADER = "X-Api-Key";

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip GET and HEAD requests (public for website)
        if (context.Request.Method == "GET" || context.Request.Method == "HEAD")
        {
            await next(context);
            return;
        }

        // Skip health endpoint
        if (context.Request.Path.StartsWithSegments("/api/health"))
        {
            await next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(API_KEY_HEADER, out var providedKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { Message = "API Key is required" });
            return;
        }

        var expectedKey = configuration["ApiSettings:ApiKey"];
        if (string.IsNullOrEmpty(expectedKey) || !FixedTimeEquals(providedKey!, expectedKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { Message = "Invalid API Key" });
            return;
        }

        await next(context);
    }

    private static bool FixedTimeEquals(string a, string b)
    {
        var bytesA = Encoding.UTF8.GetBytes(a);
        var bytesB = Encoding.UTF8.GetBytes(b);
        return CryptographicOperations.FixedTimeEquals(bytesA, bytesB);
    }
}
