using ContratacaoService.Infrastructure.Messaging.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ContratacaoService.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumer for ProposalApprovedIntegrationEvent.
/// Can be used to pre-cache approved proposals or trigger automatic contracting workflows.
/// </summary>
public sealed class ProposalApprovedConsumer : IConsumer<ProposalApprovedIntegrationEvent>
{
    private readonly ILogger<ProposalApprovedConsumer> _logger;

    public ProposalApprovedConsumer(ILogger<ProposalApprovedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<ProposalApprovedIntegrationEvent> context)
    {
        var @event = context.Message;

        _logger.LogInformation(
            "Received ProposalApprovedIntegrationEvent. ProposalId: {ProposalId}, ProposalNumber: {ProposalNumber}",
            @event.ProposalId,
            @event.ProposalNumber);

        // Here you could:
        // 1. Cache the approved proposal data for faster contract creation
        // 2. Send notifications to the user that their proposal was approved
        // 3. Trigger automatic contracting workflows

        return Task.CompletedTask;
    }
}
