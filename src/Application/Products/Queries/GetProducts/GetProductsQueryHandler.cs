using Application.Abstractions.Messaging;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Shared;

namespace Application.Products.Queries.GetProducts;

/// <summary>Loads products through the repository abstraction.</summary>
internal sealed class GetProductsQueryHandler(
    IProductRepository productRepository,
    IMetricsService? metricsService = null)
    : IQueryHandler<GetProductsQuery, List<Product>>
{
    public async Task<Result<List<Product>>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        metricsService?.RecordRequest();
        return Result.Success(await productRepository.GetTopProductsAsync(request.Count, cancellationToken));
    }
}