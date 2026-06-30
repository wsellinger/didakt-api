using Didakt.Api.Auth.Data;
using Didakt.Api.Auth.Data.Models;
using Didakt.Api.Auth.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

internal class AuthService(
    AuthDbContext Context,
    IPasswordHasher<User> Hasher,
    IConfiguration Configuration,
    TimeProvider TimeProvider) : IAuthService
{
    private const int RefreshTokenSizeBytes = 64;

    public async Task<bool> RegisterAsync(string username, string password)
    {
        //Check if exists
        if (await Context.Users.AnyAsync(u => u.Name == username))
            return false;

        //Create New User
        var user = new User { Name = username };
        user.PasswordHash = Hasher.HashPassword(user, password);

        //Insert New User
        Context.Users.Add(user);
        var result = await Context.SaveChangesAsync();

        return true;
    }

    public async Task<LoginResult?> LoginAsync(string username, string password)
    {
        //Get User
        var user = await Context.Users.FirstOrDefaultAsync(u => u.Name == username);
        if (user == null) 
            return null;

        //Check Password
        var result = Hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (result == PasswordVerificationResult.Failed) 
            return null;

        //Generate Access Token
        var accessToken = GenerateAccessToken(user);

        //Generate Refresh Token
        var refreshToken = await GenerateRefreshTokenAsync(user.Id);

        //Return
        return new(accessToken, refreshToken);

        //Local Functions

        string GenerateAccessToken(User user) => 
            new JwtSecurityTokenHandler().WriteToken(
                new JwtSecurityToken(
                    issuer: Configuration["Jwt:Issuer"],
                    audience: Configuration["Jwt:Audience"],
                    claims: [
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.Name)],
                    expires: UtcDateTimeNow.AddMinutes(GetAccessExpiryMinutes()),
                    signingCredentials: new SigningCredentials(
                        key: new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Secret"]!)),
                        algorithm: SecurityAlgorithms.HmacSha256)));

        async Task<string> GenerateRefreshTokenAsync(int userId)
        {
            var tokenBytes = RandomNumberGenerator.GetBytes(RefreshTokenSizeBytes);
            var refreshToken = Convert.ToBase64String(tokenBytes);
            var tokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));

            Context.RefreshTokens.Add(new RefreshToken
            {
                UserId = userId,
                TokenHash = tokenHash,
                ExpiresAt = UtcDateTimeNow.AddDays(GetRefreshExpiryDays())
            });

            await Context.SaveChangesAsync();

            return refreshToken;

        }

        double GetAccessExpiryMinutes() => double.Parse(Configuration["Jwt:AccessExpiryMinutes"]!);
        int GetRefreshExpiryDays() => int.Parse(Configuration["Jwt:RefreshExpiryDays"]!);
    }

    private DateTime UtcDateTimeNow => TimeProvider.GetUtcNow().UtcDateTime;
}