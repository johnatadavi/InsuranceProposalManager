using BuildingBlocks.Application.Messaging;
using PropostaService.Application.DTOs;

namespace PropostaService.Application.Features.Proposals.Commands.RejectProposal;

/// <summary>
/// Command to reject a proposal.
/// </summary>
public sealed record RejectProposalCommand(Guid ProposalId, string Reason) : ICommand<ProposalResponse>;
