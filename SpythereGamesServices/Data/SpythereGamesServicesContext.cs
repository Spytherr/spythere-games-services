using System;
using Microsoft.EntityFrameworkCore;

namespace SpythereGamesServices;

public class SpythereGamesServicesContext(DbContextOptions<SpythereGamesServicesContext> options) : DbContext(options)
{
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Game> Games => Set<Game>();
    public DbSet<Score> Scores => Set<Score>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Player>()
            .HasMany(p => p.Scores)
            .WithOne()
            .HasForeignKey(s => s.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Player>()
            .HasIndex(p => new { p.Platform, p.ExternalId })
            .IsUnique();

        modelBuilder.Entity<Game>()
            .HasMany(g => g.Scores)
            .WithOne()
            .HasForeignKey(s => s.GameId);

        modelBuilder.Entity<Score>()
            .HasIndex(s => new { s.PlayerId, s.GameId })
            .IsUnique();

        modelBuilder.Entity<Game>()
            .HasIndex(g => g.Key)
            .IsUnique();

        modelBuilder.Entity<Game>().HasData(
            new Game
            {
                Id = 1,
                Key = "chess-vs-checkers",
                Name = "Chess vs Checkers",
                Description = "A unique blend of Chess and Checkers mechanics.",
                CreatedAt = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }

}
