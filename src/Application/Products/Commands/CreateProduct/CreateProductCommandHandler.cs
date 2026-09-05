using System.Text.Json;
using Application.Abstractions.Messaging;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Shared;
using Domain.ValueObjects;

namespace Application.Products.Commands.CreateProduct;

internal sealed class CreateProductCommandHandler(
    ICacheStrategy cacheStrategy,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ICacheConfigurationService? cacheConfiguration = null,
    IMetricsService? metricsService = null
) : ICommandHandler<CreateProductCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        metricsService?.RecordRequest();
        var productResult = Product.Create(
            request.Name,
            request.Description,
            request.Price,
            request.StockQuantity);

        if (productResult.IsFailure)
            return Result.Failure<Guid>(productResult.Error);

        var cacheKey = CacheKey.Create($"products:{productResult.Value.Id}").Value;

        var writeResult = await cacheStrategy.WriteThroughAsync(
            cacheKey,
            JsonSerializer.SerializeToUtf8Bytes(productResult.Value),
            cacheConfiguration?.Expiration ?? CacheExpiration.Default,
            async () =>
            {
                await productRepository.AddAsync(productResult.Value, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
                return Result.Success();
            },
            cancellationToken
        );

        return writeResult.IsFailure
            ? Result.Failure<Guid>(writeResult.Error)
            : Result.Success(productResult.Value.Id);
    }
}