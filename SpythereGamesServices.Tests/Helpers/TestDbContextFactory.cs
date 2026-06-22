using Microsoft.EntityFrameworkCore;
using SpythereGamesServices;

namespace SpythereGamesServices.Tests.Helpers;

public static class TestDbContextFactory
{
    public static SpythereGamesServicesContext Create(string? dbName = null)
    {
        dbName ??= Guid.NewGuid().ToString();

        var options = new DbContextOptionsBuilder<SpythereGamesServicesContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var context = new SpythereGamesServicesContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
