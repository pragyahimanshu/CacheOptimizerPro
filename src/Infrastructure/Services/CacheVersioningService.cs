namespace Infrastructure.Services;

public interface ICacheVersioningService
{
    int GetCurrentVersion(string entityType);
    void IncrementVersion(string entityType);
    string GetVersionedKey(string baseKey, string entityType);
}

public class CacheVersioningService : ICacheVersioningService
{
    private readonly Dictionary<string, int> _versions = [];
    private readonly Lock _lock = new();

    public int GetCurrentVersion(string entityType)
    {
        lock (_lock)
        {
            if (!_versions.TryGetValue(entityType, out int value))
            {
                value = 1;
                _versions[entityType] = value;
            }
            return value;
        }
    }

    public void IncrementVersion(string entityType)
    {
        lock (_lock)
        {
            if (!_versions.ContainsKey(entityType))
            {
                _versions[entityType] = 1;
            }
            else
            {
                _versions[entityType]++;
            }
        }
    }

    public string GetVersionedKey(string baseKey, string entityType)
    {
        var version = GetCurrentVersion(entityType);
        return $"{baseKey}:v{version}";
    }
}
