using Application.CacheConfig.Commands;
using Application.CacheConfig.Queries;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

/// <summary>Receives cache policy requests and delegates them to MediatR.</summary>
[ApiController]
[Route("api/cache-config")]
public sealed class CacheConfigController(ISender sender) : ControllerBase
{
    /// <summary>Updates cache expiration and strategy.</summary>
    [HttpPost]
    public async Task<IActionResult> Update(CacheConfigRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateCacheConfigCommand(request.ExpirationSeconds, request.Strategy), cancellationToken);
        return result.IsSuccess ? Ok(request) : BadRequest(result.Error);
    }

    /// <summary>Gets the current cache configuration.</summary>
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
        => Ok((await sender.Send(new GetCacheConfigQuery(), cancellationToken)).Value);
}

/// <summary>Request payload for runtime cache configuration.</summary>
public sealed record CacheConfigRequest(int ExpirationSeconds, CacheStrategyType Strategy);
