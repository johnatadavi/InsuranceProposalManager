using BuildingBlocks.Application.Messaging;
using ContratacaoService.Application.DTOs;

namespace ContratacaoService.Application.Features.Contracts.Commands.CreateContract;

/// <summary>
/// Command to create a contract from an approved proposal.
/// </summary>
public sealed record CreateContractCommand(Guid ProposalId) : ICommand<ContractResponse>;
