using Domain.Entities;
using Domain.Interfaces;
using Domain.Shared;
using Domain.ValueObjects;
using Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Infrastructure.UnitTests.Services;

public class CacheWarmingServiceTests
{
    private readonly Mock<IProductRepository> _mockProductRepository;
    private readonly Mock<ICacheStrategy> _mockCacheStrategy;
    private readonly Mock<ILogger<CacheWarmingService>> _mockLogger;
    private readonly CacheWarmingService _service;

    public CacheWarmingServiceTests()
    {
        _mockProductRepository = new Mock<IProductRepository>();
        _mockCacheStrategy = new Mock<ICacheStrategy>();
        _mockLogger = new Mock<ILogger<CacheWarmingService>>();

        _service = new CacheWarmingService(
            _mockProductRepository.Object,
            _mockCacheStrategy.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task WarmupCacheAsync_Should_LoadProductsIntoCache()
    {
        byte[] exampleBytes = { 1, 2, 3, 4, 5 };

        // Arrange
        var products = new List<Product>
        {
            Product.Create("Product 1", "Description 1", 10.0m, 5).Value,
            Product.Create("Product 2", "Description 2", 20.0m, 10).Value
        };

        _mockProductRepository.Setup(r => r.GetTopProductsAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        _mockCacheStrategy.Setup(s => s.ReadThroughAsync(
                It.IsAny<CacheKey>(),
                It.IsAny<Func<Task<Result<byte[]>>>>(),
                It.IsAny<CacheExpiration>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(CachedItem.Create(
                Guid.NewGuid(),
                "test-key",
                exampleBytes,
                CacheExpiration.Create(TimeSpan.FromSeconds(10)).Value)));

        // Act
        // We can't directly call the private method, but we can test the service start
        await _service.StartAsync(CancellationToken.None);

        // Assert
        _mockProductRepository.Verify(r => r.GetTopProductsAsync(100, It.IsAny<CancellationToken>()), Times.Once);
        _mockCacheStrategy.Verify(s => s.ReadThroughAsync(
                It.IsAny<CacheKey>(),
                It.IsAny<Func<Task<Domain.Shared.Result<byte[]>>>>(),
                It.IsAny<CacheExpiration>(),
                It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}
