using Scalar.AspNetCore;
using StackExchange.Redis;

namespace Didakt.Api.Leaderboard;

internal static class Extensions
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection AddServices(IConfiguration configuration)
        {
            services.AddOpenApi();
            services.AddSingleton<IConnectionMultiplexer>(GetConnection(configuration));

            return services;

            static ConnectionMultiplexer GetConnection(IConfiguration configuration) 
                => ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")!);
        }
    }

    extension(WebApplication app)
    {
        internal WebApplication ConfigureApp()
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
