using Microsoft.Extensions.Logging;
using Domain.ValueObjects;

namespace Infrastructure.Services;

public interface ICacheKeyManagementService
{
    CacheKey GenerateKey(string entityType, object id, int? version = null);
    CacheKey GenerateCompositeKey(string entityType, params object[] identifiers);
    string GetKeyPattern(string entityType);
    void InvalidateEntityKeys(string entityType, object id);
    void InvalidatePattern(string pattern);
}

public class CacheKeyManagementService : ICacheKeyManagementService
{
    private readonly ILogger<CacheKeyManagementService> _logger;
    private readonly Dictionary<string, string> _keyTemplates = [];
    private readonly Lock _lock = new();

    public CacheKeyManagementService(ILogger<CacheKeyManagementService> logger)
    {
        _logger = logger;
        InitializeKeyTemplates();
    }

    public CacheKey GenerateKey(string entityType, object id, int? version = null)
    {
        var baseKey = $"{entityType}:{id}";
        var versionedKey = version.HasValue ? $"{baseKey}:v{version}" : baseKey;
        
        _logger.LogDebug("Generated cache key: {Key}", versionedKey);
        return CacheKey.Create(versionedKey).Value;
    }

    public CacheKey GenerateCompositeKey(string entityType, params object[] identifiers)
    {
        if (identifiers == null || identifiers.Length == 0)
            throw new ArgumentException("At least one identifier is required", nameof(identifiers));

        var keyParts = new List<string> { entityType };
        keyParts.AddRange(identifiers.Select(i => i?.ToString() ?? "null"));
        
        var compositeKey = string.Join(":", keyParts);
        _logger.LogDebug("Generated composite cache key: {Key}", compositeKey);
        return CacheKey.Create(compositeKey).Value;
    }

    public string GetKeyPattern(string entityType)
    {
        return $"{entityType}:*";
    }

    public void InvalidateEntityKeys(string entityType, object id)
    {
        var pattern = $"{entityType}:{id}*";
        _logger.LogInformation("Invalidating cache keys matching pattern: {Pattern}", pattern);
        // In a real implementation, this would trigger actual cache invalidation
    }

    public void InvalidatePattern(string pattern)
    {
        _logger.LogInformation("Invalidating cache keys matching pattern: {Pattern}", pattern);
        // In a real implementation, this would trigger actual cache invalidation
    }

    private void InitializeKeyTemplates()
    {
        lock (_lock)
        {
            _keyTemplates["product"] = "products:{id}";
            _keyTemplates["user"] = "users:{id}";
            _keyTemplates["order"] = "orders:{id}";
            _keyTemplates["category"] = "categories:{id}";
        }
    }
}
