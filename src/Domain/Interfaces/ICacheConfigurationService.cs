using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Interfaces;

/// <summary>Provides the runtime cache configuration used by application handlers.</summary>
public interface ICacheConfigurationService
{
    CacheExpiration Expiration { get; }
    CacheStrategyType Strategy { get; }
    void Update(TimeSpan expiration, CacheStrategyType strategy);
}