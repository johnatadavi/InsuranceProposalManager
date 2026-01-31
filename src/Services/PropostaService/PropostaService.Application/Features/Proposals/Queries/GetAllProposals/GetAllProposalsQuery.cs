using BuildingBlocks.Application.Messaging;
using PropostaService.Application.DTOs;

namespace PropostaService.Application.Features.Proposals.Queries.GetAllProposals;

/// <summary>
/// Query to get all proposals.
/// </summary>
public sealed record GetAllProposalsQuery : IQuery<IReadOnlyList<ProposalResponse>>;
