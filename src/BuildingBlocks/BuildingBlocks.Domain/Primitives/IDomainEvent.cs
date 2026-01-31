using MediatR;

namespace BuildingBlocks.Domain.Primitives;

/// <summary>
/// Marker interface for domain events.
/// Domain events capture something that happened in the domain.
/// </summary>
public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}
