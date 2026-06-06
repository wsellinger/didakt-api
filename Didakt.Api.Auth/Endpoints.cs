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
    internal static async Task<IResult> PostRegister()
    {
        return Results.Ok();
    }
}