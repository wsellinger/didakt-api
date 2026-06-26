using Didakt.Api.Auth.Endpoints;
using Didakt.Api.Auth.Endpoints.Requests;
using Didakt.Api.Auth.Endpoints.Responses;
using Didakt.Api.Auth.Services;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace Didakt.Api.Auth.Tests.Endpoints;

public class LoginEndpointTests
{
    private readonly Mock<IValidator<LoginRequest>> _validator;
    private readonly Mock<IAuthService> _service;

    public LoginEndpointTests() 
    {
        _validator = new Mock<IValidator<LoginRequest>>();
        _validator.Setup(x => x.ValidateAsync(It.IsAny<LoginRequest>()))
            .ReturnsAsync(new ValidationResult()); //IsValid == true

        _service = new Mock<IAuthService>();
    }

    [Fact]
    public async Task PostLogin_ValidInput_Ok_Token()
    {
        //Arrange
        var userName = "testUser";
        var password = "testPass";
        var request = new LoginRequest(userName, password);
        var expectedToken = "testToken";
        var expectedLoginResponse = new LoginResponse(expectedToken);

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
    public async Task PostLogin_InvalidInput_BadRequest()
    {
        //Arrange
        var userName = "testUser";
        var password = "testPass";
        var request = new LoginRequest(userName, password);

        _validator.Setup(x => x.ValidateAsync(It.IsAny<LoginRequest>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("", "")])); //IsValid == false

        //Act
        var result = await EndpointMethods.PostLogin(request, _validator.Object, _service.Object);

        //Assert
        _validator.Verify(x => x.ValidateAsync(request), Times.Once);
        _service.Verify(x => x.LoginAsync(userName, password), Times.Never);

        var problem = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.StatusCode);
    }

    [Fact]
    public async Task PostLogin_InvalidCredentials_Unauthorized()
    {
        //Arrange
        var userName = "testUser";
        var password = "testPass";
        var request = new LoginRequest(userName, password);

        _service.Setup(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>()));

        //Act
        var result = await EndpointMethods.PostLogin(request, _validator.Object, _service.Object);

        //Assert
        _validator.Verify(x => x.ValidateAsync(request), Times.Once);
        _service.Verify(x => x.LoginAsync(userName, password), Times.Once);

        Assert.IsType<UnauthorizedHttpResult>(result);
    }
}
