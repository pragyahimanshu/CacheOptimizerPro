using Microsoft.Extensions.Logging;
using System.IO.Compression;

namespace Infrastructure.Services;

public interface ICompressionService
{
    byte[] Compress(byte[] data);
    byte[] Decompress(byte[] compressedData);
    string CompressString(string text);
    string DecompressString(string compressedText);
}

public class CompressionService(
    ILogger<CompressionService> logger
) : ICompressionService
{
    public byte[] Compress(byte[] data)
    {
        if (data == null || data.Length == 0)
            return data!;

        try
        {
            using var output = new MemoryStream();
            using (var gzip = new GZipStream(output, CompressionMode.Compress))
            {
                gzip.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to compress data, returning original data");
            return data;
        }
    }

    public byte[] Decompress(byte[] compressedData)
    {
        if (compressedData == null || compressedData.Length == 0)
            return compressedData!;

        try
        {
            using var input = new MemoryStream(compressedData);
            using var gzip = new GZipStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();
            gzip.CopyTo(output);
            return output.ToArray();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to decompress data, returning compressed data");
            return compressedData;
        }
    }

    public string CompressString(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        try
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(text);
            var compressed = Compress(bytes);
            return Convert.ToBase64String(compressed);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to compress string, returning original string");
            return text;
        }
    }

    public string DecompressString(string compressedText)
    {
        if (string.IsNullOrEmpty(compressedText))
            return compressedText;

        try
        {
            var compressed = Convert.FromBase64String(compressedText);
            var decompressed = Decompress(compressed);
            return System.Text.Encoding.UTF8.GetString(decompressed);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to decompress string, returning compressed string");
            return compressedText;
        }
    }
}
