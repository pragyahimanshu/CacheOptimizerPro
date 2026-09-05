using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Security.Cryptography;

namespace Infrastructure.Services;

public class DistributedLockService(
    IConnectionMultiplexer redis,
    ILogger<DistributedLockService> logger
)
{
    public async Task<LockHandle?> AcquireLockAsync(
        string resourceKey,
        TimeSpan expiryTime,
        TimeSpan waitTime,
        CancellationToken cancellationToken = default)
    {
        var db = redis.GetDatabase();
        var lockKey = $"lock:{resourceKey}";
        
        // Generate a unique lock value to prevent releasing someone else's lock
        var lockValue = GenerateUniqueLockValue();
        
        var startTime = DateTime.UtcNow;
        var retryDelay = TimeSpan.FromMilliseconds(50); // Start with 50ms delay

        while (DateTime.UtcNow - startTime < waitTime)
        {
            // Try to acquire the lock using SET with NX (only set if not exists) and EX (expire time)
            var acquired = await db.StringSetAsync(
                lockKey,
                lockValue,
                expiryTime,
                When.NotExists);

            if (acquired)
            {
                logger.LogDebug("Acquired distributed lock for resource {ResourceKey}", resourceKey);
                return new LockHandle(lockKey, lockValue, db);
            }

            // Wait before retrying with exponential backoff
            await Task.Delay(retryDelay, cancellationToken);
            retryDelay = TimeSpan.FromMilliseconds(Math.Min(retryDelay.TotalMilliseconds * 1.5, 500)); // Max 500ms
        }

        logger.LogWarning("Failed to acquire distributed lock for resource {ResourceKey} within timeout", resourceKey);
        return null;
    }

    private string GenerateUniqueLockValue()
    {
        // Generate a unique identifier for this lock instance
        var machineName = Environment.MachineName;
        var processId = Environment.ProcessId;
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var random = RandomNumberGenerator.GetBytes(8);
        
        var value = $"{machineName}:{processId}:{timestamp}:{Convert.ToBase64String(random)}";
        return value;
    }
}

public class LockHandle : IDisposable
{
    private readonly string _lockKey;
    private readonly string _lockValue;
    private readonly IDatabase _db;
    private bool _disposed = false;

    internal LockHandle(string lockKey, string lockValue, IDatabase db)
    {
        _lockKey = lockKey;
        _lockValue = lockValue;
        _db = db;
    }

    public async Task<bool> ExtendAsync(TimeSpan additionalTime)
    {
        // Extend the lock by checking if we still own it and updating the expiry
        var script = @"
            if redis.call('get', KEYS[1]) == ARGV[1] then
                return redis.call('expire', KEYS[1], ARGV[2])
            else
                return 0
            end";

        var result = await _db.ScriptEvaluateAsync(
            script,
            [_lockKey],
            [_lockValue, (int)additionalTime.TotalSeconds]);

        return (int)result == 1;
    }

    public async Task ReleaseAsync()
    {
        if (_disposed) return;

        // Release the lock only if we still own it
        var script = @"
            if redis.call('get', KEYS[1]) == ARGV[1] then
                return redis.call('del', KEYS[1])
            else
                return 0
            end";

        await _db.ScriptEvaluateAsync(
            script,
            [_lockKey],
            [_lockValue]);

        _disposed = true;
    }

    public void Dispose()
    {
        // Best effort release in case of disposal
        try
        {
            ReleaseAsync().Wait(1000); // Wait up to 1 second
        }
        catch
        {
            // Ignore exceptions during disposal
        }
        _disposed = true;
    }
}
