using BuildingBlocks.Application.Messaging;
using BuildingBlocks.Domain.Results;
using ContratacaoService.Application.DTOs;
using ContratacaoService.Application.Mappers;
using ContratacaoService.Domain.Errors;
using ContratacaoService.Domain.Repositories;

namespace ContratacaoService.Application.Features.Contracts.Queries.GetContractById;

/// <summary>
/// Handler for GetContractByIdQuery.
/// </summary>
public sealed class GetContractByIdQueryHandler 
    : IQueryHandler<GetContractByIdQuery, ContractResponse>
{
    private readonly IContractRepository _contractRepository;

    public GetContractByIdQueryHandler(IContractRepository contractRepository)
    {
        _contractRepository = contractRepository;
    }

    public async Task<Result<ContractResponse>> Handle(
        GetContractByIdQuery request,
        CancellationToken cancellationToken)
    {
        var contract = await _contractRepository.GetByIdAsync(request.ContractId, cancellationToken);

        if (contract is null)
        {
            return Result.Failure<ContractResponse>(ContractErrors.NotFound(request.ContractId));
        }

        return Result.Success(contract.ToResponse());
    }
}
