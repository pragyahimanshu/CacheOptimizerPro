using System.Text.Json;
using Application.Abstractions.Messaging;
using Domain.Entities;
using Domain.Errors;
using Domain.Interfaces;
using Domain.Shared;
using Domain.ValueObjects;

namespace Application.Products.Queries.GetProduct;

internal sealed class GetProductQueryHandler(
    ICacheStrategy cacheStrategy,
    IProductRepository productRepository,
    ICacheConfigurationService? cacheConfiguration = null,
    IMetricsService? metricsService = null
) : IQueryHandler<GetProductQuery, Product>
{
    public async Task<Result<Product>> Handle(
        GetProductQuery request,
        CancellationToken cancellationToken)
    {
        metricsService?.RecordRequest();
        var cacheKey = CacheKey.Create($"products:{request.Id}").Value;

        var productResult = await cacheStrategy.ReadThroughAsync(
            cacheKey,
            async () =>
            {
                var product = await productRepository.GetByIdAsync(request.Id, cancellationToken);
                return product is null
                    ? Result.Failure<byte[]>(DomainErrors.Product.NotFound(request.Id))
                    : Result.Success(JsonSerializer.SerializeToUtf8Bytes(product));
            },
            cacheConfiguration?.Expiration ?? CacheExpiration.Default,
            cancellationToken
        );

        if (productResult.IsFailure)
            return Result.Failure<Product>(
                productResult.Error);

        return Result.Success(JsonSerializer.Deserialize<Product>(productResult.Value.Value)!);
    }
}