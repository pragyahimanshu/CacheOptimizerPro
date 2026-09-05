using Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Infrastructure.UnitTests.Services;

public class CircuitBreakerServiceTests
{
    private readonly Mock<ILogger<CircuitBreakerService>> _mockLogger;
    private readonly CircuitBreakerService _service;

    public CircuitBreakerServiceTests()
    {
        _mockLogger = new Mock<ILogger<CircuitBreakerService>>();
        _service = new CircuitBreakerService(_mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_Should_ExecuteOperation_When_CircuitClosed()
    {
        // Arrange
        var operationResult = "success";
        var fallbackResult = "fallback";

        // Act
        var result = await _service.ExecuteAsync(
            "test-operation",
            () => Task.FromResult(operationResult),
            () => Task.FromResult(fallbackResult));

        // Assert
        Assert.Equal(operationResult, result);
    }

    [Fact]
    public async Task ExecuteAsync_Should_UseFallback_When_CircuitOpen()
    {
        // Arrange
        var operationResult = "success";
        var fallbackResult = "fallback";

        // Simulate circuit breaker opening by causing failures
        for (int i = 0; i < 5; i++)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ExecuteAsync(
                "test-operation",
                () => Task.FromException<string>(new InvalidOperationException("Test exception")),
                () => Task.FromResult(fallbackResult)));
        }

        // Act - Now the circuit should be open
        var result = await _service.ExecuteAsync(
            "test-operation",
            () => Task.FromResult(operationResult),
            () => Task.FromResult(fallbackResult));

        // Assert
        Assert.Equal(fallbackResult, result);
    }

    [Fact]
    public void IsClosed_Should_ReturnTrue_When_CircuitNotTripped()
    {
        // Act
        var result = _service.IsClosed("test-operation");

        // Assert
        Assert.True(result);
    }
}
