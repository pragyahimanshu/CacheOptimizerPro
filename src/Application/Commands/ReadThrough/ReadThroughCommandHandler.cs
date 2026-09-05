using Application.Abstractions.Messaging;
using Domain.Interfaces;
using Domain.Shared;
using Domain.ValueObjects;

namespace Application.Commands.ReadThrough;

internal sealed class ReadThroughCommandHandler(
    ICacheStrategy cacheStrategy
) : ICommandHandler<ReadThroughCommand, byte[]>
{
    public async Task<Result<byte[]>> Handle(
        ReadThroughCommand request,
        CancellationToken cancellationToken)
    {
        // Create CacheKey and CacheExpiration
        var cacheKeyResult = CacheKey.Create(request.Key);
        if (cacheKeyResult.IsFailure)
            return Result.Failure<byte[]>(cacheKeyResult.Error);

        var expirationResult = CacheExpiration.Create(
            request.AbsoluteExpiration,
            request.SlidingExpiration
        );
        if (expirationResult.IsFailure)
            return Result.Failure<byte[]>(expirationResult.Error);

        // Execute Cache-Aside strategy
        var cachedItemResult = await cacheStrategy.ReadThroughAsync(
            cacheKeyResult.Value,
            request.FallbackDataSource,
            expirationResult.Value,
            cancellationToken
        );

        return cachedItemResult.IsSuccess 
            ? Result.Success(cachedItemResult.Value.Value) 
            : Result.Failure<byte[]>(cachedItemResult.Error);
    }
}