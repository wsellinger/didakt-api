using StackExchange.Redis;

namespace Didakt.Api.Leaderboard.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenApi();
        AddRedis(services, configuration);

        return services;

        static void AddRedis(IServiceCollection services, IConfiguration configuration)
        {
            var connString = configuration.GetConnectionString("Redis")!;
            var connection = ConnectionMultiplexer.Connect(connString);
            services.AddSingleton<IConnectionMultiplexer>(connection);
        }
    }
}