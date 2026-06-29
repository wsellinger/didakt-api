using Didakt.Api.Auth.Data;
using Didakt.Api.Auth.Endpoints.Responses;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Testcontainers.PostgreSql;

namespace Didakt.Api.Auth.IntegrationTests
{
    public class LoginTests : IAsyncLifetime
    {
        private const string RegisterRequestUri = "/auth/register";
        private const string LoginRequestUri = "/auth/login";

        private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17").Build();
        private HttpClient _client = null!; 
        private WebApplicationFactory<Program> _factory = null!;

        public async Task InitializeAsync()
        {
            await _postgres.StartAsync();
            
            _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
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
                        options.UseNpgsql(_postgres.GetConnectionString());
                        options.UseSnakeCaseNamingConvention();
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
            });
            _client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            await db.Database.MigrateAsync();
        }

        public async Task DisposeAsync()
        {
            await _postgres.DisposeAsync();
            await _factory.DisposeAsync();
        }

        [Fact]
        public async Task ValidRequest_OK_HasToken()
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
        public async Task BadCredentials_Unauthorized()
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
