using Domain.Interfaces;
using Domain.Shared;
using Domain.ValueObjects;
using Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using StackExchange.Redis;

namespace Infrastructure.UnitTests.Services;

public class CacheInvalidationServiceTests
{
    private readonly Mock<IConnectionMultiplexer> _mockRedis;
    private readonly Mock<ISubscriber> _mockSubscriber;
    private readonly Mock<ILogger<CacheInvalidationService>> _mockLogger;
    private readonly Mock<ICacheRepository> _mockCacheRepository;
    private readonly CacheInvalidationService _service;

    public CacheInvalidationServiceTests()
    {
        _mockRedis = new Mock<IConnectionMultiplexer>();
        _mockSubscriber = new Mock<ISubscriber>();
        _mockLogger = new Mock<ILogger<CacheInvalidationService>>();
        _mockCacheRepository = new Mock<ICacheRepository>();

        _mockRedis.Setup(r => r.GetSubscriber(null)).Returns(_mockSubscriber.Object);

        // Setup RedisSettings
        var mockSettings = Microsoft.Extensions.Options.Options.Create(new Settings.RedisSettings
        {
            ConnectionString = "localhost:6379",
            InvalidationChannel = "test-channel"
        });

        _service = new CacheInvalidationService(
            _mockRedis.Object,
            mockSettings,
            _mockLogger.Object,
            _mockCacheRepository.Object);
    }

    [Fact]
    public async Task InvalidateOnDatabaseChangeAsync_Should_DeleteFromCache_And_PublishMessage()
    {
        // Arrange
        var cacheKey = CacheKey.Create("test-key").Value;
        var entityName = "Product";
        
        _mockCacheRepository.Setup(r => r.InvalidateAsync(cacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _service.InvalidateOnDatabaseChangeAsync(cacheKey, entityName, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _mockCacheRepository.Verify(r => r.InvalidateAsync(cacheKey, It.IsAny<CancellationToken>()), Times.Once);
        _mockSubscriber.Verify(s => s.PublishAsync(
            RedisChannel.Literal("test-channel"),
            It.IsAny<RedisValue>(),
            CommandFlags.FireAndForget), Times.Once);
    }

    [Fact]
    public async Task InvalidateOnDatabaseChangeAsync_Should_ReturnFailure_When_CacheInvalidationFails()
    {
        // Arrange
        var cacheKey = CacheKey.Create("test-key").Value;
        var entityName = "Product";
        
        _mockCacheRepository.Setup(r => r.InvalidateAsync(cacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(Domain.Errors.DomainErrors.Cache.InvalidationFailed));

        // Act
        var result = await _service.InvalidateOnDatabaseChangeAsync(cacheKey, entityName, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        _mockCacheRepository.Verify(r => r.InvalidateAsync(cacheKey, It.IsAny<CancellationToken>()), Times.Once);
        _mockSubscriber.Verify(s => s.PublishAsync(It.IsAny<RedisChannel>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()), Times.Never);
    }
}
