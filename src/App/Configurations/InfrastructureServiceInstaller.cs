using Domain.Interfaces;
using Infrastructure;
using Infrastructure.Security;
using Infrastructure.Services;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Scrutor;
using StackExchange.Redis;

namespace App.Configurations;

public class InfrastructureServiceInstaller : IServiceInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration)
    {
        services
            .Scan(
                selector => selector
                    .FromAssemblies(
                        AssemblyReference.Assembly,
                        Persistence.AssemblyReference.Assembly)
                    .AddClasses(false)
                    .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                    .AsMatchingInterface()
                    .WithScopedLifetime());
        
        // Add Redis
        var redisConnection = configuration.GetConnectionString("Redis");
        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(redisConnection!));
        
        // Register Caching dependencies
        services.AddScoped<ICacheRepository>(provider =>
        {
            var partitionedRepository = new PartitionedRedisCacheRepository(
                provider.GetRequiredService<ICachePartitioningService>(),
                provider.GetRequiredService<ICompressionService>(),
                provider.GetRequiredService<IOptions<RedisSettings>>());
                
            return new SecureCacheRepository(
                partitionedRepository,
                provider.GetRequiredService<ICacheEncryptionService>(),
                provider.GetRequiredService<IOptions<RedisSettings>>());
        });
        services.AddScoped<ICacheStrategy>(provider => 
        {
            var primaryStrategy = new CacheStrategy(
                provider.GetRequiredService<ICacheRepository>(),
                provider.GetRequiredService<CacheStatisticsService>());
                
            // For now, we'll use the same strategy as fallback, but in a real system
            // this could be a different cache provider or in-memory cache
            var fallbackStrategy = new CacheStrategy(
                provider.GetRequiredService<ICacheRepository>(),
                provider.GetRequiredService<CacheStatisticsService>());
                
            return new FallbackCacheStrategy(
                primaryStrategy,
                fallbackStrategy,
                provider.GetRequiredService<ICircuitBreakerService>());
        });
        services.AddScoped<ICacheInvalidationService, CacheInvalidationService>();
        services.AddSingleton<ICacheConfigurationService, CacheConfigurationService>();
        
        // Register cache warming service
        services.AddSingleton<IHostedService>(provider =>
            new CacheWarmingService(
                provider.GetRequiredService<IServiceScopeFactory>(),
                provider.GetRequiredService<ILogger<CacheWarmingService>>()));
        
        // Register cache statistics service
        services.AddSingleton<CacheStatisticsService>();
        services.AddSingleton<IMetricsService>(provider =>
            provider.GetRequiredService<CacheStatisticsService>());
        
        // Register compression and serialization services
        services.AddSingleton<ICompressionService, CompressionService>();
        services.AddSingleton<ISerializationService, SerializationService>();
        
        // Register cache partitioning service
        services.AddSingleton<ICachePartitioningService, CachePartitioningService>();
        
        // Register circuit breaker service
        services.AddSingleton<ICircuitBreakerService, CircuitBreakerService>();
        
        // Register cache key management service
        services.AddSingleton<ICacheKeyManagementService, CacheKeyManagementService>();
        
        // Register cache versioning service
        services.AddSingleton<ICacheVersioningService, CacheVersioningService>();
        
        // Register cache encryption service
        services.AddSingleton<ICacheEncryptionService, CacheEncryptionService>();
    }
}
