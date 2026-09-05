using Domain.Shared;
using Domain.ValueObjects;

namespace Domain.Interfaces;

public interface ICacheInvalidationService
{
    Task<Result> InvalidateOnDatabaseChangeAsync(
        CacheKey key,
        string databaseEntity,
        CancellationToken cancellationToken);

    Task<Result> SubscribeToInvalidationChannelAsync(
        string channel,
        Action<CacheKey> onInvalidation,
        CancellationToken cancellationToken);
}