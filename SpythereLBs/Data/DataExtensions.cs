

using Microsoft.EntityFrameworkCore;

namespace SpythereLBs;

public static class DataExtensions
{
    public static void MigrateDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SpythereLBsContext>();
        dbContext.Database.Migrate();
    }
    public static void SpythereLBsDataExtensions(this WebApplicationBuilder builder, string? connectionString)
    {
        builder.Services.AddDbContext<SpythereLBsContext>(options =>
            options.UseNpgsql(connectionString));
    }

}
