using SpythereGamesServices;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

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


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

app.MapScoresEndpoints();
app.MapPlayersEndpoints();
app.MapHealthEndpoints();

app.MigrateDatabase();


app.Run();

