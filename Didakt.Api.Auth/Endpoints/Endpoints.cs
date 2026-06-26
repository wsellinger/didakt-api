using Didakt.Api.Auth.Endpoints.Requests;
using Didakt.Api.Auth.Endpoints.Responses;
using Didakt.Api.Auth.Services;
using FluentValidation;

namespace Didakt.Api.Auth.Endpoints;

internal static class EndpointExtensions
{
    //=== Mappings

    extension(WebApplication app)
    {
        internal WebApplication MapEndpoints()
        {
            app.MapPost("/auth/register", EndpointMethods.PostRegister);
            app.MapPost("/auth/login", EndpointMethods.PostLogin);
            return app;
        }
    }
}

internal static class EndpointMethods
{
    //Register

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

    //Login

    internal static async Task<IResult> PostLogin(LoginRequest request, IValidator<LoginRequest> validator, IAuthService service)
    {
        //Validate
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        //Service
        var result = await service.LoginAsync(request.UserName, request.Password);

        //Return
        return result is not null ? Results.Ok(new LoginResponse(result)) : Results.Unauthorized();
    }
}