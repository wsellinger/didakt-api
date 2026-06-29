using Didakt.Api.Leaderboard.Endpoints.Requests;
using Didakt.Api.Leaderboard.Endpoints.Validators;

namespace Didakt.Api.Leaderboard.UnitTests.Endpoints.Validators
{
    public class PostScoreValidatorTests
    {
        [Fact]
        public async Task ValidInput_IsValid()
        {
            //Arrange
            var userName = "testUser";
            var score = 1;
            var request = new PostScoreRequest(userName, score);
            var validator = new PostScoreValidator();

            //Act
            var result = await validator.ValidateAsync(request);

            //Assert
            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData(null, 1.0)] //Null Player
        [InlineData("", 1.0)] //Empty Player
        [InlineData("user", null)] //Null Score
        [InlineData("user", 0.0)] //Empty Score
        [InlineData("user", -1.0)] //Negative Score
        public async Task InvalidInput_IsNotValid(string? userName, double? score)
        {
            //Arrange
            var request = new PostScoreRequest(userName, score);
            var validator = new PostScoreValidator();

            //Act
            var result = await validator.ValidateAsync(request);

            //Assert
            Assert.False(result.IsValid);
        }
    }
}