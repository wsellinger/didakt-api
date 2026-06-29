namespace Didakt.Api.Leaderboard.Endpoints
{
    namespace Responses
    {
        internal record GetScoreResponse(double Score);
        internal record GetTopResponse(int Rank, string Player, double Score);
    }
}