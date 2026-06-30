using System.Net;
using System.Net.Http.Json;

namespace Didakt.Api.Auth.IntegrationTests;

public class RegisterTests : TestBase
{
    private const string RegisterRequestUri = "/auth/register";

    [Fact]
    public async Task ValidRequest_Created()
    {
        //Arrange
        var requestBody = new { username = "testUser", password = "testpassword" };

        //Act
        var response = await Client.PostAsJsonAsync(RegisterRequestUri, requestBody);

        //Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task DuplicateUsername_Conflict()
    {
        //Arrange
        var requestBody = new { username = "testUser", password = "testpassword" };
        _ = await Client.PostAsJsonAsync(RegisterRequestUri, requestBody);

        //Act
        var response = await Client.PostAsJsonAsync(RegisterRequestUri, requestBody);

        //Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
