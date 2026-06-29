using Didakt.Api.Auth.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Testcontainers.PostgreSql;

namespace Didakt.Api.Auth.IntegrationTests;

public class RegisterTests : IAsyncLifetime
{
    private const string RegisterRequestUri = "/auth/register";

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
    public async Task ValidRequest_Created()
    {
        //Arrange
        var requestBody = new { username = "testUser", password = "testpassword" };

        //Act
        var response = await _client.PostAsJsonAsync(RegisterRequestUri, requestBody);

        //Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task DuplicateUsername_Conflict()
    {
        //Arrange
        var requestBody = new { username = "testUser", password = "testpassword" };
        _ = await _client.PostAsJsonAsync(RegisterRequestUri, requestBody);

        //Act
        var response = await _client.PostAsJsonAsync(RegisterRequestUri, requestBody);

        //Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
