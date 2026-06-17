using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace SpythereGamesServices;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this WebApplication app)
    {
        var handler = async (SpythereGamesServicesContext context) =>
        {
            try
            {
                // To keep Supabase instance awake, execute a lightweight query.
                await context.Database.ExecuteSqlRawAsync("SELECT 1");
                return Results.Ok(new { Status = "Healthy" });
            }
            catch
            {
                return Results.StatusCode(503);
            }
        };

        app.MapGet("/api/health", handler).WithName("HealthCheckGet");
        app.MapMethods("/api/health", new[] { "HEAD" }, handler).WithName("HealthCheckHead");
    }
}
