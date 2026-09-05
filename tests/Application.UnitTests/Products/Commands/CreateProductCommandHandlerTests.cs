using Application.Products.Commands.CreateProduct;
using Domain.Entities;
using Domain.Errors;
using Domain.Interfaces;
using Domain.Shared;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Application.UnitTests.Products.Commands;

public class CreateProductCommandHandlerTests
{
    private readonly Mock<ICacheStrategy> _cacheStrategyMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _cacheStrategyMock = new Mock<ICacheStrategy>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CreateProductCommandHandler(
            _cacheStrategyMock.Object,
            _productRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnProductId_WhenProductIsCreated()
    {
        // Arrange
        var product = Product.Create(
            "Laptop",
            "High-performance laptop",
            1200.00m,
            10
        ).Value;

        _ = CacheKey.Create($"products:{product.Id}").Value;

        _cacheStrategyMock
            .Setup(x => x.WriteThroughAsync(
                It.IsAny<CacheKey>(),
                It.IsAny<byte[]>(),
                It.IsAny<CacheExpiration>(),
                It.IsAny<Func<Task<Result>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var command = new CreateProductCommand(
            "Laptop",
            "High-performance laptop",
            1200.00m,
            10
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(product.Id);
        _productRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenProductCreationFails()
    {
        // Arrange
        var command = new CreateProductCommand(
            "", // Invalid name
            "High-performance laptop",
            1200.00m,
            10
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Product.InvalidName);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCacheWriteFails()
    {
        // Arrange
        var product = Product.Create(
            "Laptop",
            "High-performance laptop",
            1200.00m,
            10
        ).Value;

        _ = CacheKey.Create($"products:{product.Id}").Value;

        _cacheStrategyMock
            .Setup(x => x.WriteThroughAsync(
                It.IsAny<CacheKey>(),
                It.IsAny<byte[]>(),
                It.IsAny<CacheExpiration>(),
                It.IsAny<Func<Task<Result>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(new Error("Cache.WriteFailed", "Failed to write to cache.")));

        var command = new CreateProductCommand(
            "Laptop",
            "High-performance laptop",
            1200.00m,
            10
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Cache.WriteFailed");
    }
}