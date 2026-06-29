using Didakt.Api.Auth.Data;
using Didakt.Api.Auth.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Didakt.Api.Auth.UnitTests.Services
{
    public class AuthServiceRegisterTests
    {
        private readonly AuthDbContext _context;
        private readonly Mock<IPasswordHasher<User>> _hasher;
        private readonly Mock<IConfiguration> _configuration;
        private readonly AuthService _service;

        public AuthServiceRegisterTests() 
        {
            var options = new DbContextOptionsBuilder<AuthDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AuthDbContext(options);
            _hasher = new Mock<IPasswordHasher<User>>();
            _configuration = new Mock<IConfiguration>();
            _service = new AuthService(_context, _hasher.Object, _configuration.Object);

            _hasher.Setup(x => x.HashPassword(It.IsAny<User>(), It.IsAny<string>()))
                .Returns("testHash");
        }

        [Fact]
        public async Task RegisterAsync_ValidUser_True()
        {
            //Arrange
            var userName = "testUser";
            var password = "testPassword";

            //Act
            var result = await _service.RegisterAsync(userName, password);

            //Assert
            _hasher.Verify(x => x.HashPassword(It.IsAny<User>(), password), Times.Once());

            Assert.True(result);
        }

        [Fact]
        public async Task RegisterAsync_UserExists_False()
        {
            //Arrange
            var userName = "testUser";
            var password = "testPassword";

            //Act
            await _service.RegisterAsync(userName, password);
            var result = await _service.RegisterAsync(userName, password);

            //Assert
            _hasher.Verify(x => x.HashPassword(It.IsAny<User>(), It.IsAny<string>()), Times.Once());

            Assert.False(result);
        }

        [Fact]
        public async Task RegisterAsync_ValidUser_SavesHashedPassword()
        {
            //Arrange
            var userName = "testUser";
            var password = "testPassword";
            var hash = "testHash";

            _hasher.Setup(x => x.HashPassword(It.IsAny<User>(), It.IsAny<string>()))
                .Returns(hash);

            //Act
            await _service.RegisterAsync(userName, password);
            var user = await _context.Users.FirstAsync(u => u.Name == userName);

            //Assert
            _hasher.Verify(x => x.HashPassword(It.IsAny<User>(), password), Times.Once());

            Assert.Equal(hash, user.PasswordHash);
        }
    }
}
