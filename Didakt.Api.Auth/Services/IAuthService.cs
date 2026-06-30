namespace Didakt.Api.Auth.Services;

internal interface IAuthService
{
    Task<bool> RegisterAsync(string username, string password);
    Task<LoginResult?> LoginAsync(string username, string password);
}