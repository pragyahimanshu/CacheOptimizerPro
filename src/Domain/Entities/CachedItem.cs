using Domain.Events;
using Domain.Primitives;
using Domain.ValueObjects;

namespace Domain.Entities;

public sealed class CachedItem : AggregateRoot, IAuditableEntity
{
    #region Constructors

    private CachedItem(
        Guid id,
        string key,
        byte[] value,
        CacheExpiration cacheExpiration) : base(id)
    {
        Key = key;
        Value = value;
        Expiration = cacheExpiration;
    }

    #endregion

    #region Properties
    
    public string Key { get; private set; }
    public byte[] Value { get; private set; }
    public CacheExpiration Expiration { get; private set; }

    public DateTime CreatedOnUtc { get; set; }
    public DateTime? ModifiedOnUtc { get; set; }

    #endregion

    #region Factory Methods

    public static CachedItem Create(
        Guid id,
        string key,
        byte[] value,
        CacheExpiration cacheExpiration)
    {
        var cachedItem = new CachedItem(
            id,
            key,
            value,
            cacheExpiration);

        cachedItem.RaiseDomainEvent(new CachedItemCreatedDomainEvent(
            Guid.NewGuid(),
            cachedItem.Id));

        return cachedItem;
    }

    #endregion
}