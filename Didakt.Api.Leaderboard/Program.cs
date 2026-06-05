using Didakt.Api.Leaderboard.Extensions;
using Scalar.AspNetCore;
using StackExchange.Redis;

//=== Constants

const string KeyBase = "leaderboard:";

//=== Setup

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddServices(builder.Configuration);

//Build App
var app = builder.Build();

//Configure App
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

//Map Requests
app.MapPost("/leaderboard/{game}/score", PostScore);
app.MapGet("/leaderboard/{game}/score", GetScore);
app.MapGet("/leaderboard/{game}/top", GetTopPlayers);

//Run
app.Run();

//=== Endpoints

//Post Score
static async Task<IResult> PostScore(string game, ScoreEntry entry, IConnectionMultiplexer redis)
{
    var db = redis.GetDatabase();
    await db.SortedSetAddAsync($"{KeyBase}{game}", entry.Player, entry.Score);
    return Results.Ok();
}

//Get Score
static async Task<double> GetScore(string game, string player, IConnectionMultiplexer redis)
{
    var db = redis.GetDatabase();
    double? score = await db.SortedSetScoreAsync($"{KeyBase}{game}", player);
    return score ?? 0;
}

//Get Top Players
static async Task<PlayerScore[]> GetTopPlayers(string game, long count, IConnectionMultiplexer redis)
{
    var db = redis.GetDatabase();
    SortedSetEntry[] entries = await db.SortedSetRangeByRankWithScoresAsync($"{KeyBase}{game}", 0, count - 1, Order.Descending);
    var rank = 0;
    return [.. entries.Select(x => new PlayerScore(++rank, x.Element.ToString(), x.Score))];
}

//=== Data Structures

//Input

record ScoreEntry(string Player, double Score);

//Output

record PlayerScore(int Rank, string Player, double Score);