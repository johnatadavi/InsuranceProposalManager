using BuildingBlocks.Application.Messaging;
using PropostaService.Application.DTOs;

namespace PropostaService.Application.Features.Proposals.Commands.MarkAsContracted;

/// <summary>
/// Command to mark a proposal as contracted.
/// This is called internally when a contract is created.
/// </summary>
public sealed record MarkAsContractedCommand(Guid ProposalId) : ICommand<ProposalResponse>;
