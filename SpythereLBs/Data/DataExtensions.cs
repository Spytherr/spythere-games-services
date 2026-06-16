

using Microsoft.EntityFrameworkCore;

namespace SpythereGamesServices;

public static class DataExtensions
{
    public static void MigrateDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SpythereGamesServicesContext>();
        dbContext.Database.Migrate();
    }
    public static void SpythereGamesServicesDataExtensions(this WebApplicationBuilder builder, string? connectionString)
    {
        builder.Services.AddDbContext<SpythereGamesServicesContext>(options =>
            options.UseNpgsql(connectionString));
    }

}
