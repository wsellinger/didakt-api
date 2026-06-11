using Didakt.Api.Leaderboard.Endpoints;
using Didakt.Api.Leaderboard.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddServices(builder.Configuration);

var app = builder.Build();
app.ConfigureApp();
app.MapEndpoints();
app.Run();