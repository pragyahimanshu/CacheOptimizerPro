namespace Domain.Events;

public sealed record CachedItemCreatedDomainEvent(
    Guid Id,
    Guid CachedItemId) : DomainEvent(Id);