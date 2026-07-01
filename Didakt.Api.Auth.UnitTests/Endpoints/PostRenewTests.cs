using Didakt.Api.Auth.Endpoints;
using Didakt.Api.Auth.Endpoints.Requests;
using Didakt.Api.Auth.Endpoints.Responses;
using Didakt.Api.Auth.Services;
using Didakt.Api.Auth.Services.Models;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace Didakt.Api.Auth.UnitTests.Endpoints;

public class PostRenewTests
{
    private readonly Mock<IValidator<RenewRequest>> _validator = new();
    private readonly Mock<IAuthService> _service = new();

    private readonly ValidationResult FailedValidation = new([new ValidationFailure("", "")]);

    [Fact]
    public async Task ValidInput_Ok_Token()
    {
        //Arrange
        var refreshToken = "testRefreshToken";
        var request = new RenewRequest(refreshToken);

        var expectedAccessToken = "newAccessToken";
        var expectedNewRefreshToken = "newRefreshToken";
        var expectedResult = new LoginResult(expectedAccessToken, expectedNewRefreshToken);
        var expectedResponse = new LoginResponse(expectedAccessToken, expectedNewRefreshToken);

        _validator.Setup(x => x.ValidateAsync(It.IsAny<RenewRequest>())).ReturnsAsync(new ValidationResult());
        _service.Setup(x => x.RenewAsync(It.IsAny<string>())).ReturnsAsync(expectedResult);

        //Act
        var result = await EndpointMethods.PostRenew(request, _validator.Object, _service.Object);

        //Assert
        _validator.Verify(x => x.ValidateAsync(request), Times.Once);
        _service.Verify(x => x.RenewAsync(refreshToken), Times.Once);

        var response = Assert.IsType<Ok<LoginResponse>>(result).Value;
        Assert.Equal(expectedResponse, response);
    }

    [Fact]
    public async Task InvalidInput_BadRequest()
    {
        //Arrange
        var request = new RenewRequest("");

        _validator.Setup(x => x.ValidateAsync(It.IsAny<RenewRequest>())).ReturnsAsync(FailedValidation);

        //Act
        var result = await EndpointMethods.PostRenew(request, _validator.Object, _service.Object);

        //Assert
        _validator.Verify(x => x.ValidateAsync(request), Times.Once);
        _service.Verify(x => x.RenewAsync(It.IsAny<string>()), Times.Never);

        var problem = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.StatusCode);
    }

    [Fact]
    public async Task InvalidToken_Unauthorized()
    {
        //Arrange
        var refreshToken = "invalidRefreshToken";
        var request = new RenewRequest(refreshToken);

        _validator.Setup(x => x.ValidateAsync(It.IsAny<RenewRequest>())).ReturnsAsync(new ValidationResult());
        _service.Setup(x => x.RenewAsync(It.IsAny<string>())).ReturnsAsync((LoginResult?)null);

        //Act
        var result = await EndpointMethods.PostRenew(request, _validator.Object, _service.Object);

        //Assert
        _validator.Verify(x => x.ValidateAsync(request), Times.Once);
        _service.Verify(x => x.RenewAsync(refreshToken), Times.Once);

        Assert.IsType<UnauthorizedHttpResult>(result);
    }
}