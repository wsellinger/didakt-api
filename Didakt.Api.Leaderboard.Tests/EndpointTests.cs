using Didakt.Api.Leaderboard.Endpoints;
using Didakt.Api.Leaderboard.Models.Requests;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using StackExchange.Redis;

namespace Didakt.Api.Leaderboard.Tests;

public class EndpointTests
{
    private readonly Mock<IDatabase> _database;
    private readonly Mock<IConnectionMultiplexer> _connection;

    public EndpointTests()
    {
        _database = new Mock<IDatabase>();
        _connection = new Mock<IConnectionMultiplexer>();

        _connection.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(_database.Object);
    }

    [Fact]
    public async Task PostScore_Given_ValidInput_Returns_OK()
    {
        //Arrange
        var categoryName = "testCategory";
        var playerName = "testPlayer";
        var scoreAmount = 1234;
        var postScoreRequest = new PostScoreRequest(playerName, scoreAmount);

        _database.Setup(x => x.SortedSetAddAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<double>()));

        //Act
        var result = await EndpointMethods.PostScore(categoryName, postScoreRequest, _connection.Object);

        //Assert
        _database.Verify(x => x.SortedSetAddAsync(It.Is<RedisKey>(x => x.ToString().Contains(categoryName)), playerName, scoreAmount), Times.Once);

        Assert.IsType<Ok>(result);
    }
}
