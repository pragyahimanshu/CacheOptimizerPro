using Domain.Shared;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Behaviors;

public sealed class CachePipelineBehavior<TRequest, TResponse>(
    ILogger<CachePipelineBehavior<TRequest, TResponse>> logger
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Caching pipeline started for {RequestType}", typeof(TRequest).Name);
        var result = await next();
        logger.LogInformation("Caching pipeline completed for {RequestType}", typeof(TRequest).Name);
        return result;
    }
}