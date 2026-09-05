using Application.Abstractions.Messaging;
using Domain.Interfaces;
using Domain.Shared;
using Domain.ValueObjects;

namespace Application.Commands.WriteThrough;

internal sealed class WriteThroughCommandHandler(
    ICacheStrategy cacheStrategy
) : ICommandHandler<WriteThroughCommand>
{
    public async Task<Result> Handle(
        WriteThroughCommand request,
        CancellationToken cancellationToken)
    {
        // Create CacheKey and CacheExpiration
        var cacheKeyResult = CacheKey.Create(request.Key);
        if (cacheKeyResult.IsFailure)
            return Result.Failure(cacheKeyResult.Error);

        var expirationResult = CacheExpiration.Create(
            request.AbsoluteExpiration,
            request.SlidingExpiration
        );
        if (expirationResult.IsFailure)
            return Result.Failure(expirationResult.Error);

        // Execute Write-Through strategy
        return await cacheStrategy.WriteThroughAsync(
            cacheKeyResult.Value,
            request.Value,
            expirationResult.Value,
            request.UpdateDataSource,
            cancellationToken
        );
    }
}