using BuildingBlocks.Application.Messaging;
using PropostaService.Application.DTOs;

namespace PropostaService.Application.Features.Proposals.Queries.GetProposalById;

/// <summary>
/// Query to get a proposal by its ID.
/// </summary>
public sealed record GetProposalByIdQuery(Guid ProposalId) : IQuery<ProposalResponse>;
