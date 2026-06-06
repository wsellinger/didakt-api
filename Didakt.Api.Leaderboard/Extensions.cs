using Scalar.AspNetCore;
using StackExchange.Redis;

namespace Didakt.Api.Leaderboard;

public static class Extensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddServices(IConfiguration configuration)
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

    extension(WebApplication app)
    {
        public WebApplication ConfigureApp()
        {
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            app.UseHttpsRedirection();

            return app;
        }
    }
}
