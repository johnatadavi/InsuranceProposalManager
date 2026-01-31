using BuildingBlocks.Infrastructure.Messaging;

namespace ContratacaoService.Infrastructure.Messaging.Events;

/// <summary>
/// Integration event published when a contract is created.
/// </summary>
public sealed record ContractCreatedIntegrationEvent(
    Guid ContractId,
    Guid ProposalId,
    string ContractNumber,
    DateTime ContractedAt) : IntegrationEvent;
