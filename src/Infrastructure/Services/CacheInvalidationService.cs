using Domain.Errors;
using Domain.Interfaces;
using Domain.Shared;
using Domain.ValueObjects;
using Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;

namespace Infrastructure.Services;

public sealed class CacheInvalidationService(
    IConnectionMultiplexer redis,
    IOptions<RedisSettings> settings,
    ILogger<CacheInvalidationService> logger,
    ICacheRepository cacheRepository,
    IMetricsService? metricsService = null
) : ICacheInvalidationService, IDisposable
{
    private readonly ISubscriber _subscriber = redis.GetSubscriber();
    private readonly string _invalidationChannel = settings.Value.InvalidationChannel;
    private bool _disposed = false;

    public async Task<Result> InvalidateOnDatabaseChangeAsync(
        CacheKey key,
        string databaseEntity,
        CancellationToken cancellationToken)
    {
        try
        {
            // Delete from local cache
            var result = await cacheRepository.InvalidateAsync(key, cancellationToken);
            
            if (result.IsSuccess)
            {
                metricsService?.RecordInvalidation(key.Value);
                // Publish invalidation message to other instances
                var message = new InvalidationMessage
                {
                    Key = key.Value,
                    EntityType = databaseEntity,
                    Timestamp = DateTime.UtcNow
                };

                var jsonMessage = JsonSerializer.Serialize(message);
                await _subscriber.PublishAsync(RedisChannel.Literal(_invalidationChannel), jsonMessage);
                logger.LogInformation("Cache invalidation published for key: {Key}", key.Value);
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error invalidating cache for key: {Key}", key.Value);
            return Result.Failure(DomainErrors.Cache.InvalidationFailed);
        }
    }

    public async Task<Result> SubscribeToInvalidationChannelAsync(
        string channel,
        Action<CacheKey> onInvalidation,
        CancellationToken cancellationToken)
    {
        try
        {
            await _subscriber.SubscribeAsync(RedisChannel.Literal(channel), (channel, value) =>
            {
                try
                {
                    var message = JsonSerializer.Deserialize<InvalidationMessage>(value.ToString());
                    if (message != null)
                    {
                        var cacheKey = CacheKey.Create(message.Key);
                        if (cacheKey.IsSuccess)
                        {
                            onInvalidation(cacheKey.Value);
                            logger.LogInformation("Cache invalidated for key: {Key}", message.Key);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing cache invalidation message");
                }
            });

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error subscribing to cache invalidation channel");
            return Result.Failure(Domain.Errors.DomainErrors.Cache.SubscriptionFailed(channel));
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _subscriber?.UnsubscribeAll();
            _disposed = true;
        }
    }

    private class InvalidationMessage
    {
        public string Key { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
