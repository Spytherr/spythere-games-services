using SpythereGamesServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHttpClient();


builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = null; 
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:3000",    
            "http://localhost:5173",
            "https://spythere-games.vercel.app"
            
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

app.UseCors("AllowFrontend");
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<ApiKeyMiddleware>();

app.MapScoresEndpoints();
app.MapPlayersEndpoints();
app.MapGamesEndpoints();
app.MapHealthEndpoints();

app.MigrateDatabase();

app.Run();
