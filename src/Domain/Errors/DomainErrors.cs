using Domain.Shared;

namespace Domain.Errors;

public static class DomainErrors
{
    #region User
    
    #region Entities
    
    public static class Product
    {
        public static readonly Error InvalidName = new(
            "Product.InvalidName", "Product name is invalid.");

        public static readonly Error InvalidPrice = new(
            "Product.InvalidPrice", "Product price must be greater than zero.");

        public static readonly Error InvalidStockQuantity = new(
            "Product.InvalidStockQuantity", "Stock quantity cannot be negative.");
        
        public static readonly Func<Guid, Error> NotFound = id => new Error(
            "Product.NotFound",
            $"The Product with the identifier {id} was not found.");
    }
    
    public static class Cache
    {
        public static readonly Error InvalidKey = new(
            "Cache.InvalidKey",
            "Cache key is invalid.");
        
        public static readonly Error InvalidExpiration = new(
            "Cache.InvalidExpiration",
            "Cache expiration is invalid.");
        
        public static readonly Error InvalidKeyFormat = new(
            "Cache.InvalidKeyFormat",
            "Cache key must match format [a-z0-9:-].");
        
        public static readonly Error CacheMiss = new(
            "Cache.Miss",
            "The requested item is not in the cache.");
        
        public static readonly Error InvalidationFailed = new(
            "Cache.InvalidationFailed", 
            "Failed to invalidate cache item.");

        public static readonly Func<string, Error> SubscriptionFailed = channel => new Error(
            "Cache.SubscriptionFailed",
            $"Failed to subscribe to cache invalidation channel: {channel}.");
    }

    #endregion

    #region Value Objects

    public static class RateLimit
    {
        public static readonly Func<int, Error> InvalidMaxRequests = maxRequests => new Error(
            "RateLimit.InvalidMaxRequests",
            $"MaxRequests {maxRequests} must be greater than zero.");
        
        public static readonly Func<TimeSpan, Error> InvalidTimeWindow = timeWindow => new Error(
            "RateLimit.InvalidTimeWindow",
            $"TimeWindow {timeWindow} must be greater than zero.");
        
        public static readonly Error Exceeded = new Error(
            "RateLimit.Exceeded",
            "Too many requests. Please try again later.");
    }
    
    #endregion
    
    #endregion
}