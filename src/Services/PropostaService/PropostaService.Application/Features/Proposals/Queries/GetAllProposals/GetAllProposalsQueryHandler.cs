using BuildingBlocks.Application.Messaging;
using BuildingBlocks.Domain.Results;
using PropostaService.Application.DTOs;
using PropostaService.Application.Mappers;
using PropostaService.Domain.Repositories;

namespace PropostaService.Application.Features.Proposals.Queries.GetAllProposals;

/// <summary>
/// Handler for GetAllProposalsQuery.
/// </summary>
public sealed class GetAllProposalsQueryHandler 
    : IQueryHandler<GetAllProposalsQuery, IReadOnlyList<ProposalResponse>>
{
    private readonly IProposalRepository _proposalRepository;

    public GetAllProposalsQueryHandler(IProposalRepository proposalRepository)
    {
        _proposalRepository = proposalRepository;
    }

    public async Task<Result<IReadOnlyList<ProposalResponse>>> Handle(
        GetAllProposalsQuery request,
        CancellationToken cancellationToken)
    {
        var proposals = await _proposalRepository.GetAllAsync(cancellationToken);

        var response = proposals
            .Select(p => p.ToResponse())
            .ToList()
            .AsReadOnly();

        return Result.Success<IReadOnlyList<ProposalResponse>>(response);
    }
}
