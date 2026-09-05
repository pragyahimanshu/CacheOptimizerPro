using Application.Abstractions.Messaging;
using Domain.Interfaces;
using Domain.Shared;

namespace Application.CacheConfig.Queries;

/// <summary>Reads cache policy through its Domain abstraction.</summary>
internal sealed class GetCacheConfigQueryHandler(ICacheConfigurationService configuration) : IQueryHandler<GetCacheConfigQuery, CacheConfigSnapshot>
{
    public Task<Result<CacheConfigSnapshot>> Handle(GetCacheConfigQuery request, CancellationToken cancellationToken)
        => Task.FromResult(Result.Success(new CacheConfigSnapshot(configuration.Expiration.AbsoluteExpiration.TotalSeconds, configuration.Strategy)));
}