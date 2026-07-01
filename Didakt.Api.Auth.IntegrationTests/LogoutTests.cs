using Didakt.Api.Auth.Data;
using Didakt.Api.Auth.Endpoints.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;

namespace Didakt.Api.Auth.IntegrationTests
{
    public class LogoutTests : TestBase
    {
        private const string RegisterRequestUri = "/auth/register";
        private const string LoginRequestUri = "/auth/login";
        private const string LogoutRequestUri = "/auth/logout";
        private const string RenewRequestUri = "/auth/renew";

        [Fact]
        public async Task ValidRequest_Ok()
        {
            //Arrange
            var credentials = new { username = "testUser", password = "testPassword" };
            await Client.PostAsJsonAsync(RegisterRequestUri, credentials);
            var loginResponse = await Client.PostAsJsonAsync(LoginRequestUri, credentials);
            var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

            Client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", loginBody!.AccessToken);

            var logoutBody = new { refreshToken = loginBody.RefreshToken };

            //Act
            var response = await Client.PostAsJsonAsync(LogoutRequestUri, logoutBody);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ValidRequest_RevokesToken()
        {
            //Arrange
            var credentials = new { username = "testUser", password = "testPassword" };
            await Client.PostAsJsonAsync(RegisterRequestUri, credentials);
            var loginResponse = await Client.PostAsJsonAsync(LoginRequestUri, credentials);
            var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

            Client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", loginBody!.AccessToken);

            var body = new { refreshToken = loginBody.RefreshToken };
            await Client.PostAsJsonAsync(LogoutRequestUri, body);

            //Act
            var renewResponse = await Client.PostAsJsonAsync(RenewRequestUri, body);

            //Assert
            Assert.Equal(HttpStatusCode.Unauthorized, renewResponse.StatusCode);
        }

        [Fact]
        public async Task NoAccessToken_Unauthorized()
        {
            //Arrange
            var logoutBody = new { refreshToken = "someToken" };

            //Act
            var response = await Client.PostAsJsonAsync(LogoutRequestUri, logoutBody);

            //Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task UnknownRefreshToken_NotFound()
        {
            //Arrange
            var credentials = new { username = "testUser", password = "testPassword" };
            await Client.PostAsJsonAsync(RegisterRequestUri, credentials);
            var loginResponse = await Client.PostAsJsonAsync(LoginRequestUri, credentials);
            var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

            Client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", loginBody!.AccessToken);

            var logoutBody = new { refreshToken = "not-a-real-token" };

            //Act
            var response = await Client.PostAsJsonAsync(LogoutRequestUri, logoutBody);

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task ExpiredRefreshToken_NotFound()
        {
            //Arrange
            var credentials = new { username = "testUser", password = "testPassword" };
            await Client.PostAsJsonAsync(RegisterRequestUri, credentials);
            var loginResponse = await Client.PostAsJsonAsync(LoginRequestUri, credentials);
            var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

            Client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", loginBody!.AccessToken);

            //Manually expire the token in the DB
            using var scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            var tokenHash = Convert.ToBase64String(
                SHA256.HashData(Encoding.UTF8.GetBytes(loginBody.RefreshToken)));
            var refreshToken = await db.RefreshTokens.FirstAsync(rt => rt.TokenHash == tokenHash);
            refreshToken.ExpiresAt = DateTime.UtcNow.AddDays(-1);
            await db.SaveChangesAsync();

            var logoutBody = new { refreshToken = loginBody.RefreshToken };

            //Act
            var response = await Client.PostAsJsonAsync(LogoutRequestUri, logoutBody);

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}