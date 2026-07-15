using Microsoft.EntityFrameworkCore;

namespace SpythereGamesServices;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this WebApplication app)
    {
        var handler = async (SpythereGamesServicesContext context, ILogger<SpythereGamesServicesContext> logger, CancellationToken ct) =>
        {
            try
            {
                // To keep Supabase instance awake, execute a lightweight query.
                await context.Database.ExecuteSqlRawAsync("SELECT 1", ct);
                return Results.Ok(new { Status = "Healthy" });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Health check failed — database is unreachable");
                return Results.Json(new { Status = "Unhealthy", Message = "Database is unreachable" }, statusCode: 503);
            }
        };

        app.MapGet("/api/health", handler).WithName("HealthCheckGet");
        app.MapMethods("/api/health", new[] { "HEAD" }, handler).WithName("HealthCheckHead");
    }
}
