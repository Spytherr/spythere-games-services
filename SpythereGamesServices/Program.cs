using System.Threading.RateLimiting;
using SpythereGamesServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHttpClient(); // Wymagane przez GoogleAuthService

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 60,
                Window = TimeSpan.FromMinutes(1)
            }));
});

// CORS — wymagane dla React SPA (portfolio) na innej domenie
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:3000",    // React dev (CRA)
            "http://localhost:5173"     // React dev (Vite)
            // Dodaj tu produkcyjny URL gdy zdeployujesz React, np.:
            // "https://spyther.dev"
        )
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

var connectionString = builder.Configuration.GetConnectionString("SpythereGamesServicesDatabase");
builder.SpythereGamesServicesDataExtensions(connectionString);

builder.Services.AddScoped<IPlayerService, PlayerService>();
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();
builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseRateLimiter();
app.UseMiddleware<ApiKeyMiddleware>();

app.MapScoresEndpoints();
app.MapPlayersEndpoints();
app.MapGamesEndpoints();
app.MapHealthEndpoints();

app.MigrateDatabase();

app.Run();
