using Domain.Entities;
using Domain.Errors;
using Domain.Interfaces;
using Domain.Shared;
using Domain.ValueObjects;
using Infrastructure.Services;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Infrastructure.Security;

public sealed class PartitionedRedisCacheRepository(
    ICachePartitioningService partitioningService,
    ICompressionService compressionService,
    IOptions<RedisSettings> settings
) : ICacheRepository
{
    private readonly string _invalidationChannel = settings.Value.InvalidationChannel;

    public async Task<Result<CachedItem>> GetAsync(
        CacheKey key,
        CancellationToken cancellationToken)
    {
        var db = partitioningService.GetPartitionDatabase(key.Value);
        var redisValue = await db.StringGetAsync(key.Value);
        
        if (redisValue.IsNullOrEmpty)
            return Result.Failure<CachedItem>(DomainErrors.Cache.CacheMiss);

        try
        {
            // Decompress the data if it was compressed
            var compressedData = JsonSerializer.Deserialize<byte[]>(redisValue.ToString());
            var decompressedData = compressionService.Decompress(compressedData!);
            
            return CachedItem.Create(
                Guid.NewGuid(),
                key.Value,
                decompressedData,
                CacheExpiration.Default
            );
        }
        catch (Exception)
        {
            // Fallback to original deserialization if compression fails
            return CachedItem.Create(
                Guid.NewGuid(),
                key.Value,
                JsonSerializer.Deserialize<byte[]>(redisValue.ToString())!,
                CacheExpiration.Default
            );
        }
    }

    public async Task<Result> SetAsync(
        CachedItem item,
        CancellationToken cancellationToken)
    {
        var db = partitioningService.GetPartitionDatabase(item.Key);
        
        try
        {
            // Compress the data before storing
            var compressedData = compressionService.Compress(item.Value);
            var serializedValue = JsonSerializer.Serialize(compressedData);
            
            await db.StringSetAsync(
                item.Key,
                serializedValue,
                item.Expiration.AbsoluteExpiration
            );
            return Result.Success();
        }
        catch (Exception)
        {
            // Fallback to original serialization if compression fails
            var serializedValue = JsonSerializer.Serialize(item.Value);
            await db.StringSetAsync(
                item.Key,
                serializedValue,
                item.Expiration.AbsoluteExpiration
            );
            return Result.Success();
        }
    }

    public async Task<Result> InvalidateAsync(
        CacheKey key,
        CancellationToken cancellationToken)
    {
        try
        {
            var db = partitioningService.GetPartitionDatabase(key.Value);
            await db.KeyDeleteAsync(key.Value);
            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure(DomainErrors.Cache.InvalidationFailed);
        }
    }
}
