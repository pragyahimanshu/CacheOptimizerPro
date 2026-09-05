using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Domain.Shared;
using Domain.ValueObjects;
using Infrastructure.Services;

namespace Infrastructure.Security;

public sealed class CacheStrategy(
    ICacheRepository cacheRepository,
    CacheStatisticsService cacheStatistics
) : ICacheStrategy
{
    public CacheStrategyType StrategyType => CacheStrategyType.CacheAside;

    public async Task<Result<CachedItem>> ReadThroughAsync(
        CacheKey key,
        Func<Task<Result<byte[]>>> fallbackDataSource,
        CacheExpiration expiration,
        CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;

        // 1. Check cache
        var cachedItemResult = await cacheRepository.GetAsync(key, cancellationToken);
        if (cachedItemResult.IsSuccess)
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            cacheStatistics.RecordHit(key.Value, duration);
            return cachedItemResult;
        }

        // 2. Fallback to database
        var dataResult = await fallbackDataSource();
        var durationMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
        
        if (dataResult.IsFailure)
        {
            cacheStatistics.RecordMiss(key.Value, durationMs);
            return Result.Failure<CachedItem>(dataResult.Error);
        }

        // 3. Update cache
        var newItem = CachedItem.Create(
            Guid.NewGuid(),
            key.Value,
            dataResult.Value,
            expiration);
        await cacheRepository.SetAsync(newItem, cancellationToken);
        
        cacheStatistics.RecordMiss(key.Value, durationMs);
        return newItem;
    }

    public async Task<Result> WriteThroughAsync(
        CacheKey key,
        byte[] value,
        CacheExpiration expiration,
        Func<Task<Result>> updateDataSource,
        CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;

        // 1. Update database
        var updateResult = await updateDataSource();
        if (updateResult.IsFailure)
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            cacheStatistics.RecordMiss(key.Value, duration);
            return updateResult;
        }

        // 2. Update cache
        var item = CachedItem.Create(
            Guid.NewGuid(),
            key.Value,
            value,
            expiration);
        var setResult = await cacheRepository.SetAsync(item, cancellationToken);
        
        var durationMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
        cacheStatistics.RecordHit(key.Value, durationMs);
        
        return setResult;
    }
}
