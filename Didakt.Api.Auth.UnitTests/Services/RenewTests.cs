using Didakt.Api.Auth.Data;
using Didakt.Api.Auth.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Time.Testing;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Didakt.Api.Auth.UnitTests.Services
{
    public class RenewTests
    {
        private const int RefreshExpiryDays = 7;

        private readonly AuthDbContext _context;
        private readonly Mock<IPasswordHasher<User>> _hasher = new();
        private readonly Mock<IConfiguration> _config = new();
        private readonly FakeTimeProvider _timeProvider = new(DateTimeOffset.UtcNow);
        private readonly AuthService _service;

        public RenewTests()
        {
            var options = new DbContextOptionsBuilder<AuthDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AuthDbContext(options);
            _service = new AuthService(_context, _hasher.Object, _config.Object, _timeProvider);

            _config.Setup(c => c["Jwt:Secret"]).Returns("testSecret-123456789012345678901234567890");
            _config.Setup(c => c["Jwt:AccessExpiryMinutes"]).Returns("15");
            _config.Setup(c => c["Jwt:RefreshExpiryDays"]).Returns(RefreshExpiryDays.ToString());
            _config.Setup(c => c["Jwt:Issuer"]).Returns("didakt-api");
            _config.Setup(c => c["Jwt:Audience"]).Returns("didakt-client");
        }

        private static string HashToken(string token) =>
            Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

        [Fact]
        public async Task ValidToken_ReturnsNewTokens()
        {
            //Arrange
            var userName = "testUser";
            var user = new User { Id = 1, Name = userName, PasswordHash = "hash" };
            await _context.Users.AddAsync(user);

            var rawRefreshToken = "validRefreshToken";
            var existingRefreshToken = new RefreshToken
            {
                UserId = user.Id,
                TokenHash = HashToken(rawRefreshToken),
                ExpiresAt = _timeProvider.GetUtcNow().UtcDateTime.AddDays(1)
            };
            await _context.RefreshTokens.AddAsync(existingRefreshToken);
            await _context.SaveChangesAsync();

            var tokenHandler = new JwtSecurityTokenHandler();

            //Act
            var result = await _service.RenewAsync(rawRefreshToken);

            //Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.AccessToken);
            Assert.NotEmpty(result.RefreshToken);
            Assert.NotEqual(rawRefreshToken, result.RefreshToken);

            var accessToken = tokenHandler.ReadJwtToken(result.AccessToken);
            var nameClaim = accessToken.Claims.First(c => c.Type == ClaimTypes.Name);
            Assert.Equal(userName, nameClaim.Value);

            //Old token should be revoked
            var oldTokenInDb = await _context.RefreshTokens
                .FirstAsync(rt => rt.TokenHash == HashToken(rawRefreshToken));
            Assert.NotNull(oldTokenInDb.RevokedAt);

            //New token should exist
            var newTokenInDb = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.TokenHash == HashToken(result.RefreshToken));
            Assert.NotNull(newTokenInDb);
            Assert.Null(newTokenInDb.RevokedAt);
        }

        [Fact]
        public async Task UnknownToken_ReturnsNull()
        {
            //Arrange
            var rawRefreshToken = "nonExistentToken";

            //Act
            var result = await _service.RenewAsync(rawRefreshToken);

            //Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ExpiredToken_ReturnsNull()
        {
            //Arrange
            var user = new User { Id = 1, Name = "testUser", PasswordHash = "hash" };
            await _context.Users.AddAsync(user);

            var rawRefreshToken = "expiredRefreshToken";
            var existingRefreshToken = new RefreshToken
            {
                UserId = user.Id,
                TokenHash = HashToken(rawRefreshToken),
                ExpiresAt = _timeProvider.GetUtcNow().UtcDateTime.AddDays(-1)
            };
            await _context.RefreshTokens.AddAsync(existingRefreshToken);
            await _context.SaveChangesAsync();

            //Act
            var result = await _service.RenewAsync(rawRefreshToken);

            //Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task RevokedToken_ReturnsNull()
        {
            //Arrange
            var user = new User { Id = 1, Name = "testUser", PasswordHash = "hash" };
            await _context.Users.AddAsync(user);

            var rawRefreshToken = "revokedRefreshToken";
            var existingRefreshToken = new RefreshToken
            {
                UserId = user.Id,
                TokenHash = HashToken(rawRefreshToken),
                ExpiresAt = _timeProvider.GetUtcNow().UtcDateTime.AddDays(1),
                RevokedAt = _timeProvider.GetUtcNow().UtcDateTime.AddMinutes(-5)
            };
            await _context.RefreshTokens.AddAsync(existingRefreshToken);
            await _context.SaveChangesAsync();

            //Act
            var result = await _service.RenewAsync(rawRefreshToken);

            //Assert
            Assert.Null(result);
        }
    }
}