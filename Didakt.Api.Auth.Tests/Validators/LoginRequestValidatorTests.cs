using Didakt.Api.Auth.Endpoints.Requests;
using Didakt.Api.Auth.Endpoints.Validators;

namespace Didakt.Api.Auth.Tests.Validators
{
    public class LoginRequestValidatorTests
    {
        public LoginRequestValidatorTests() { }

        [Fact]
        public void Validate_ValidInput_IsValid()
        {
            //Arrange
            var userName = "testUser";
            var password = "testPass";
            var request = new LoginRequest(userName, password);
            var validator = new LoginRequestValidator();

            //Act
            var result = validator.Validate(request);

            //Assert
            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData("", "123456789012")] //Empty userName
        [InlineData("12", "")] //Empty Password
        public void Validate_InvalidInput_IsNotValid(string userName, string password)
        {
            //Arrange
            var request = new LoginRequest(userName, password);
            var validator = new LoginRequestValidator();

            //Act
            var result = validator.Validate(request);

            //Assert
            Assert.False(result.IsValid);
        }
    }
}
