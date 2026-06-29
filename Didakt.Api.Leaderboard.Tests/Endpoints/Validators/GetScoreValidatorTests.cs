using Didakt.Api.Leaderboard.Endpoints.Requests;
using Didakt.Api.Leaderboard.Endpoints.Validators;
using System.Numerics;

namespace Didakt.Api.Leaderboard.UnitTests.Endpoints.Validators
{
    public class GetScoreValidatorTests
    {
        [Fact]
        public async Task ValidInput_IsValid()
        {
            //Arrange
            var validator = new GetScoreValidator();
            var player = "testUser";
            var request = new GetScoreRequest(player);

            //Act
            var result = await validator.ValidateAsync(request);

            //Assert
            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task InvalidInput_IsNotValid(string? player)
        {
            //Arrange
            var validator = new GetScoreValidator();
            var request = new GetScoreRequest(player);

            //Act
            var result = await validator.ValidateAsync(request);

            //Assert
            Assert.False(result.IsValid);
        }
    }
}
