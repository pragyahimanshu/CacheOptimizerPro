using Application.Abstractions.Messaging;
using Domain.Entities;

namespace Application.Products.Queries.GetProducts;

/// <summary>Requests the product collection for the catalog.</summary>
public sealed record GetProductsQuery(int Count = 100) : IQuery<List<Product>>;