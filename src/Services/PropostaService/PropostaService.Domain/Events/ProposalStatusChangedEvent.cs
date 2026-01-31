using BuildingBlocks.Domain.Primitives;

namespace PropostaService.Domain.Events;

/// <summary>
/// Domain event raised when a proposal status changes.
/// </summary>
public sealed record ProposalStatusChangedEvent(
    Guid ProposalId,
    string OldStatus,
    string NewStatus) : DomainEvent;
