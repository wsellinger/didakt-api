namespace Didakt.Api.Auth.Models
{
    namespace Requests
    {
        public record RegisterRequest(string UserName, string Password);
        public record LoginRequest(string UserName, string Password);
    }
}