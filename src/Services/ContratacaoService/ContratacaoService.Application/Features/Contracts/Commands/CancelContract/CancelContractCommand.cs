using BuildingBlocks.Application.Messaging;
using ContratacaoService.Application.DTOs;

namespace ContratacaoService.Application.Features.Contracts.Commands.CancelContract;

/// <summary>
/// Command to cancel a contract.
/// </summary>
public sealed record CancelContractCommand(Guid ContractId, string Reason) : ICommand<ContractResponse>;
