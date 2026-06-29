using Didakt.Api.Auth.Endpoints.Requests;
using Didakt.Api.Auth.Endpoints.Validators;

namespace Didakt.Api.Auth.UnitTests.Endpoints.Validators
{
    public class RegisterValidatorTests
    {
        private const string MaxLengthPassword = 
            "123456789012345678901234567890123456789012345678901234567890123" +
            "45678901234567890123456789012345678901234567890123456789012345678";

        [Theory]
        [InlineData("123", "123456789012")] //Min Size
        [InlineData("12345678901234567890", MaxLengthPassword)] //Max Size
        [InlineData("a-B_3", "!@#$%^&*()-_")] //Allowed Chars
        public async Task ValidInput_IsValid(string? userName, string? password)
        {
            //Arrange
            var request = new RegisterRequest(userName, password);
            var validator = new RegisterValidator();

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
        [InlineData("12", "123456789012")] //Short userName
        [InlineData("123", "12345678901")] //Short password
        [InlineData("123456789012345678901", "123456789012")] //Long userName
        [InlineData("123", MaxLengthPassword + "9")] //Long password
        [InlineData("a@c", "12345678901")] //Invalid character in userName
        [InlineData("-abc", "12345678901")] //Invalid character at start of userName
        [InlineData("_abc", "12345678901")] //Invalid character at start of userName
        [InlineData("abc-", "12345678901")] //Invalid character at end of userName
        [InlineData("abc_", "12345678901")] //Invalid character at end of userName
        public async Task InvalidInput_IsNotValid(string? userName, string? password)
        {
            //Arrange
            var request = new RegisterRequest(userName, password);
            var validator = new RegisterValidator();

            //Act
            var result = await validator.ValidateAsync(request);

            //Assert
            Assert.False(result.IsValid);
        }
    }
}
