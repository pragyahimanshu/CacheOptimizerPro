using Application.Abstractions.Messaging;
using Domain.Enums;

namespace Application.CacheConfig.Commands;

/// <summary>Changes cache policy at runtime.</summary>
public sealed record UpdateCacheConfigCommand(int ExpirationSeconds, CacheStrategyType Strategy) : ICommand;