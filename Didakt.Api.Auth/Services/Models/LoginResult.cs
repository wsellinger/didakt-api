namespace Didakt.Api.Auth.Services.Models
{
    internal record LoginResult(string AccessToken, string RefreshToken);
}