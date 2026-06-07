using FluentValidation;
using Scalar.AspNetCore;

namespace Didakt.Api.Auth;

internal static class Extensions
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
