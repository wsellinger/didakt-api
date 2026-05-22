using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//Add Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect("localhost:6379"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

//Map Requests
app.MapPost("/leaderboard/{game}/score", async (string game, ScoreEntry entry, IConnectionMultiplexer redis) =>
{
    var db = redis.GetDatabase();
    await db.SortedSetAddAsync($"leaderboard:{game}", entry.Player, entry.Score);
    return Results.Ok();
});

app.Run();

record ScoreEntry(string Player, double Score);