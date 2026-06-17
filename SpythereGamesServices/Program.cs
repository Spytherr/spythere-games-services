using SpythereGamesServices;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

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

app.MapScoresEndpoints();
app.MapPlayersEndpoints();
app.MapHealthEndpoints();

app.MigrateDatabase();


app.Run();
