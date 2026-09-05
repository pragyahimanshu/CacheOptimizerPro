using Domain.Interfaces;
using Domain.Shared;
using Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Infrastructure.Services;

public class CacheWarmingService : IHostedService
{
    private readonly IServiceScopeFactory? _scopeFactory;
    private readonly ILogger<CacheWarmingService> _logger;
    private readonly IProductRepository? _productRepository;
    private readonly ICacheStrategy? _cacheStrategy;
    private readonly Timer? _timer;
    private readonly TimeSpan _warmupInterval = TimeSpan.FromHours(1); // Warm up cache every hour

    [ActivatorUtilitiesConstructor]
    public CacheWarmingService(
        IServiceScopeFactory scopeFactory,
        ILogger<CacheWarmingService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _timer = new Timer(async _ => await WarmupCacheAsync(), null, Timeout.Infinite, Timeout.Infinite);
    }

    public CacheWarmingService(
        IProductRepository productRepository,
        ICacheStrategy cacheStrategy,
        ILogger<CacheWarmingService> logger)
    {
        _scopeFactory = null;
        _productRepository = productRepository;
        _cacheStrategy = cacheStrategy;
        _logger = logger;
        _timer = new Timer(async _ => await WarmupCacheAsync(), null, Timeout.Infinite, Timeout.Infinite);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cache warming service starting");
        
        // Initial warmup
        await WarmupCacheAsync();
        
        // Schedule periodic warmup
        _timer?.Change(_warmupInterval, _warmupInterval);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cache warming service stopping");
        _timer?.Change(Timeout.Infinite, Timeout.Infinite);
        return Task.CompletedTask;
    }

    private async Task WarmupCacheAsync()
    {
        try
        {
            _logger.LogInformation("Starting cache warmup process");

            using var scope = _scopeFactory?.CreateScope();
            var productRepository = _productRepository ?? scope!.ServiceProvider.GetRequiredService<IProductRepository>();

            // Get top N products to warm up (in a real system, this would be more sophisticated)
            var products = await productRepository.GetTopProductsAsync(100); // Assuming this method exists

            foreach (var product in products)
            {
                var cacheKey = CacheKey.Create($"products:{product.Id}").Value;

                var cacheStrategy = _cacheStrategy ?? scope!.ServiceProvider.GetRequiredService<ICacheStrategy>();
                // Preload product into cache
                var result = await cacheStrategy.ReadThroughAsync(
                    cacheKey,
                    async () =>
                    {
                        return await Task.FromResult(
                            Result.Success(JsonSerializer.SerializeToUtf8Bytes(product))
                        );
                    },
                    CacheExpiration.Create(TimeSpan.FromHours(2)).Value, // 2 hour expiration
                    CancellationToken.None);

                if (result.IsFailure)
                {
                    _logger.LogWarning("Failed to warm up cache for product {ProductId}: {Error}",
                        product.Id, result.Error.Message);
                }
            }

            _logger.LogInformation("Cache warmup completed for {ProductCount} products", products.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during cache warmup process");
        }
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
