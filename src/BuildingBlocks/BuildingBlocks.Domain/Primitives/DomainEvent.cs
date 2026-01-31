namespace BuildingBlocks.Domain.Primitives;

/// <summary>
/// Base record for domain events providing common properties.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
