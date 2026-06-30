using Didakt.Api.Leaderboard.Endpoints.Requests;
using Didakt.Api.Leaderboard.Endpoints.Validators;

namespace Didakt.Api.Leaderboard.UnitTests.Endpoints.Validators
{
    public class GetTopValidatorTests
    {
        [Fact]
        public async Task ValidInput_IsValid()
        {
            //Arrange
            var validator = new GetTopValidator();
            var count = 1;
            var request = new GetTopRequest(count);

            //Act
            var result = await validator.ValidateAsync(request);

            //Assert
            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData(null)] //Null count
        [InlineData(0L)] //Zero count
        [InlineData(-1L)] //Negative count
        public async Task InvalidInput_IsNotValid(long? count)
        {
            //Arrange
            var validator = new GetTopValidator();
            var request = new GetTopRequest(count);

            //Act
            var result = await validator.ValidateAsync(request);

            //Assert
            Assert.False(result.IsValid);
        }
    }
}
