using Didakt.Api.Auth.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace Didakt.Api.Auth.IntegrationTests
{
    public class LoginIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private const string RegisterRequestUri = "/auth/register";
        private const string LoginRequestUri = "/auth/login";

        private readonly HttpClient _client;

        public LoginIntegrationTests(WebApplicationFactory<Program> factory)
        {
            string databaseName = Guid.NewGuid().ToString();

            _client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptors = services.Where(d =>
                        d.ServiceType == typeof(DbContextOptions<AuthDbContext>) ||
                        d.ServiceType == typeof(IDbContextOptionsConfiguration<AuthDbContext>))
                        .ToList();

                    foreach (var descriptor in descriptors)
                        services.Remove(descriptor);

                    services.AddDbContext<AuthDbContext>(options =>
                    {
                        options.UseInMemoryDatabase(databaseName);
                    });
                });

                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Jwt:Secret"] = "test-secret-key-min-32-chars-long!!",
                        ["Jwt:Issuer"] = "didakt-api",
                        ["Jwt:Audience"] = "didakt-client",
                        ["Jwt:ExpiryMinutes"] = "15"
                    });
                });
            }).CreateClient();
        }

        [Fact]
        public async Task Login_ValidRequest_OK_HasToken()
        {
            //Arrange
            var requestBody = new { username = "testUser", password = "testPassword" };
            await _client.PostAsJsonAsync(RegisterRequestUri, requestBody);

            //Act
            var response = await _client.PostAsJsonAsync(LoginRequestUri, requestBody);
            var body = await response.Content.ReadFromJsonAsync<LoginResponse>();

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(body?.Token);
        }

        [Fact]
        public async Task Login_BadCredentials_Unauthorized()
        {
            //Arrange
            var registerBody = new { username = "testUser", password = "testPassword" };
            var loginBody = new { username = "testUser", password = "invalidPassword" };
            await _client.PostAsJsonAsync(RegisterRequestUri, registerBody);

            //Act
            var response = await _client.PostAsJsonAsync(LoginRequestUri, loginBody);

            //Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
