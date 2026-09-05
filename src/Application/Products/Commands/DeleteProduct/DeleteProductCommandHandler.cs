using Application.Abstractions.Messaging;
using Domain.Errors;
using Domain.Interfaces;
using Domain.Shared;
using Domain.ValueObjects;

namespace Application.Products.Commands.DeleteProduct;

/// <summary>Coordinates product deletion through repository abstractions.</summary>
internal sealed class DeleteProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ICacheInvalidationService cacheInvalidationService,
    IMetricsService? metricsService = null)
    : ICommandHandler<DeleteProductCommand>
{
    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        metricsService?.RecordRequest();
        var product = await productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null) return Result.Failure(DomainErrors.Product.NotFound(request.Id));

        await productRepository.DeleteAsync(product, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        var key = CacheKey.Create($"products:{request.Id}").Value;
        return await cacheInvalidationService.InvalidateOnDatabaseChangeAsync(key, "Product", cancellationToken);
    }
}