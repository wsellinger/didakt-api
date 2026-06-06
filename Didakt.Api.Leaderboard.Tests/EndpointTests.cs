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
        var gameName = "testGame";
        var playerName = "testPlayer";
        var scoreAmount = 1234;
        var postScoreRequest = new Endpoints.PostScoreRequest(playerName, scoreAmount);

        _database.Setup(x => x.SortedSetAddAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<double>(), It.IsAny<SortedSetWhen>(), It.IsAny<CommandFlags>()));

        //Act
        var result = await Endpoints.PostScore(gameName, postScoreRequest, _connection.Object);

        //Assert
        Assert.IsType<Ok>(result);
    }
}
