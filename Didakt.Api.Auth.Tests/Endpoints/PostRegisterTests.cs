using Didakt.Api.Auth.Endpoints;
using Didakt.Api.Auth.Endpoints.Requests;
using Didakt.Api.Auth.Services;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace Didakt.Api.Auth.UnitTests.Endpoints;

public class PostRegisterTests
{
    private readonly Mock<IValidator<RegisterRequest>> _validator = new();
    private readonly Mock<IAuthService> _service = new();

    private readonly ValidationResult FailedValidation = new([new ValidationFailure("", "")]);

    public PostRegisterTests() 
    {
        _validator.Setup(x => x.ValidateAsync(It.IsAny<RegisterRequest>()))
            .ReturnsAsync(new ValidationResult()); //IsValid == true

        _service.Setup(x => x.RegisterAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);
    }

    [Fact]
    public async Task ValidInput_Created()
    {
        //Arrange
        var userName = "testUser";
        var password = "testPass";
        var request = new RegisterRequest(userName, password);

        _validator.Setup(x => x.ValidateAsync(It.IsAny<RegisterRequest>())).ReturnsAsync(new ValidationResult());
        _service.Setup(x => x.RegisterAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        //Act
        var result = await EndpointMethods.PostRegister(request, _validator.Object, _service.Object);

        //Assert
        _validator.Verify(x => x.ValidateAsync(request), Times.Once);
        _service.Verify(x => x.RegisterAsync(userName, password), Times.Once);

        Assert.IsType<Created>(result);
    }

    [Fact]
    public async Task InvalidInput_BadRequest()
    {
        //Arrange
        var userName = "testUser";
        var password = "testPass";
        var request = new RegisterRequest(userName, password);

        _validator.Setup(x => x.ValidateAsync(It.IsAny<RegisterRequest>())).ReturnsAsync(FailedValidation);
        _service.Setup(x => x.RegisterAsync(It.IsAny<string>(), It.IsAny<string>()));

        //Act
        var result = await EndpointMethods.PostRegister(request, _validator.Object, _service.Object);

        //Assert
        _validator.Verify(x => x.ValidateAsync(request), Times.Once);
        _service.Verify(x => x.RegisterAsync(userName, password), Times.Never);

        var problem = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.StatusCode);
    }

    [Fact]
    public async Task UserExists_Conflict()
    {
        //Arrange
        var userName = "testUser";
        var password = "testPass";
        var request = new RegisterRequest(userName, password);

        _service.Setup(x => x.RegisterAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        //Act
        var result = await EndpointMethods.PostRegister(request, _validator.Object, _service.Object);

        //Assert
        _validator.Verify(x => x.ValidateAsync(request), Times.Once);
        _service.Verify(x => x.RegisterAsync(userName, password), Times.Once);

        Assert.IsType<Conflict>(result);
    }
}
