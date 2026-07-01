using Didakt.Api.Auth.Endpoints.Requests;
using Didakt.Api.Auth.Endpoints.Validators;

namespace Didakt.Api.Auth.UnitTests.Endpoints.Validators
{
    public class RenewValidatorTests
    {
        private readonly RenewValidator _validator = new();

        [Fact]
        public async Task ValidInput_IsValid()
        {
            //Arrange
            var request = new RenewRequest("someValidRefreshToken");

            //Act
            var result = await _validator.ValidateAsync(request);

            //Assert
            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task InvalidInput_IsNotValid(string? refreshToken)
        {
            //Arrange
            var request = new RenewRequest(refreshToken);

            //Act
            var result = await _validator.ValidateAsync(request);

            //Assert
            Assert.False(result.IsValid);
        }
    }
}