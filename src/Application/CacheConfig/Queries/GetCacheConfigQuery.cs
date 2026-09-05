using Application.Abstractions.Messaging;
using Domain.Enums;

namespace Application.CacheConfig.Queries;

/// <summary>Reads the current runtime cache policy.</summary>
public sealed record GetCacheConfigQuery : IQuery<CacheConfigSnapshot>;

/// <summary>Application response for the current cache policy.</summary>
public sealed record CacheConfigSnapshot(double ExpirationSeconds, CacheStrategyType Strategy);