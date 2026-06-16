

using Microsoft.EntityFrameworkCore;

namespace SpythereLBs;

public static class DataExtensions
{
    public static void SpythereLBsDataExtensions(this WebApplicationBuilder builder, string? connectionString)
    {
        builder.Services.AddDbContext<SpythereLBsContext>(options =>
            options.UseNpgsql(connectionString));
    }

}
