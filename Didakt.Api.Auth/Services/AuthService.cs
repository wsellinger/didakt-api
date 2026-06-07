using Didakt.Api.Auth.Data;
using Didakt.Api.Auth.Services;

internal class AuthService(AuthDbContext context) : IAuthService
{
    public Task<bool> RegisterAsync(string username, string password)
    {
        throw new NotImplementedException();
    }
}