using Didakt.Api.Auth.Endpoints;
using Didakt.Api.Auth.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddServices(builder.Configuration);

var app = builder.Build();
app.ConfigureApp();
app.MapEndpoints();
app.Run();