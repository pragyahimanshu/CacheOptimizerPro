using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Domain.Shared;
using Domain.ValueObjects;
using Infrastructure.Services;

namespace Infrastructure.Security;

public sealed class FallbackCacheStrategy(
    ICacheStrategy primaryStrategy,
    ICacheStrategy fallbackStrategy,
    ICircuitBreakerService circuitBreaker
) : ICacheStrategy
{
    public CacheStrategyType StrategyType => CacheStrategyType.CacheAside;

    public async Task<Result<CachedItem>> ReadThroughAsync(
        CacheKey key,
        Func<Task<Result<byte[]>>> fallbackDataSource,
        CacheExpiration expiration,
        CancellationToken cancellationToken)
    {
        return await circuitBreaker.ExecuteAsync(
            $"read:{key.Value}",
            async () => await primaryStrategy.ReadThroughAsync(key, fallbackDataSource, expiration, cancellationToken),
            async () => await fallbackStrategy.ReadThroughAsync(key, fallbackDataSource, expiration, cancellationToken));
    }

    public async Task<Result> WriteThroughAsync(
        CacheKey key,
        byte[] value,
        CacheExpiration expiration,
        Func<Task<Result>> updateDataSource,
        CancellationToken cancellationToken)
    {
        return await circuitBreaker.ExecuteAsync(
            $"write:{key.Value}",
            async () => await primaryStrategy.WriteThroughAsync(key, value, expiration, updateDataSource, cancellationToken),
            async () => await fallbackStrategy.WriteThroughAsync(key, value, expiration, updateDataSource, cancellationToken));
    }
}
