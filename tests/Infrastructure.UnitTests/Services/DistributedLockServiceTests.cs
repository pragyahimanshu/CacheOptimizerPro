using Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using StackExchange.Redis;

namespace Infrastructure.UnitTests.Services;

public class DistributedLockServiceTests
{
    private readonly Mock<IConnectionMultiplexer> _mockRedis;
    private readonly Mock<IDatabase> _mockDatabase;
    private readonly Mock<ILogger<DistributedLockService>> _mockLogger;
    private readonly DistributedLockService _service;

    public DistributedLockServiceTests()
    {
        _mockRedis = new Mock<IConnectionMultiplexer>();
        _mockDatabase = new Mock<IDatabase>();
        _mockLogger = new Mock<ILogger<DistributedLockService>>();

        _mockRedis.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(_mockDatabase.Object);

        _service = new DistributedLockService(
            _mockRedis.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task AcquireLockAsync_Should_ReturnLockHandle_When_LockAcquired()
    {
        // Arrange
        _mockDatabase.Setup(db => db.StringSetAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.AcquireLockAsync(
            "test-resource",
            TimeSpan.FromSeconds(30),
            TimeSpan.FromSeconds(5),
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task AcquireLockAsync_Should_ReturnNull_When_LockNotAcquired()
    {
        // Arrange
        _mockDatabase.Setup(db => db.StringSetAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.AcquireLockAsync(
            "test-resource",
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(2),
            CancellationToken.None);

        // Assert
        Assert.Null(result);
    }
}
