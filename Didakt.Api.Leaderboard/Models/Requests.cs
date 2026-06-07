namespace Didakt.Api.Leaderboard.Models
{
    namespace Requests
    {
        internal record PostScoreRequest(string Player, double Score);
    }
}