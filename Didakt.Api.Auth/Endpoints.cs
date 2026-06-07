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
    internal record RegisterRequest(string UserName, string Password);

    internal static async Task<IResult> PostRegister(RegisterRequest request, IValidator<RegisterRequest> validator)
    {
        //Validate
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        return Results.Ok();
    }
}