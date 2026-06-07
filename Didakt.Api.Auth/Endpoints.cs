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

    internal static async Task<IResult> PostRegister(RegisterRequest request)
    {
        return Results.Ok();
    }
}