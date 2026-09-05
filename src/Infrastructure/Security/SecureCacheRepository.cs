using Domain.Entities;
using Domain.Interfaces;
using Domain.Shared;
using Domain.ValueObjects;
using Infrastructure.Services;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace Infrastructure.Security;

public sealed class SecureCacheRepository(
    ICacheRepository innerRepository,
    ICacheEncryptionService encryptionService,
    IOptions<RedisSettings> settings
) : ICacheRepository
{
    private readonly string _encryptionKey = settings.Value.EncryptionKey ?? "default-encryption-key";

    public async Task<Result<CachedItem>> GetAsync(
        CacheKey key,
        CancellationToken cancellationToken)
    {
        var result = await innerRepository.GetAsync(key, cancellationToken);
        
        if (result.IsSuccess)
        {
            try
            {
                // Decrypt the data
                var decryptedData = encryptionService.Decrypt(result.Value.Value, _encryptionKey);
                var decryptedItem = CachedItem.Create(
                    result.Value.Id,
                    result.Value.Key,
                    decryptedData,
                    result.Value.Expiration);
                
                return decryptedItem;
            }
            catch (Exception)
            {
                // If decryption fails, return the original (possibly unencrypted) data
                return result;
            }
        }
        
        return result;
    }

    public async Task<Result> SetAsync(
        CachedItem item,
        CancellationToken cancellationToken)
    {
        try
        {
            // Encrypt the data before storing
            var encryptedData = encryptionService.Encrypt(item.Value, _encryptionKey);
            var encryptedItem = CachedItem.Create(
                item.Id,
                item.Key,
                encryptedData,
                item.Expiration);
            
            return await innerRepository.SetAsync(encryptedItem, cancellationToken);
        }
        catch (Exception)
        {
            // If encryption fails, store the original (unencrypted) data
            return await innerRepository.SetAsync(item, cancellationToken);
        }
    }

    public async Task<Result> InvalidateAsync(
        CacheKey key,
        CancellationToken cancellationToken)
    {
        return await innerRepository.InvalidateAsync(key, cancellationToken);
    }
}
