using Application.Abstractions.Messaging;
using Domain.Entities;

namespace Application.Products.Commands.UpdateProduct;

/// <summary>Updates a product and invalidates its cache entry.</summary>
public sealed record UpdateProductCommand(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity) : ICommand<Product>;