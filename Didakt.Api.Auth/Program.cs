using Scalar.AspNetCore;

//Build App
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
var app = builder.Build();

//Configure App
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
app.MapPost("/auth/register", PostRegister);

//Run
app.Run();

//=== Endpoints

//Register Player
static async Task<IResult> PostRegister()
{
    return Results.Ok();
}