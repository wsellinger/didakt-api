using Didakt.Api.Auth.Endpoints.Validators;
using Didakt.Api.Auth.Models.Requests;

namespace Didakt.Api.Auth.Tests.Validators
{
    public class RegisterRequestValidatorTests
    {
        private const string MaxLengthPassword = 
            "123456789012345678901234567890123456789012345678901234567890123" +
            "45678901234567890123456789012345678901234567890123456789012345678";

        public RegisterRequestValidatorTests() { }

        [Theory]
        [InlineData("123", "123456789012")] //Min Size
        [InlineData("12345678901234567890", MaxLengthPassword)] //Max Size
        [InlineData("a-B_3", "!@#$%^&*()-_")] //Allowed Chars
        public void Validate_ValidInput_IsValid(string userName, string password)
        {
            //Arrange
            var request = new RegisterRequest(userName, password);
            var validator = new RegisterRequestValidator();

            //Act
            var result = validator.Validate(request);

            //Assert
            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData("", "123456789012")] //Empty userName
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
        public void Validate_InvalidInput_IsNotValid(string userName, string password)
        {
            //Arrange
            var request = new RegisterRequest(userName, password);
            var validator = new RegisterRequestValidator();

            //Act
            var result = validator.Validate(request);

            //Assert
            Assert.False(result.IsValid);
        }
    }
}
