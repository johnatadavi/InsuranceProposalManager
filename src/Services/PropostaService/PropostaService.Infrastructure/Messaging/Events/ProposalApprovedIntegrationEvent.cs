using BuildingBlocks.Infrastructure.Messaging;

namespace PropostaService.Infrastructure.Messaging.Events;

/// <summary>
/// Integration event published when a proposal is approved.
/// Consumed by ContratacaoService to allow contracting.
/// </summary>
public sealed record ProposalApprovedIntegrationEvent(
    Guid ProposalId,
    string ProposalNumber,
    string HolderCpf,
    string HolderName,
    string HolderEmail,
    string InsuranceType,
    decimal CoverageAmount,
    decimal PremiumAmount,
    string Currency,
    DateOnly CoverageStartDate,
    DateOnly CoverageEndDate) : IntegrationEvent;
