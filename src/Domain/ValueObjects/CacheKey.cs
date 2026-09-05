using System.Text.RegularExpressions;
using Domain.Errors;
using Domain.Primitives;
using Domain.Shared;

namespace Domain.ValueObjects;

public sealed partial class CacheKey : ValueObject
{
    #region Constructors
    
    private CacheKey(string value)
    {
        Value = value;
    }

    #endregion
    
    #region Properties
    
    public string Value { get; }
    
    #endregion
    
    #region Factory Methods

    public static Result<CacheKey> Create(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return Result.Failure<CacheKey>(
                DomainErrors.Cache.InvalidKey);

        // Example: Enforce key format (e.g., "users:123")
        var regex = KeyRegex();
        if (!regex.IsMatch(key))
            return Result.Failure<CacheKey>(
                DomainErrors.Cache.InvalidKeyFormat);

        return new CacheKey(key);
    }
    
    #endregion
    
    #region Overrides

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
    
    #endregion
    
    #region Helper Methods

    [GeneratedRegex(@"^[a-z0-9:-]+$")]
    private static partial Regex KeyRegex();
    
    #endregion
}