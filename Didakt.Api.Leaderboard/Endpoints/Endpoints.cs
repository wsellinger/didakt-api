using Didakt.Api.Leaderboard.Models.Requests;
using Didakt.Api.Leaderboard.Models.Responses;
using StackExchange.Redis;

namespace Didakt.Api.Leaderboard.Endpoints;

internal static class Endpoints
{
    //=== Mappings

    extension(WebApplication app)
    {
        internal WebApplication MapEndpoints()
        {
            var group = app.MapGroup("/leaderboard/{game}/");
            group.MapPost("score", EndpointMethods.PostScore).RequireAuthorization();
            group.MapGet("score", EndpointMethods.GetScore);
            group.MapGet("top", EndpointMethods.GetTopPlayers);

            return app;
        }
    }
}
internal static class EndpointMethods
{
    //=== Endpoints

    const string KeyBase = "leaderboard:";

    //Post Score

    internal static async Task<IResult> PostScore(string game, PostScoreRequest entry, IConnectionMultiplexer redis)
    {
        var db = redis.GetDatabase();
        await db.SortedSetAddAsync($"{KeyBase}{game}", entry.Player, entry.Score);
        return Results.Ok();
    }

    //Get Score
    internal static async Task<double> GetScore(string game, string player, IConnectionMultiplexer redis)
    {
        var db = redis.GetDatabase();
        double? score = await db.SortedSetScoreAsync($"{KeyBase}{game}", player);
        return score ?? 0;
    }

    //Get Top Players

    internal static async Task<GetTopPlayersResponse[]> GetTopPlayers(string game, long count, IConnectionMultiplexer redis)
    {
        var db = redis.GetDatabase();
        SortedSetEntry[] entries = await db.SortedSetRangeByRankWithScoresAsync($"{KeyBase}{game}", 0, count - 1, Order.Descending);
        var rank = 0;
        return [.. entries.Select(x => new GetTopPlayersResponse(++rank, x.Element.ToString(), x.Score))];
    }
}