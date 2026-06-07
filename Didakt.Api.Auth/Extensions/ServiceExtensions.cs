using Didakt.Api.Auth.Data;
using Didakt.Api.Auth.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Didakt.Api.Auth.Extensions;

internal static class ServiceExtensions
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection AddServices(IConfiguration configuration)
        {
            services.AddOpenApi();
            services.AddValidatorsFromAssemblyContaining<Program>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddDbContext<AuthDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("Postgres"));
                options.UseSnakeCaseNamingConvention();
            });

            return services;
        }
    }
}
