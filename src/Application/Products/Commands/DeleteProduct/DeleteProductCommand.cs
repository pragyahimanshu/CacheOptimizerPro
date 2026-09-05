using Application.Abstractions.Messaging;

namespace Application.Products.Commands.DeleteProduct;

/// <summary>Deletes a product and invalidates its cache entry.</summary>
public sealed record DeleteProductCommand(Guid Id) : ICommand;