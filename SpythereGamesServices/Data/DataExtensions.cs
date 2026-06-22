using Microsoft.EntityFrameworkCore;

namespace SpythereGamesServices;

public static class DataExtensions
{
    public static void MigrateDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SpythereGamesServicesContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<SpythereGamesServicesContext>>();

        try
        {
            dbContext.Database.Migrate();
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Database migration failed — the application may not function correctly");
            throw;
        }
    }

    public static void SpythereGamesServicesDataExtensions(this WebApplicationBuilder builder, string? connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'SpythereGamesServicesDatabase' is not configured. " +
                "Set it in appsettings.json or via the ConnectionStrings__SpythereGamesServicesDatabase environment variable.");
        }

        builder.Services.AddDbContext<SpythereGamesServicesContext>(options =>
            options.UseNpgsql(connectionString));
    }
}
