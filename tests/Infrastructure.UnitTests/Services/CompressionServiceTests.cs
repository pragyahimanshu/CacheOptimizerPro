using Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Infrastructure.UnitTests.Services;

public class CompressionServiceTests
{
    private readonly Mock<ILogger<CompressionService>> _mockLogger;
    private readonly CompressionService _service;

    public CompressionServiceTests()
    {
        _mockLogger = new Mock<ILogger<CompressionService>>();
        _service = new CompressionService(_mockLogger.Object);
    }

    [Fact]
    public void Compress_Should_ReduceDataSize()
    {
        // Arrange
        var originalData = System.Text.Encoding.UTF8.GetBytes("This is a test string that should compress well because it has repeated patterns and enough data to benefit from compression");

        // Act
        var compressedData = _service.Compress(originalData);

        // Assert
        Assert.NotNull(compressedData);
        Assert.True(compressedData.Length < originalData.Length);
    }

    [Fact]
    public void Decompress_Should_RestoreOriginalData()
    {
        // Arrange
        var originalText = "This is a test string for compression and decompression";
        var originalData = System.Text.Encoding.UTF8.GetBytes(originalText);
        var compressedData = _service.Compress(originalData);

        // Act
        var decompressedData = _service.Decompress(compressedData);

        // Assert
        Assert.NotNull(decompressedData);
        var decompressedText = System.Text.Encoding.UTF8.GetString(decompressedData);
        Assert.Equal(originalText, decompressedText);
    }

    [Fact]
    public void CompressString_Should_CompressText()
    {
        // Arrange
        var originalText = "This is a test string that should compress well";

        // Act
        var compressedText = _service.CompressString(originalText);

        // Assert
        Assert.NotNull(compressedText);
        Assert.NotEqual(originalText, compressedText);
    }

    [Fact]
    public void DecompressString_Should_RestoreOriginalText()
    {
        // Arrange
        var originalText = "This is a test string for compression and decompression";
        var compressedText = _service.CompressString(originalText);

        // Act
        var decompressedText = _service.DecompressString(compressedText);

        // Assert
        Assert.Equal(originalText, decompressedText);
    }
}
