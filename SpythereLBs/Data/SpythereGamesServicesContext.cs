using System;
using Microsoft.EntityFrameworkCore;

namespace SpythereGamesServices;

public class SpythereGamesServicesContext(DbContextOptions<SpythereGamesServicesContext> options) : DbContext(options)
{
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Game> Games => Set<Game>();
    public DbSet<Scores> Scores => Set<Scores>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Player>()
            .HasMany(p => p.Scores)
            .WithOne()
            .HasForeignKey(s => s.PlayerId);

        modelBuilder.Entity<Game>()
            .HasMany(g => g.Scores)
            .WithOne()
            .HasForeignKey(s => s.GameId);
    }

}
