using SpythereLBs;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("SpythereLBsDatabase");
builder.SpythereLBsDataExtensions(connectionString);


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapChvChEndpoints();
app.MapPlayersEndpoints();


app.Run();
