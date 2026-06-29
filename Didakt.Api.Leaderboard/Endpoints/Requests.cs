namespace Didakt.Api.Leaderboard.Endpoints
{
    namespace Requests
    {
        public record PostScoreRequest(string? Player, double? Score);
        public record GetScoreRequest(string? Player);
        public record GetTopRequest(long? Count);
    }
}