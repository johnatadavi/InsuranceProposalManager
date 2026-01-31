using BuildingBlocks.Infrastructure.Messaging;

namespace ContratacaoService.Infrastructure.Messaging.Events;

/// <summary>
/// Integration event received when a proposal is approved.
/// This is the same as ProposalApprovedIntegrationEvent but defined here to avoid coupling.
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
