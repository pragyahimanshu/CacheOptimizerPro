using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.Services;

public interface ISerializationService
{
    byte[] Serialize<T>(T obj);
    T? Deserialize<T>(byte[] data);
    string SerializeToString<T>(T obj);
    T? DeserializeFromString<T>(string data);
}

public class SerializationService : ISerializationService
{
    private readonly JsonSerializerOptions _options;
    private readonly ILogger<SerializationService> _logger;

    public SerializationService(ILogger<SerializationService> logger)
    {
        _logger = logger;
        _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false,
            PropertyNameCaseInsensitive = true
        };
    }

    public byte[] Serialize<T>(T obj)
    {
        if (obj == null)
            return [];

        try
        {
            return JsonSerializer.SerializeToUtf8Bytes(obj, _options);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to serialize object to bytes");
            throw;
        }
    }

    public T? Deserialize<T>(byte[] data)
    {
        if (data == null || data.Length == 0)
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(data, _options);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize bytes to object");
            throw;
        }
    }

    public string SerializeToString<T>(T obj)
    {
        if (obj == null)
            return string.Empty;

        try
        {
            return JsonSerializer.Serialize(obj, _options);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to serialize object to string");
            throw;
        }
    }

    public T? DeserializeFromString<T>(string data)
    {
        if (string.IsNullOrEmpty(data))
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(data, _options);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize string to object");
            throw;
        }
    }
}
