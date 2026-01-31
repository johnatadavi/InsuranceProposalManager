using BuildingBlocks.Application.Messaging;
using BuildingBlocks.Domain.Results;
using ContratacaoService.Application.DTOs;
using ContratacaoService.Application.Mappers;
using ContratacaoService.Domain.Repositories;

namespace ContratacaoService.Application.Features.Contracts.Queries.GetAllContracts;

/// <summary>
/// Handler for GetAllContractsQuery.
/// </summary>
public sealed class GetAllContractsQueryHandler 
    : IQueryHandler<GetAllContractsQuery, IReadOnlyList<ContractResponse>>
{
    private readonly IContractRepository _contractRepository;

    public GetAllContractsQueryHandler(IContractRepository contractRepository)
    {
        _contractRepository = contractRepository;
    }

    public async Task<Result<IReadOnlyList<ContractResponse>>> Handle(
        GetAllContractsQuery request,
        CancellationToken cancellationToken)
    {
        var contracts = await _contractRepository.GetAllAsync(cancellationToken);

        var response = contracts
            .Select(c => c.ToResponse())
            .ToList()
            .AsReadOnly();

        return Result.Success<IReadOnlyList<ContractResponse>>(response);
    }
}
