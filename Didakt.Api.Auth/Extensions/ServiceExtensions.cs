using FluentValidation;

namespace Didakt.Api.Auth.Extensions;

internal static class ServiceExtensions
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection AddServices(IConfiguration configuration)
        {
            services.AddOpenApi();
            services.AddValidatorsFromAssemblyContaining<Program>();

            return services;
        }
    }
}
