using Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Infrastructure.UnitTests.Services;

public class CacheEncryptionServiceTests
{
    private readonly Mock<ILogger<CacheEncryptionService>> _mockLogger;
    private readonly CacheEncryptionService _service;
    private readonly string _testKey = "test-encryption-key-32bytes!";

    public CacheEncryptionServiceTests()
    {
        _mockLogger = new Mock<ILogger<CacheEncryptionService>>();
        _service = new CacheEncryptionService(_mockLogger.Object);
    }

    [Fact]
    public void Encrypt_Should_ProduceDifferentOutput()
    {
        // Arrange
        var originalData = System.Text.Encoding.UTF8.GetBytes("This is test data to encrypt");

        // Act
        var encryptedData = _service.Encrypt(originalData, _testKey);

        // Assert
        Assert.NotNull(encryptedData);
        Assert.NotEqual(originalData, encryptedData);
    }

    [Fact]
    public void Decrypt_Should_RestoreOriginalData()
    {
        // Arrange
        var originalText = "This is test data to encrypt and decrypt";
        var originalData = System.Text.Encoding.UTF8.GetBytes(originalText);
        var encryptedData = _service.Encrypt(originalData, _testKey);

        // Act
        var decryptedData = _service.Decrypt(encryptedData, _testKey);

        // Assert
        Assert.NotNull(decryptedData);
        var decryptedText = System.Text.Encoding.UTF8.GetString(decryptedData);
        Assert.Equal(originalText, decryptedText);
    }

    [Fact]
    public void EncryptString_Should_ProduceDifferentOutput()
    {
        // Arrange
        var originalText = "This is a test string";

        // Act
        var encryptedText = _service.EncryptString(originalText, _testKey);

        // Assert
        Assert.NotNull(encryptedText);
        Assert.NotEqual(originalText, encryptedText);
    }

    [Fact]
    public void DecryptString_Should_RestoreOriginalText()
    {
        // Arrange
        var originalText = "This is a test string to encrypt and decrypt";
        var encryptedText = _service.EncryptString(originalText, _testKey);

        // Act
        var decryptedText = _service.DecryptString(encryptedText, _testKey);

        // Assert
        Assert.Equal(originalText, decryptedText);
    }
}
