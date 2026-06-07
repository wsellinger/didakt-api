namespace Didakt.Api.Leaderboard.Models
{
    namespace Responses
    {
        internal record GetTopPlayersResponse(int Rank, string Player, double Score);
    }
}