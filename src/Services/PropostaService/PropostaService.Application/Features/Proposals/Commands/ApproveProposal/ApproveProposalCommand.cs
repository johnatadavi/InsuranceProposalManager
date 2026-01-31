using BuildingBlocks.Application.Messaging;
using PropostaService.Application.DTOs;

namespace PropostaService.Application.Features.Proposals.Commands.ApproveProposal;

/// <summary>
/// Command to approve a proposal.
/// </summary>
public sealed record ApproveProposalCommand(Guid ProposalId) : ICommand<ProposalResponse>;
