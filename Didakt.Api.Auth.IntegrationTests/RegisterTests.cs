using Didakt.Api.Auth.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace Didakt.Api.Auth.IntegrationTests;

public class RegisterTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string RegisterRequestUri = "/auth/register";

    private readonly HttpClient _client;

    public RegisterTests(WebApplicationFactory<Program> factory)
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
