using Didakt.Api.Auth.Data;
using Didakt.Api.Auth.Data.Models;
using Didakt.Api.Auth.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

internal class AuthService(AuthDbContext context, IPasswordHasher<User> hasher, IConfiguration configuration) : IAuthService
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

    public async Task<string?> LoginAsync(string username, string password)
    {
        //Get User
        var user = await context.Users.FirstOrDefaultAsync(u => u.Name == username);
        if (user == null) 
            return null;

        //Check Password
        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (result == PasswordVerificationResult.Failed) 
            return null;

        //Generate Token
        return GenerateToken(user, configuration);

        //Local Functions

        static string GenerateToken(User user, IConfiguration config) => 
            new JwtSecurityTokenHandler().WriteToken(
                new JwtSecurityToken(
                    issuer: config["Jwt:Issuer"],
                    audience: config["Jwt:Audience"],
                    claims: [
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.Name)],
                    expires: DateTime.UtcNow.AddMinutes(double.Parse(config["Jwt:ExpiryMinutes"]!)),
                    signingCredentials: new SigningCredentials(
                        key: new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Secret"]!)),
                        algorithm: SecurityAlgorithms.HmacSha256)));
    }
}