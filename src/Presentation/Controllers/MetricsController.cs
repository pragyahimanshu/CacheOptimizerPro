using Application.Metrics.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

/// <summary>Receives telemetry requests and delegates them to MediatR.</summary>
[ApiController]
[Route("api/metrics")]
public sealed class MetricsController(ISender sender) : ControllerBase
{
    /// <summary>Gets live cache performance metrics.</summary>
    [HttpGet]
    public async Task<IActionResult> GetMetrics(CancellationToken cancellationToken)
        => Ok((await sender.Send(new GetMetricsQuery(), cancellationToken)).Value);
}
