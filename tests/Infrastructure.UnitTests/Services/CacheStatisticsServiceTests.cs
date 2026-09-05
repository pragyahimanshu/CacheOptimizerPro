using Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Infrastructure.UnitTests.Services;

public class CacheStatisticsServiceTests
{
    private readonly Mock<ILogger<CacheStatisticsService>> _mockLogger;
    private readonly CacheStatisticsService _service;

    public CacheStatisticsServiceTests()
    {
        _mockLogger = new Mock<ILogger<CacheStatisticsService>>();
        _service = new CacheStatisticsService(_mockLogger.Object);
    }

    [Fact]
    public void RecordHit_Should_IncrementHitCount()
    {
        // Act
        _service.RecordHit("test-key", 10.5);
        var metrics = _service.GetMetrics();

        // Assert
        Assert.Equal(1, metrics.Hits);
        Assert.Equal(0, metrics.Misses);
    }

    [Fact]
    public void RecordMiss_Should_IncrementMissCount()
    {
        // Act
        _service.RecordMiss("test-key", 15.2);
        var metrics = _service.GetMetrics();

        // Assert
        Assert.Equal(0, metrics.Hits);
        Assert.Equal(1, metrics.Misses);
    }

    [Fact]
    public void RecordInvalidation_Should_IncrementInvalidationCount()
    {
        // Act
        _service.RecordInvalidation("test-key");
        var metrics = _service.GetMetrics();

        // Assert
        Assert.Equal(1, metrics.Invalidations);
    }

    [Fact]
    public void GetMetrics_Should_CalculateHitRate()
    {
        // Arrange
        _service.RecordHit("key1", 10.0);
        _service.RecordHit("key2", 15.0);
        _service.RecordMiss("key3", 20.0);

        // Act
        var metrics = _service.GetMetrics();

        // Assert
        Assert.Equal(2, metrics.Hits);
        Assert.Equal(1, metrics.Misses);
        Assert.Equal(0.66, Math.Round(metrics.HitRate, 2));
    }
}
