using StackExchange.Redis;

//=== Setup

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
app.MapPost("/leaderboard/{game}/score", PostScore);

//Run
app.Run();

//=== Endpoints

//Post Score
static async Task<IResult> PostScore(string game, ScoreEntry entry, IConnectionMultiplexer redis)
{
    var db = redis.GetDatabase();
    await db.SortedSetAddAsync($"leaderboard:{game}", entry.Player, entry.Score);
    return Results.Ok();
}

//=== Definitions

record ScoreEntry(string Player, double Score);