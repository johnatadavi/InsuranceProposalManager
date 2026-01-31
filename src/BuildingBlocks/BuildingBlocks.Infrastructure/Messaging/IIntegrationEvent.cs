namespace BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// Base interface for integration events that cross service boundaries.
/// </summary>
public interface IIntegrationEvent
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}
