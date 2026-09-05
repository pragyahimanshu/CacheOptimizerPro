using Application.Abstractions.Messaging;
using Domain.Interfaces;

namespace Application.Metrics.Queries;

/// <summary>Requests current cache telemetry.</summary>
public sealed record GetMetricsQuery : IQuery<MetricsSnapshot>;