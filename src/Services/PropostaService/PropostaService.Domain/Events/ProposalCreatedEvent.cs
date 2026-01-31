using BuildingBlocks.Domain.Primitives;

namespace PropostaService.Domain.Events;

/// <summary>
/// Domain event raised when a new proposal is created.
/// </summary>
public sealed record ProposalCreatedEvent(
    Guid ProposalId,
    string HolderCpf,
    string InsuranceType,
    decimal CoverageAmount,
    decimal PremiumAmount) : DomainEvent;
