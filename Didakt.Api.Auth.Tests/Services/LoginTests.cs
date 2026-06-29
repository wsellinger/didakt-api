using Didakt.Api.Auth.Data;
using Didakt.Api.Auth.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Didakt.Api.Auth.UnitTests.Services
{
    public class LoginTests
    {
        private readonly AuthDbContext _context;
        private readonly Mock<IPasswordHasher<User>> _hasher;
        private readonly Mock<IConfiguration> _config;
        private readonly AuthService _service;

        public LoginTests() 
        {
            var options = new DbContextOptionsBuilder<AuthDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AuthDbContext(options);
            _hasher = new Mock<IPasswordHasher<User>>();
            _config = new Mock<IConfiguration>();
            _service = new AuthService(_context, _hasher.Object, _config.Object);

            _hasher.Setup(h => h.VerifyHashedPassword(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(PasswordVerificationResult.Success);

            _config.Setup(c => c["Jwt:Secret"]).Returns("testSecret-123456789012345678901234567890");
            _config.Setup(c => c["Jwt:ExpiryMinutes"]).Returns("15");
            _config.Setup(c => c["Jwt:Issuer"]).Returns("didakt-api");
            _config.Setup(c => c["Jwt:Audience"]).Returns("didakt-client");
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_Token()
        {
            //Arrange
            var userName = "testUser";
            var password = "testPassword";
            var hash = "hash";

            var user = new User() { Id = 1, Name = userName, PasswordHash = hash };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var tokenHandler = new JwtSecurityTokenHandler();

            //Act
            var result = await _service.LoginAsync(userName, password);

            //Assert
            _hasher.Verify(x => x.VerifyHashedPassword(It.IsAny<User>(), hash, password), Times.Once());

            Assert.NotNull(result);
            Assert.Equal(userName, tokenHandler.ReadJwtToken(result).Claims.First(c => c.Type == ClaimTypes.Name).Value);
        }

        [Fact]
        public async Task LoginAsync_InvalidUser_Null()
        {
            //Arrange
            var userName = "testUser";
            var password = "testPassword";

            //Act
            var result = await _service.LoginAsync(userName, password);

            //Assert
            _hasher.Verify(x => x.VerifyHashedPassword(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());

            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_InvalidPassword_Null()
        {
            //Arrange
            var userName = "testUser";
            var password = "testPassword";
            var hash = "testHash";

            var user = new User() { Id = 1, Name = userName, PasswordHash = hash };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            _hasher.Setup(x => x.VerifyHashedPassword(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(PasswordVerificationResult.Failed);

            //Act
            var result = await _service.LoginAsync(userName, password);

            //Assert
            _hasher.Verify(x => x.VerifyHashedPassword(It.IsAny<User>(), hash, password), Times.Once());

            Assert.Null(result);
        }
    }
}
