using Microsoft.AspNetCore.Http.HttpResults;

namespace Didakt.Api.Auth.Tests;

public class EndpointTests
{

    public EndpointTests() {}

    [Fact]
    public async Task PostScore_Given_ValidInput_Returns_OK()
    {
        //Arrange

        //Act
        var result = await Endpoints.PostRegister();

        //Assert
        Assert.IsType<Ok>(result);
    }
}
