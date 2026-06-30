using Didakt.Api.Auth.Endpoints.Responses;
using System.Net;
using System.Net.Http.Json;

namespace Didakt.Api.Auth.IntegrationTests
{
    public class LoginTests : TestBase
    {
        private const string RegisterRequestUri = "/auth/register";
        private const string LoginRequestUri = "/auth/login";

        [Fact]
        public async Task ValidRequest_OK_HasToken()
        {
            //Arrange
            var requestBody = new { username = "testUser", password = "testPassword" };
            await Client.PostAsJsonAsync(RegisterRequestUri, requestBody);

            //Act
            var response = await Client.PostAsJsonAsync(LoginRequestUri, requestBody);
            var body = await response.Content.ReadFromJsonAsync<LoginResponse>();

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(body?.Token);
        }

        [Fact]
        public async Task BadCredentials_Unauthorized()
        {
            //Arrange
            var registerBody = new { username = "testUser", password = "testPassword" };
            var loginBody = new { username = "testUser", password = "invalidPassword" };
            await Client.PostAsJsonAsync(RegisterRequestUri, registerBody);

            //Act
            var response = await Client.PostAsJsonAsync(LoginRequestUri, loginBody);

            //Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
