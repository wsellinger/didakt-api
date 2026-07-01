using Didakt.Api.Auth.Endpoints;
using Didakt.Api.Auth.Endpoints.Requests;
using Didakt.Api.Auth.Services;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace Didakt.Api.Auth.UnitTests.Endpoints;

public class PostLogoutTests
{
    private readonly Mock<IValidator<LogoutRequest>> _validator = new();
    private readonly Mock<IAuthService> _service = new();

    private readonly ValidationResult FailedValidation = new([new ValidationFailure("", "")]);

    [Fact]
    public async Task ValidRequest_Ok()
    {
        //Arrange
        var refreshToken = "validToken";
        var request = new LogoutRequest(refreshToken);

        _validator.Setup(x => x.ValidateAsync(It.IsAny<LogoutRequest>())).ReturnsAsync(new ValidationResult());
        _service.Setup(x => x.LogoutAsync(It.IsAny<string>())).ReturnsAsync(true);

        //Act
        var result = await EndpointMethods.PostLogout(request, _validator.Object, _service.Object);

        //Assert
        _validator.Verify(x => x.ValidateAsync(request), Times.Once);
        _service.Verify(x => x.LogoutAsync(refreshToken), Times.Once);

        Assert.IsType<Ok>(result);
    }

    [Fact]
    public async Task InvalidInput_BadRequest()
    {
        //Arrange
        var request = new LogoutRequest("");

        _validator.Setup(x => x.ValidateAsync(It.IsAny<LogoutRequest>())).ReturnsAsync(FailedValidation);

        //Act
        var result = await EndpointMethods.PostLogout(request, _validator.Object, _service.Object);

        //Assert
        _validator.Verify(x => x.ValidateAsync(request), Times.Once);
        _service.Verify(x => x.LogoutAsync(It.IsAny<string>()), Times.Never);

        var problem = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.StatusCode);
    }

    [Fact]
    public async Task UnknownToken_NotFound()
    {
        //Arrange
        var refreshToken = "unknownToken";
        var request = new LogoutRequest(refreshToken);

        _validator.Setup(x => x.ValidateAsync(It.IsAny<LogoutRequest>())).ReturnsAsync(new ValidationResult());
        _service.Setup(x => x.LogoutAsync(It.IsAny<string>())).ReturnsAsync(false);

        //Act
        var result = await EndpointMethods.PostLogout(request, _validator.Object, _service.Object);

        //Assert
        Assert.IsType<NotFound>(result);
    }
}