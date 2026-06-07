using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity.Data;

namespace Didakt.Api.Auth.Tests;

public class EndpointTests
{
    public EndpointTests() {}

    [Fact]
    public async Task PostScore_Given_ValidInput_Returns_OK()
    {
        //Arrange
        var userName = "testUser";
        var password = "testPass";
        var request = new Endpoints.RegisterRequest(userName, password);

        //Act
        var result = await Endpoints.PostRegister(request);

        //Assert
        Assert.IsType<Ok>(result);
    }
}
