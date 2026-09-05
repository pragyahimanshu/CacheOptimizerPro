using Application.Products.Commands.CreateProduct;
using Application.Products.Commands.DeleteProduct;
using Application.Products.Commands.UpdateProduct;
using Application.Products.Queries.GetProduct;
using Application.Products.Queries.GetProducts;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Presentation.Abstractions;

namespace Presentation.Controllers;

/// <summary>Receives product HTTP requests and delegates them to MediatR.</summary>
[ApiController]
[Route("api/products")]
public sealed class ProductsController(ISender sender) : ApiController(sender)
{
    /// <summary>Gets the product catalog.</summary>
    [HttpGet]
    public async Task<IActionResult> GetProducts(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetProductsQuery(), cancellationToken);
        return Ok(result.Value);
    }

    /// <summary>Creates a product.</summary>
    [HttpPost]
    public async Task<IActionResult> CreateProduct(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(request, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>Gets a product by identifier.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProduct(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetProductQuery(id), cancellationToken);
        return result.IsSuccess ? Ok(result) : NotFound();
    }

    /// <summary>Updates a product through its application command.</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateProduct(Guid id, ProductRequest request, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            new UpdateProductCommand(id, request.Name, request.Description, request.Price, request.StockQuantity),
            cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    /// <summary>Deletes a product through its application command.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteProduct(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new DeleteProductCommand(id), cancellationToken);
        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }
}

/// <summary>HTTP payload for product updates.</summary>
public sealed record ProductRequest(string Name, string Description, decimal Price, int StockQuantity);
