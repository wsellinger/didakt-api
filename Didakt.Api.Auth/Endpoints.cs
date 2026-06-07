using Didakt.Api.Auth.Models.Requests;
using Didakt.Api.Auth.Services;
using FluentValidation;

namespace Didakt.Api.Auth;

internal static class Endpoints
{
    //=== Mappings

    extension(WebApplication app)
    {
        internal WebApplication MapEndpoints()
        {
            app.MapPost("/auth/register", PostRegister);
            return app;
        }
    }

    //=== Endpoints

    //Register Player

    internal static async Task<IResult> PostRegister(RegisterRequest request, IValidator<RegisterRequest> validator, IAuthService service)
    {
        //Validate
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        //Service
        var result = await service.RegisterAsync(request.UserName, request.Password);

        //Return
        return result ? Results.Created() : Results.Conflict();
    }
}