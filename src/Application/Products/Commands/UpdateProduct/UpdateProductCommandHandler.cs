using Application.Abstractions.Messaging;
using Domain.Entities;
using Domain.Errors;
using Domain.Interfaces;
using Domain.Shared;
using Domain.ValueObjects;

namespace Application.Products.Commands.UpdateProduct;

/// <summary>Applies product business rules, persistence, and cache invalidation.</summary>
internal sealed class UpdateProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ICacheInvalidationService cacheInvalidationService,
    IMetricsService? metricsService = null)
    : ICommandHandler<UpdateProductCommand, Product>
{
    public async Task<Result<Product>> Handle(
        UpdateProductCommand request,
        CancellationToken cancellationToken)
    {
        metricsService?.RecordRequest();
        var product = await productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null) return Result.Failure<Product>(DomainErrors.Product.NotFound(request.Id));

        var updateResult = product.Update(request.Name, request.Description, request.Price, request.StockQuantity);
        if (updateResult.IsFailure) return Result.Failure<Product>(updateResult.Error);

        await productRepository.UpdateAsync(product, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        var key = CacheKey.Create($"products:{request.Id}").Value;
        await cacheInvalidationService.InvalidateOnDatabaseChangeAsync(key, "Product", cancellationToken);
        return Result.Success(product);
    }
}