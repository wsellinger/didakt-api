using Didakt.Api.Leaderboard.Endpoints;
using Didakt.Api.Leaderboard.Endpoints.Requests;
using Didakt.Api.Leaderboard.Endpoints.Responses;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using StackExchange.Redis;

namespace Didakt.Api.Leaderboard.UnitTests.Endpoints
{
    public class GetScoreTests
    {
        private readonly Mock<IValidator<GetScoreRequest>> _validator = new();
        private readonly Mock<IDatabase> _database = new();
        private readonly Mock<IConnectionMultiplexer> _connection = new();

        private readonly ValidationResult PassedValidation = new();
        private readonly ValidationResult FailedValidation = new([new ValidationFailure("", "")]);
        
        public GetScoreTests()
        {
            _connection.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                .Returns(_database.Object);
        }

        [Fact]
        public async Task ValidInput_OK()
        {
            //Arrange
            var category = "testCategory";
            var player = "testPlayer";
            var score = 1234;

            var request = new GetScoreRequest(player);

            _validator.Setup(x => x.ValidateAsync(It.IsAny<GetScoreRequest>())).ReturnsAsync(PassedValidation);
            _database.Setup(x => x.SortedSetScoreAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>())).ReturnsAsync(score);

            //Act
            var result = await EndpointMethods.GetScore(category, request, _validator.Object, _connection.Object);

            //Assert
            _validator.Verify(x => x.ValidateAsync(request), Times.Once);
            _database.Verify(x => x.SortedSetScoreAsync(It.Is<RedisKey>(x => x.ToString().Contains(category)), player), Times.Once);

            var response = Assert.IsType<Ok<GetScoreResponse>>(result);
            Assert.NotNull(response);
            Assert.Equal(score, response?.Value?.Score);
        }

        [Fact]
        public async Task InvalidInput_BadRequest()
        {
            //Arrange
            var category = "testCategory";
            var player = "testPlayer";

            var request = new GetScoreRequest(player);

            _validator.Setup(x => x.ValidateAsync(It.IsAny<GetScoreRequest>())).ReturnsAsync(FailedValidation);
            _database.Setup(x => x.SortedSetScoreAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>()));

            //Act
            var result = await EndpointMethods.GetScore(category, request, _validator.Object, _connection.Object);

            //Assert
            _validator.Verify(x => x.ValidateAsync(request), Times.Once);
            _database.Verify(x => x.SortedSetScoreAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>()), Times.Never);

            var response = Assert.IsType<ProblemHttpResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PlayerNotFound_NotFound()
        {
            //Arrange
            var category = "testCategory";
            var player = "testPlayer";

            var request = new GetScoreRequest(player);

            _validator.Setup(x => x.ValidateAsync(It.IsAny<GetScoreRequest>())).ReturnsAsync(PassedValidation);
            _database.Setup(x => x.SortedSetScoreAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>())).ReturnsAsync((double?)null);

            //Act
            var result = await EndpointMethods.GetScore(category, request, _validator.Object, _connection.Object);

            //Assert
            _validator.Verify(x => x.ValidateAsync(request), Times.Once);
            _database.Verify(x => x.SortedSetScoreAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>()), Times.Once);

            var response = Assert.IsType<NotFound>(result);
        }
    }
}
