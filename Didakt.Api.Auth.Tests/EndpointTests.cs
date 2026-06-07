using Didakt.Api.Auth.Models.Requests;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace Didakt.Api.Auth.Tests;

public class EndpointTests
{
    private readonly Mock<IValidator<RegisterRequest>> _validator;

    public EndpointTests() 
    {
        _validator = new Mock<IValidator<RegisterRequest>>();
        _validator.Setup(x => x.ValidateAsync(It.IsAny<RegisterRequest>()))
            .ReturnsAsync(new ValidationResult()); //IsValid == true
    }

    [Fact]
    public async Task PostScore_Given_ValidInput_Returns_OK()
    {
        //Arrange
        var userName = "testUser";
        var password = "testPass";
        var request = new RegisterRequest(userName, password);

        //Act
        var result = await Endpoints.PostRegister(request, _validator.Object);

        //Assert
        Assert.IsType<Ok>(result);
    }

    [Fact]
    public async Task PostScore_Given_InvalidInput_Returns_BadRequest()
    {
        //Arrange
        var userName = "testUser";
        var password = "testPass";
        var request = new RegisterRequest(userName, password);

        _validator.Setup(x => x.ValidateAsync(It.IsAny<RegisterRequest>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("", "")])); //IsValid == false

        //Act
        var result = await Endpoints.PostRegister(request, _validator.Object);

        //Assert
        var problem = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.StatusCode);
    }
}
