using Microsoft.Extensions.Logging;
using Domain.Interfaces;
using System.Collections.Concurrent;
using System.Diagnostics.Metrics;

namespace Infrastructure.Services;

public class CacheStatisticsService : IMetricsService, IDisposable
{
    private readonly Meter _meter;
    private readonly Counter<long> _cacheHits;
    private readonly Counter<long> _cacheMisses;
    private readonly Counter<long> _cacheInvalidations;
    private readonly Histogram<double> _cacheOperationDuration;
    private readonly UpDownCounter<long> _currentCacheSize;
    
    private readonly ConcurrentDictionary<string, long> _operationCounts = new();
    private long _hitDurationTicks;
    private long _missDurationTicks;
    private long _totalRequests;
    private readonly Timer _metricsTimer;
    private readonly ILogger<CacheStatisticsService> _logger;
    private bool _disposed = false;

    public CacheStatisticsService(ILogger<CacheStatisticsService> logger)
    {
        _logger = logger;
        _meter = new Meter("DistributedCachingSystem.CacheMetrics", "1.0.0");
        
        _cacheHits = _meter.CreateCounter<long>(
            "cache.hits",
            description: "Number of cache hits");
            
        _cacheMisses = _meter.CreateCounter<long>(
            "cache.misses",
            description: "Number of cache misses");
            
        _cacheInvalidations = _meter.CreateCounter<long>(
            "cache.invalidations",
            description: "Number of cache invalidations");
            
        _cacheOperationDuration = _meter.CreateHistogram<double>(
            "cache.operation.duration",
            unit: "ms",
            description: "Cache operation duration in milliseconds");
            
        _currentCacheSize = _meter.CreateUpDownCounter<long>(
            "cache.size",
            description: "Current cache size in items");

        // Periodically log metrics
        _metricsTimer = new Timer(LogMetrics, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    public void RecordHit(string cacheKey, double durationMs)
    {
        _cacheHits.Add(1);
        _cacheOperationDuration.Record(durationMs);
        _operationCounts.AddOrUpdate("hits", 1, (_, count) => count + 1);
        Interlocked.Add(ref _hitDurationTicks, (long)(durationMs * TimeSpan.TicksPerMillisecond));
        
        _logger.LogDebug("Cache hit for key {CacheKey} in {DurationMs}ms", cacheKey, durationMs);
    }

    /// <inheritdoc />
    public void RecordRequest()
    {
        Interlocked.Increment(ref _totalRequests);
    }

    public void RecordMiss(string cacheKey, double durationMs)
    {
        _cacheMisses.Add(1);
        _cacheOperationDuration.Record(durationMs);
        _operationCounts.AddOrUpdate("misses", 1, (_, count) => count + 1);
        Interlocked.Add(ref _missDurationTicks, (long)(durationMs * TimeSpan.TicksPerMillisecond));
        
        _logger.LogDebug("Cache miss for key {CacheKey} in {DurationMs}ms", cacheKey, durationMs);
    }

    public void RecordInvalidation(string cacheKey)
    {
        _cacheInvalidations.Add(1);
        _operationCounts.AddOrUpdate("invalidations", 1, (_, count) => count + 1);
        
        _logger.LogDebug("Cache invalidated for key {CacheKey}", cacheKey);
    }

    public void UpdateCacheSize(long size)
    {
        _currentCacheSize.Add(size);
        _logger.LogDebug("Cache size updated to {Size} items", size);
    }

    public CacheMetrics GetMetrics()
    {
        var hits = _operationCounts.GetValueOrDefault("hits", 0);
        var misses = _operationCounts.GetValueOrDefault("misses", 0);
        var invalidations = _operationCounts.GetValueOrDefault("invalidations", 0);
        
        var totalOperations = hits + misses;
        var hitRate = totalOperations > 0 ? (double)hits / totalOperations * 100 : 0;
        
        return new CacheMetrics
        {
            Hits = hits,
            Misses = misses,
            Invalidations = invalidations,
            HitRate = hitRate,
            TotalOperations = totalOperations,
            AverageHitResponseTimeMs = hits > 0
                ? TimeSpan.FromTicks(Interlocked.Read(ref _hitDurationTicks)).TotalMilliseconds / hits
                : 0,
            AverageMissResponseTimeMs = misses > 0
                ? TimeSpan.FromTicks(Interlocked.Read(ref _missDurationTicks)).TotalMilliseconds / misses
                : 0
        };
    }

    /// <inheritdoc />
    public MetricsSnapshot GetSnapshot()
    {
        var metrics = GetMetrics();
        return new MetricsSnapshot(
            metrics.Hits,
            metrics.Misses,
            metrics.Invalidations,
            Interlocked.Read(ref _totalRequests),
            metrics.TotalOperations,
            metrics.HitRate,
            metrics.AverageHitResponseTimeMs,
            metrics.AverageMissResponseTimeMs);
    }

    private void LogMetrics(object? state)
    {
        try
        {
            var metrics = GetMetrics();
            _logger.LogInformation(
                "Cache Metrics - Hits: {Hits}, Misses: {Misses}, Hit Rate: {HitRate:P2}, Invalidations: {Invalidations}",
                metrics.Hits,
                metrics.Misses,
                metrics.HitRate,
                metrics.Invalidations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging cache metrics");
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _metricsTimer?.Dispose();
            _meter?.Dispose();
            _disposed = true;
        }
    }
}

public class CacheMetrics
{
    public long Hits { get; set; }
    public long Misses { get; set; }
    public long Invalidations { get; set; }
    public double HitRate { get; set; }
    public long TotalOperations { get; set; }
    public double AverageHitResponseTimeMs { get; set; }
    public double AverageMissResponseTimeMs { get; set; }
}
