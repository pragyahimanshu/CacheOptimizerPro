using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repositories;

internal sealed class ProductRepository(
    ApplicationDbContext dbContext
) : IProductRepository
{
    public async Task<Product?> GetByIdAsync(
        Guid id, 
        CancellationToken cancellationToken)
    {
        return await dbContext.Set<Product>()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task AddAsync(
        Product product, 
        CancellationToken cancellationToken)
    {
        await dbContext.Set<Product>().AddAsync(product, cancellationToken);
    }

    public async Task<List<Product>> GetTopProductsAsync(
        int count, 
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<Product>()
            .OrderByDescending(p => p.CreatedOnUtc) // Most recently created first
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        dbContext.Set<Product>().Update(product);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Product product, CancellationToken cancellationToken = default)
    {
        dbContext.Set<Product>().Remove(product);
        return Task.CompletedTask;
    }
}
