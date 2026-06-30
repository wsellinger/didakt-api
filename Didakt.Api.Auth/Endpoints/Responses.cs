namespace Didakt.Api.Auth.Endpoints
{
    namespace Responses
    {
        internal record LoginResponse(string AccessToken, string RefreshToken);
    }
}