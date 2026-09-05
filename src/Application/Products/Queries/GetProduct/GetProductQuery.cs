using Application.Abstractions.Messaging;
using Domain.Entities;

namespace Application.Products.Queries.GetProduct;

public sealed record GetProductQuery(Guid Id) : IQuery<Product>;