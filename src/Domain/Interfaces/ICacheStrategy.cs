using Domain.Entities;
using Domain.Enums;
using Domain.Shared;
using Domain.ValueObjects;

namespace Domain.Interfaces;

public interface ICacheStrategy
{
    CacheStrategyType StrategyType { get; }

    Task<Result<CachedItem>> ReadThroughAsync(
        CacheKey key,
        Func<Task<Result<byte[]>>> fallbackDataSource,
        CacheExpiration expiration,
        CancellationToken cancellationToken);

    Task<Result> WriteThroughAsync(
        CacheKey key,
        byte[] value,
        CacheExpiration expiration,
        Func<Task<Result>> updateDataSource,
        CancellationToken cancellationToken);
}