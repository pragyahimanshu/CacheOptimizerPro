using Application.Abstractions.Messaging;
using Domain.Interfaces;
using Domain.Shared;

namespace Application.CacheConfig.Commands;

/// <summary>Validates and applies runtime cache policy changes.</summary>
internal sealed class UpdateCacheConfigCommandHandler(ICacheConfigurationService configuration) : ICommandHandler<UpdateCacheConfigCommand>
{
    public Task<Result> Handle(UpdateCacheConfigCommand request, CancellationToken cancellationToken)
    {
        if (request.ExpirationSeconds <= 0 || !Enum.IsDefined(request.Strategy))
            return Task.FromResult(Result.Failure(new Error("CacheConfig.Invalid", "Cache configuration is invalid.")));
        configuration.Update(TimeSpan.FromSeconds(request.ExpirationSeconds), request.Strategy);
        return Task.FromResult(Result.Success());
    }
}