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
    public class GetTopTests
    {
        private readonly Mock<IValidator<GetTopRequest>> _validator = new();
        private readonly Mock<IDatabase> _database = new();
        private readonly Mock<IConnectionMultiplexer> _connection = new();

        private readonly ValidationResult PassedValidation = new();
        private readonly ValidationResult FailedValidation = new([new ValidationFailure("", "")]);

        public GetTopTests()
        {
            _connection.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                .Returns(_database.Object);
        }

        [Fact]
        public async Task ValidInput_OK()
        {
            //Arrange
            var category = "testCategory";
            var count = 1;
            
            var rank = 1;
            var player = "testPlayer";
            var score = 1234;

            var request = new GetTopRequest(count);            
            var scores = new SortedSetEntry[] { new(player, score) };
            var expectedResponse = new GetTopResponse(rank, player, score);

            _validator.Setup(x => x.ValidateAsync(It.IsAny<GetTopRequest>())).ReturnsAsync(PassedValidation);
            _database.Setup(x => x.SortedSetRangeByRankWithScoresAsync(
                It.IsAny<RedisKey>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<Order>())).ReturnsAsync(scores);

            //Act
            var result = await EndpointMethods.GetTop(category, request, _validator.Object, _connection.Object);

            //Assert
            _validator.Verify(x => x.ValidateAsync(request), Times.Once);
            _database.Verify(x => x.SortedSetRangeByRankWithScoresAsync(
                It.Is<RedisKey>(x => x.ToString().Contains(category)), 0, count - 1, Order.Descending), Times.Once);

            var response = Assert.IsType<Ok<GetTopResponse[]>>(result);
            Assert.NotNull(response.Value);
            Assert.NotEmpty(response.Value);

            var entry = response.Value[0];
            Assert.Equal(rank, entry.Rank);
            Assert.Equal(player, entry.Player);
            Assert.Equal(score, entry.Score);
        }

        [Fact]
        public async Task InvalidInput_BadRequest()
        {
            //Arrange
            var category = "testCategory";
            var count = 1;

            var request = new GetTopRequest(count);

            _validator.Setup(x => x.ValidateAsync(It.IsAny<GetTopRequest>())).ReturnsAsync(FailedValidation);
            _database.Setup(x => x.SortedSetRangeByRankWithScoresAsync(It.IsAny<RedisKey>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<Order>()));

            //Act
            var result = await EndpointMethods.GetTop(category, request, _validator.Object, _connection.Object);

            //Assert
            _validator.Verify(x => x.ValidateAsync(request), Times.Once);
            _database.Verify(x => x.SortedSetRangeByRankWithScoresAsync(It.IsAny<RedisKey>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<Order>()), Times.Never);

            var response = Assert.IsType<ProblemHttpResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
        }
    }
}
