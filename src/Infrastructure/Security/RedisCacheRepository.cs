using Domain.Entities;
using Domain.Errors;
using Domain.Interfaces;
using Domain.Shared;
using Domain.ValueObjects;
using Infrastructure.Services;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;

namespace Infrastructure.Security;

public sealed class RedisCacheRepository(
    IConnectionMultiplexer redis,
    IOptions<RedisSettings> settings,
    ICompressionService compressionService,
    ISerializationService serializationService
) : ICacheRepository
{
    private readonly IDatabase _redisDb = redis.GetDatabase();
    private readonly string _invalidationChannel = settings.Value.InvalidationChannel;
    private readonly ICompressionService _compressionService = compressionService;
    private readonly ISerializationService _serializationService = serializationService;

    public async Task<Result<CachedItem>> GetAsync(
        CacheKey key, 
        CancellationToken cancellationToken)
    {
        var redisValue = await _redisDb.StringGetAsync(key.Value);
        if (redisValue.IsNullOrEmpty)
            return Result.Failure<CachedItem>(
                DomainErrors.Cache.CacheMiss);

        try
        {
            // Decompress the data if it was compressed
            var compressedData = JsonSerializer.Deserialize<byte[]>(redisValue.ToString());
            var decompressedData = _compressionService.Decompress(compressedData!);
            
            return CachedItem.Create(
                Guid.NewGuid(),
                key.Value,
                decompressedData,
                CacheExpiration.Default // Replace with actual expiration from Redis
            );
        }
        catch (Exception)
        {
            // Fallback to original deserialization if compression fails
            return CachedItem.Create(
                Guid.NewGuid(),
                key.Value,
                JsonSerializer.Deserialize<byte[]>(redisValue.ToString())!,
                CacheExpiration.Default // Replace with actual expiration from Redis
            );
        }
    }

    public async Task<Result> SetAsync(
        CachedItem item, 
        CancellationToken cancellationToken)
    {
        try
        {
            // Compress the data before storing
            var compressedData = _compressionService.Compress(item.Value);
            var serializedValue = JsonSerializer.Serialize(compressedData);
            
            await _redisDb.StringSetAsync(
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
            await _redisDb.StringSetAsync(
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
            await _redisDb.KeyDeleteAsync(key.Value);
            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure(Domain.Errors.DomainErrors.Cache.InvalidationFailed);
        }
    }
}
