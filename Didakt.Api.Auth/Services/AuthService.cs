using Didakt.Api.Auth.Data;
using Didakt.Api.Auth.Data.Models;
using Didakt.Api.Auth.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

internal class AuthService(AuthDbContext context, IPasswordHasher<User> hasher) : IAuthService
{
    public async Task<bool> RegisterAsync(string username, string password)
    {
        //Check if exists
        if (await context.Users.AnyAsync(u => u.Name == username))
            return false;

        //Create New User
        var user = new User { Name = username };
        user.PasswordHash = hasher.HashPassword(user, password);

        //Insert New User
        context.Users.Add(user);
        await context.SaveChangesAsync();

        return true;
    }
}