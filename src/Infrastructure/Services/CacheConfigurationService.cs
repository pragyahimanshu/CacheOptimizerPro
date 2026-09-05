using System.Threading;
using Domain.Enums;
using Domain.Interfaces;
using Domain.ValueObjects;

namespace Infrastructure.Services;

/// <summary>Stores cache strategy settings safely for the lifetime of the process.</summary>
public sealed class CacheConfigurationService : ICacheConfigurationService
{
    private long _expirationTicks = CacheExpiration.Default.AbsoluteExpiration.Ticks;
    private int _strategy = (int)CacheStrategyType.CacheAside;

    /// <inheritdoc />
    public CacheExpiration Expiration => CacheExpiration.Create(
        TimeSpan.FromTicks(Interlocked.Read(ref _expirationTicks))).Value;

    /// <inheritdoc />
    public CacheStrategyType Strategy => (CacheStrategyType)Volatile.Read(ref _strategy);

    /// <inheritdoc />
    public void Update(TimeSpan expiration, CacheStrategyType strategy)
    {
        Interlocked.Exchange(ref _expirationTicks, expiration.Ticks);
        Volatile.Write(ref _strategy, (int)strategy);
    }
}