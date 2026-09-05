using System.Text.Json;
using Application.Products.Queries.GetProduct;
using Domain.Entities;
using Domain.Errors;
using Domain.Interfaces;
using Domain.Shared;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Application.UnitTests.Products.Queries;

public class GetProductQueryHandlerTests
{
    private readonly Mock<ICacheStrategy> _cacheStrategyMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly GetProductQueryHandler _handler;

    public GetProductQueryHandlerTests()
    {
        _cacheStrategyMock = new Mock<ICacheStrategy>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _handler = new GetProductQueryHandler(
            _cacheStrategyMock.Object,
            _productRepositoryMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnProduct_WhenProductIsCached()
    {
        // Arrange
        var product = Product.Create(
            "Laptop",
            "High-performance laptop",
            1200.00m,
            10
        ).Value;

        var cacheKey = CacheKey.Create($"products:{product.Id}").Value;

        _cacheStrategyMock
            .Setup(x => x.ReadThroughAsync(
                It.IsAny<CacheKey>(),
                It.IsAny<Func<Task<Result<byte[]>>>>(),
                It.IsAny<CacheExpiration>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(CachedItem.Create(
                Guid.NewGuid(),
                cacheKey.Value,
                JsonSerializer.SerializeToUtf8Bytes(product),
                CacheExpiration.Default
            )));

        var query = new GetProductQuery(product.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(product);
    }

    [Fact]
    public async Task Handle_ShouldReturnProduct_WhenProductIsNotCached()
    {
        // Arrange
        var product = Product.Create(
            "Laptop",
            "High-performance laptop",
            1200.00m,
            10
        ).Value;

        var cacheKey = CacheKey.Create($"products:{product.Id}").Value;

        _cacheStrategyMock
            .Setup(x => x.ReadThroughAsync(
                It.IsAny<CacheKey>(),
                It.IsAny<Func<Task<Result<byte[]>>>>(),
                It.IsAny<CacheExpiration>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(CachedItem.Create(
                Guid.NewGuid(),
                cacheKey.Value,
                JsonSerializer.SerializeToUtf8Bytes(product),
                CacheExpiration.Default
            )));

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var query = new GetProductQuery(product.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(product);
        _productRepositoryMock.Verify(x => x.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenProductIsNotFound()
    {
        // Arrange
        var productId = Guid.NewGuid();

        _cacheStrategyMock
            .Setup(x => x.ReadThroughAsync(
                It.IsAny<CacheKey>(),
                It.IsAny<Func<Task<Result<byte[]>>>>(),
                It.IsAny<CacheExpiration>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<CachedItem>(DomainErrors.Product.NotFound(productId)));

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var query = new GetProductQuery(productId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Product.NotFound(productId));
    }
}