using Didakt.Api.Auth.Endpoints.Requests;
using Didakt.Api.Auth.Endpoints.Validators;

namespace Didakt.Api.Auth.UnitTests.Endpoints.Validators
{
    public class LoginValidatorTests
    {
        [Fact]
        public async Task ValidInput_IsValid()
        {
            //Arrange
            var userName = "testUser";
            var password = "testPass";
            var request = new LoginRequest(userName, password);
            var validator = new LoginValidator();

            //Act
            var result = await validator.ValidateAsync(request);

            //Assert
            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData(null, "123456789012")] //Null UserName
        [InlineData("", "123456789012")] //Empty UserName
        [InlineData("12", null)] //Null Password
        [InlineData("12", "")] //Empty Password
        public async Task InvalidInput_IsNotValid(string? userName, string? password)
        {
            //Arrange
            var request = new LoginRequest(userName, password);
            var validator = new LoginValidator();

            //Act
            var result = await validator.ValidateAsync(request);

            //Assert
            Assert.False(result.IsValid);
        }
    }
}
