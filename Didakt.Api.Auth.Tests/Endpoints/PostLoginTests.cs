using Didakt.Api.Auth.Endpoints;
using Didakt.Api.Auth.Endpoints.Requests;
using Didakt.Api.Auth.Endpoints.Responses;
using Didakt.Api.Auth.Services;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace Didakt.Api.Auth.UnitTests.Endpoints;

public class PostLoginTests
{
    private readonly Mock<IValidator<LoginRequest>> _validator = new();
    private readonly Mock<IAuthService> _service = new();

    private readonly ValidationResult FailedValidation = new([new ValidationFailure("", "")]);

    public PostLoginTests() { }

    [Fact]
    public async Task ValidInput_Ok_Token()
    {
        //Arrange
        var userName = "testUser";
        var password = "testPass";
        var request = new LoginRequest(userName, password);
        var expectedToken = "testToken";
        var expectedLoginResponse = new LoginResponse(expectedToken);

        _validator.Setup(x => x.ValidateAsync(It.IsAny<LoginRequest>())).ReturnsAsync(new ValidationResult());
        _service.Setup(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(expectedToken);

        //Act
        var result = await EndpointMethods.PostLogin(request, _validator.Object, _service.Object);

        //Assert
        _validator.Verify(x => x.ValidateAsync(request), Times.Once);
        _service.Verify(x => x.LoginAsync(userName, password), Times.Once);

        var token = Assert.IsType<Ok<LoginResponse>>(result).Value;
        Assert.Equal(expectedLoginResponse, token);
    }

    [Fact]
    public async Task InvalidInput_BadRequest()
    {
        //Arrange
        var userName = "testUser";
        var password = "testPass";
        var request = new LoginRequest(userName, password);

        _validator.Setup(x => x.ValidateAsync(It.IsAny<LoginRequest>())).ReturnsAsync(FailedValidation);
        _service.Setup(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>()));

        //Act
        var result = await EndpointMethods.PostLogin(request, _validator.Object, _service.Object);

        //Assert
        _validator.Verify(x => x.ValidateAsync(request), Times.Once);
        _service.Verify(x => x.LoginAsync(userName, password), Times.Never);

        var problem = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.StatusCode);
    }

    [Fact]
    public async Task InvalidCredentials_Unauthorized()
    {
        //Arrange
        var userName = "testUser";
        var password = "testPass";
        var request = new LoginRequest(userName, password);

        _validator.Setup(x => x.ValidateAsync(It.IsAny<LoginRequest>())).ReturnsAsync(new ValidationResult());
        _service.Setup(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>()));

        //Act
        var result = await EndpointMethods.PostLogin(request, _validator.Object, _service.Object);

        //Assert
        _validator.Verify(x => x.ValidateAsync(request), Times.Once);
        _service.Verify(x => x.LoginAsync(userName, password), Times.Once);

        Assert.IsType<UnauthorizedHttpResult>(result);
    }
}
