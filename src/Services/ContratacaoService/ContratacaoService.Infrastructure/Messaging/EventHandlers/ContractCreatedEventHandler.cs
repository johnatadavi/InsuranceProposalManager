using ContratacaoService.Domain.Events;
using ContratacaoService.Infrastructure.Messaging.Events;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ContratacaoService.Infrastructure.Messaging.EventHandlers;

/// <summary>
/// Handler that publishes integration event when a contract is created.
/// </summary>
public sealed class ContractCreatedEventHandler : INotificationHandler<ContractCreatedEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<ContractCreatedEventHandler> _logger;

    public ContractCreatedEventHandler(
        IPublishEndpoint publishEndpoint,
        ILogger<ContractCreatedEventHandler> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Handle(ContractCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Publishing ContractCreatedIntegrationEvent for contract {ContractId}",
            notification.ContractId);

        var integrationEvent = new ContractCreatedIntegrationEvent(
            notification.ContractId,
            notification.ProposalId,
            notification.ContractNumber,
            notification.ContractedAt);

        await _publishEndpoint.Publish(integrationEvent, cancellationToken);

        _logger.LogInformation(
            "Successfully published ContractCreatedIntegrationEvent for contract {ContractId}",
            notification.ContractId);
    }
}
