using Application.Abstractions.Messaging;
using Domain.Interfaces;
using Domain.Shared;

namespace Application.Metrics.Queries;

/// <summary>Reads telemetry through the application-facing metrics contract.</summary>
internal sealed class GetMetricsQueryHandler(IMetricsService metricsService) : IQueryHandler<GetMetricsQuery, MetricsSnapshot>
{
    public Task<Result<MetricsSnapshot>> Handle(GetMetricsQuery request, CancellationToken cancellationToken)
        => Task.FromResult(Result.Success(metricsService.GetSnapshot()));
}