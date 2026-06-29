namespace Didakt.Api.Auth.Endpoints
{
    namespace Requests
    {
        public record RegisterRequest(string? UserName, string? Password);
        public record LoginRequest(string? UserName, string? Password);
    }
}