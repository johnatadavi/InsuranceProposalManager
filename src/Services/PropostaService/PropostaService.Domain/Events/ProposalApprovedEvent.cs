using BuildingBlocks.Domain.Primitives;

namespace PropostaService.Domain.Events;

/// <summary>
/// Domain event raised when a proposal is approved.
/// </summary>
public sealed record ProposalApprovedEvent(
    Guid ProposalId,
    string HolderCpf,
    string InsuranceType,
    decimal CoverageAmount,
    decimal PremiumAmount) : DomainEvent;
