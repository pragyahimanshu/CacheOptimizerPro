namespace Infrastructure.Settings;

public sealed class RedisSettings
{
    public string ConnectionString { get; set; } = null!;
    public string InvalidationChannel { get; set; } = "cache-invalidation";
    public string? EncryptionKey { get; set; }
}
