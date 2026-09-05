using Domain.Entities;
using Domain.Shared;
using Domain.ValueObjects;

namespace Domain.Interfaces;

public interface ICacheRepository
{
    Task<Result<CachedItem>> GetAsync(CacheKey key, CancellationToken cancellationToken);
    Task<Result> SetAsync(CachedItem item, CancellationToken cancellationToken);
    Task<Result> InvalidateAsync(CacheKey key, CancellationToken cancellationToken);
}