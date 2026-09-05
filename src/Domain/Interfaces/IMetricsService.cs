namespace Domain.Interfaces;

/// <summary>Provides cache telemetry without exposing its infrastructure implementation.</summary>
public interface IMetricsService
{
    /// <summary>Records one handled API request.</summary>
    void RecordRequest();

    /// <summary>Records a successful cache invalidation.</summary>
    void RecordInvalidation(string cacheKey);

    MetricsSnapshot GetSnapshot();
}

/// <summary>Read-only cache telemetry returned by the application layer.</summary>
public sealed record MetricsSnapshot(
    long Hits,
    long Misses,
    long Invalidations,
    long TotalRequests,
    long TotalOperations,
    double HitRate,
    double AverageHitResponseTimeMs,
    double AverageMissResponseTimeMs);