using BuildingBlocks.Application.Messaging;
using BuildingBlocks.Domain.Results;
using PropostaService.Application.DTOs;
using PropostaService.Application.Mappers;
using PropostaService.Domain.Errors;
using PropostaService.Domain.Repositories;

namespace PropostaService.Application.Features.Proposals.Queries.GetProposalById;

/// <summary>
/// Handler for GetProposalByIdQuery.
/// </summary>
public sealed class GetProposalByIdQueryHandler 
    : IQueryHandler<GetProposalByIdQuery, ProposalResponse>
{
    private readonly IProposalRepository _proposalRepository;

    public GetProposalByIdQueryHandler(IProposalRepository proposalRepository)
    {
        _proposalRepository = proposalRepository;
    }

    public async Task<Result<ProposalResponse>> Handle(
        GetProposalByIdQuery request,
        CancellationToken cancellationToken)
    {
        var proposal = await _proposalRepository.GetByIdAsync(request.ProposalId, cancellationToken);

        if (proposal is null)
        {
            return Result.Failure<ProposalResponse>(ProposalErrors.NotFound(request.ProposalId));
        }

        return Result.Success(proposal.ToResponse());
    }
}
