using Didakt.Api.Auth.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Didakt.Api.Auth.IntegrationTests
{
    public abstract class TestBase : IAsyncLifetime
    {
        protected readonly PostgreSqlContainer Postgres = new PostgreSqlBuilder("postgres:17").Build();
        protected WebApplicationFactory<Program> Factory = null!;
        protected HttpClient Client = null!;

        public async Task InitializeAsync()
        {
            await Postgres.StartAsync();

            Factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
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
                        options.UseNpgsql(Postgres.GetConnectionString());
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

            Client = Factory.CreateClient();

            using var scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            await db.Database.MigrateAsync();
        }

        public async Task DisposeAsync()
        {
            await Postgres.DisposeAsync();
            await Factory.DisposeAsync();
        }
    }
}