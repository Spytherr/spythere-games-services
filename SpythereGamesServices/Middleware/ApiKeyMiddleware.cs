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
            await context.Response.WriteAsJsonAsync(new MessageResponse("API Key is required"));
            return;
        }

        var expectedKey = configuration["ApiSettings:ApiKey"];
        if (string.IsNullOrEmpty(expectedKey) || !string.Equals(providedKey, expectedKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new MessageResponse("Invalid API Key"));
            return;
        }

        await next(context);
    }
}
