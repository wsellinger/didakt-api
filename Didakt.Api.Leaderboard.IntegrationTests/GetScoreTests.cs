using Didakt.Api.Leaderboard.Endpoints.Responses;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Testcontainers.Redis;

namespace Didakt.Api.Leaderboard.IntegrationTests
{
    public class GetScoreTests : IAsyncLifetime
    {
        private const string RequestUri = "/leaderboard/category/score";
        private const string JwtSecret = "test-secret-key-min-32-chars-long!!";
        private const string JwtIssuer = "didakt-api";
        private const string JwtAudience = "didakt-client";

        private readonly RedisContainer _redis = new RedisBuilder("redis:latest")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(6379))
            .Build();
        private WebApplicationFactory<Program> _factory = null!;
        private HttpClient _client = null!;

        public async Task InitializeAsync()
        {
            await _redis.StartAsync();

            _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Jwt:Secret"] = JwtSecret,
                        ["Jwt:Issuer"] = JwtIssuer,
                        ["Jwt:Audience"] = JwtAudience,
                        ["ConnectionStrings:Redis"] = _redis.GetConnectionString()
                    });
                });
            });

            _client = _factory.CreateClient();
        }

        public async Task DisposeAsync()
        {
            await _redis.DisposeAsync();
            await _factory.DisposeAsync();
        }

        private void SetBearerToken(string token)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        private static string GenerateTestToken()
        {
            // Generate a valid JWT using the same secret as the test config
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(JwtSecret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: JwtIssuer,
                audience: JwtAudience,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [Fact]
        public async Task ValidRequest_ReturnsOk()
        {
            //Arrange
            SetBearerToken(GenerateTestToken());

            var player = "testPlayer";
            var score = 1234.0;
            var parameters = $"?player={player}";

            var postBody = new { player, score } ;
            await _client.PostAsJsonAsync(RequestUri, postBody);

            //Act
            var response = await _client.GetAsync(RequestUri + parameters);
            var body = await response.Content.ReadFromJsonAsync<GetScoreResponse>();

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(body);
            Assert.Equal(score, body.Score);
        }

        [Theory]
        [InlineData("")]
        [InlineData("?player=")]
        public async Task InvalidInput_ReturnsBadRequest(string parameters)
        {
            //Arrange
            //Act
            var response = await _client.GetAsync(RequestUri + parameters);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}