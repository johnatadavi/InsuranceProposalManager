using BuildingBlocks.Domain.Primitives;

namespace ContratacaoService.Domain.Events;

/// <summary>
/// Domain event raised when a contract is created.
/// </summary>
public sealed record ContractCreatedEvent(
    Guid ContractId,
    Guid ProposalId,
    string ContractNumber,
    DateTime ContractedAt) : DomainEvent;
