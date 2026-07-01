using Didakt.Api.Auth.Data;
using Didakt.Api.Auth.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Time.Testing;
using Moq;
using System.Security.Cryptography;
using System.Text;

namespace Didakt.Api.Auth.UnitTests.Services
{
    public class LogoutTests
    {
        private readonly AuthDbContext _context;
        private readonly Mock<IPasswordHasher<User>> _hasher = new();
        private readonly Mock<IConfiguration> _config = new();
        private readonly FakeTimeProvider _timeProvider = new(DateTimeOffset.UtcNow);
        private readonly AuthService _service;

        public LogoutTests()
        {
            var options = new DbContextOptionsBuilder<AuthDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AuthDbContext(options);
            _service = new AuthService(_context, _hasher.Object, _config.Object, _timeProvider);
        }

        private static string HashToken(string token) =>
            Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

        [Fact]
        public async Task ValidToken_ReturnsTrue_RevokesToken()
        {
            //Arrange
            var user = new User { Id = 1, Name = "testUser", PasswordHash = "hash" };
            await _context.Users.AddAsync(user);

            var rawToken = "validToken";
            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                TokenHash = HashToken(rawToken),
                ExpiresAt = _timeProvider.GetUtcNow().UtcDateTime.AddDays(1)
            };
            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            //Act
            var result = await _service.LogoutAsync(rawToken);

            //Assert
            Assert.True(result);

            var tokenInDb = await _context.RefreshTokens.FirstAsync(rt => rt.TokenHash == HashToken(rawToken));
            Assert.NotNull(tokenInDb.RevokedAt);
        }

        [Fact]
        public async Task UnknownToken_ReturnsFalse()
        {
            //Act
            var result = await _service.LogoutAsync("nonExistentToken");

            //Assert
            Assert.False(result);
        }

        [Fact]
        public async Task AlreadyRevokedToken_ReturnsFalse()
        {
            //Arrange
            var user = new User { Id = 1, Name = "testUser", PasswordHash = "hash" };
            await _context.Users.AddAsync(user);

            var rawToken = "revokedToken";
            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                TokenHash = HashToken(rawToken),
                ExpiresAt = _timeProvider.GetUtcNow().UtcDateTime.AddDays(1),
                RevokedAt = _timeProvider.GetUtcNow().UtcDateTime.AddMinutes(-5)
            };
            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            //Act
            var result = await _service.LogoutAsync(rawToken);

            //Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExpiredToken_ReturnsFalse()
        {
            //Arrange
            var user = new User { Id = 1, Name = "testUser", PasswordHash = "hash" };
            await _context.Users.AddAsync(user);

            var rawToken = "expiredToken";
            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                TokenHash = HashToken(rawToken),
                ExpiresAt = _timeProvider.GetUtcNow().UtcDateTime.AddDays(-1)
            };
            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            //Act
            var result = await _service.LogoutAsync(rawToken);

            //Assert
            Assert.False(result);
        }
    }
}