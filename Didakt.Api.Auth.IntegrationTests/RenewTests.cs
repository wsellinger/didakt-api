using Didakt.Api.Auth.Data;
using Didakt.Api.Auth.Endpoints.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;

namespace Didakt.Api.Auth.IntegrationTests
{
    public class RenewTests : TestBase
    {
        private const string RegisterRequestUri = "/auth/register";
        private const string LoginRequestUri = "/auth/login";
        private const string RenewRequestUri = "/auth/renew";

        [Fact]
        public async Task ValidRefreshToken_ReturnsNewTokens()
        {
            //Arrange
            var credentials = new { username = "testUser", password = "testPassword" };
            await Client.PostAsJsonAsync(RegisterRequestUri, credentials);
            var loginResponse = await Client.PostAsJsonAsync(LoginRequestUri, credentials);
            var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

            var renewBody = new { refreshToken = loginBody!.RefreshToken };

            //Act
            var response = await Client.PostAsJsonAsync(RenewRequestUri, renewBody);
            var body = await response.Content.ReadFromJsonAsync<LoginResponse>();

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(body);
            Assert.NotEmpty(body.AccessToken);
            Assert.NotEmpty(body.RefreshToken);
            Assert.NotEqual(loginBody.RefreshToken, body.RefreshToken);
        }

        [Fact]
        public async Task ReusedRefreshToken_Unauthorized()
        {
            //Arrange
            var credentials = new { username = "testUser", password = "testPassword" };
            await Client.PostAsJsonAsync(RegisterRequestUri, credentials);
            var loginResponse = await Client.PostAsJsonAsync(LoginRequestUri, credentials);
            var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

            var renewBody = new { refreshToken = loginBody!.RefreshToken };
            await Client.PostAsJsonAsync(RenewRequestUri, renewBody);

            //Act
            var response = await Client.PostAsJsonAsync(RenewRequestUri, renewBody);

            //Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task InvalidRefreshToken_Unauthorized()
        {
            //Arrange
            var renewBody = new { refreshToken = "not-a-real-token" };

            //Act
            var response = await Client.PostAsJsonAsync(RenewRequestUri, renewBody);

            //Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task ExpiredRefreshToken_Unauthorized()
        {
            //Arrange
            var credentials = new { username = "testUser", password = "testPassword" };
            await Client.PostAsJsonAsync(RegisterRequestUri, credentials);
            var loginResponse = await Client.PostAsJsonAsync(LoginRequestUri, credentials);
            var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

            //Manually expire the token in the DB
            using var scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            var tokenHash = Convert.ToBase64String(
                SHA256.HashData(Encoding.UTF8.GetBytes(loginBody!.RefreshToken)));
            var refreshToken = await db.RefreshTokens.FirstAsync(rt => rt.TokenHash == tokenHash);
            refreshToken.ExpiresAt = DateTime.UtcNow.AddDays(-1);
            await db.SaveChangesAsync();

            var renewBody = new { refreshToken = loginBody.RefreshToken };

            //Act
            var response = await Client.PostAsJsonAsync(RenewRequestUri, renewBody);

            //Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}