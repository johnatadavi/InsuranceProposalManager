using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using PropostaService.Application.Mappers;
using PropostaService.Domain.Events;
using PropostaService.Domain.Repositories;
using PropostaService.Infrastructure.Messaging.Events;

namespace PropostaService.Infrastructure.Messaging.EventHandlers;

/// <summary>
/// Handler that publishes integration event when a proposal is approved.
/// </summary>
public sealed class ProposalApprovedEventHandler : INotificationHandler<ProposalApprovedEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IProposalRepository _proposalRepository;
    private readonly ILogger<ProposalApprovedEventHandler> _logger;

    public ProposalApprovedEventHandler(
        IPublishEndpoint publishEndpoint,
        IProposalRepository proposalRepository,
        ILogger<ProposalApprovedEventHandler> logger)
    {
        _publishEndpoint = publishEndpoint;
        _proposalRepository = proposalRepository;
        _logger = logger;
    }

    public async Task Handle(ProposalApprovedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Publishing ProposalApprovedIntegrationEvent for proposal {ProposalId}",
            notification.ProposalId);

        var proposal = await _proposalRepository.GetByIdAsync(notification.ProposalId, cancellationToken);

        if (proposal is null)
        {
            _logger.LogWarning("Proposal {ProposalId} not found when publishing integration event", notification.ProposalId);
            return;
        }

        var integrationEvent = new ProposalApprovedIntegrationEvent(
            proposal.Id,
            proposal.ProposalNumber,
            proposal.HolderCpf.Value,
            proposal.HolderName,
            proposal.HolderEmail.Value,
            proposal.InsuranceType.ToString(),
            proposal.CoverageAmount.Amount,
            proposal.PremiumAmount.Amount,
            proposal.CoverageAmount.Currency,
            proposal.CoveragePeriod.StartDate,
            proposal.CoveragePeriod.EndDate);

        await _publishEndpoint.Publish(integrationEvent, cancellationToken);

        _logger.LogInformation(
            "Successfully published ProposalApprovedIntegrationEvent for proposal {ProposalId}",
            notification.ProposalId);
    }
}
