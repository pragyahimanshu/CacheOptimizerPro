using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Infrastructure.Services;

public interface ICachePartitioningService
{
    int GetPartitionIndex(string key, int totalPartitions);
    IDatabase GetPartitionDatabase(string key);
    Task<IDatabase> GetPartitionDatabaseAsync(string key);
}

public class CachePartitioningService(
    IConnectionMultiplexer redis,
    ILogger<CachePartitioningService> logger,
    int totalPartitions = 4
) : ICachePartitioningService
{
    private readonly int _totalPartitions = Math.Max(1, totalPartitions);

    public int GetPartitionIndex(string key, int totalPartitions)
    {
        if (string.IsNullOrEmpty(key))
            return 0;

        // Simple hash-based partitioning
        var hash = GetHashCode(key);
        return Math.Abs(hash) % totalPartitions;
    }

    public IDatabase GetPartitionDatabase(string key)
    {
        var partitionIndex = GetPartitionIndex(key, _totalPartitions);
        logger.LogDebug("Routing key {Key} to partition {Partition}", key, partitionIndex);
        return redis.GetDatabase(partitionIndex);
    }

    public async Task<IDatabase> GetPartitionDatabaseAsync(string key)
    {
        // For Redis, getting a database is synchronous, but we provide async method for consistency
        return await Task.FromResult(GetPartitionDatabase(key));
    }

    private static int GetHashCode(string str)
    {
        unchecked
        {
            var hash1 = 5381;
            var hash2 = 5381;
            var chars = str.ToCharArray();

            for (var i = 0; i < chars.Length; i++)
            {
                hash1 = ((hash1 << 5) + hash1) ^ chars[i];
                if (i + 1 < chars.Length)
                {
                    hash2 = ((hash2 << 5) + hash2) ^ chars[i + 1];
                    i++;
                }
            }

            return hash1 + (hash2 * 1566083941);
        }
    }
}
