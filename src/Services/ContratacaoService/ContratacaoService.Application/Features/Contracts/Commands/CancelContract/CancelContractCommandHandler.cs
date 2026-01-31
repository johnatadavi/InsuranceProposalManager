using BuildingBlocks.Application.Messaging;
using BuildingBlocks.Domain.Results;
using ContratacaoService.Application.Abstractions;
using ContratacaoService.Application.DTOs;
using ContratacaoService.Application.Mappers;
using ContratacaoService.Domain.Errors;
using ContratacaoService.Domain.Repositories;

namespace ContratacaoService.Application.Features.Contracts.Commands.CancelContract;

/// <summary>
/// Handler for CancelContractCommand.
/// </summary>
public sealed class CancelContractCommandHandler 
    : ICommandHandler<CancelContractCommand, ContractResponse>
{
    private readonly IContractRepository _contractRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelContractCommandHandler(
        IContractRepository contractRepository,
        IUnitOfWork unitOfWork)
    {
        _contractRepository = contractRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ContractResponse>> Handle(
        CancelContractCommand request,
        CancellationToken cancellationToken)
    {
        var contract = await _contractRepository.GetByIdAsync(request.ContractId, cancellationToken);

        if (contract is null)
        {
            return Result.Failure<ContractResponse>(ContractErrors.NotFound(request.ContractId));
        }

        var result = contract.Cancel(request.Reason);

        if (result.IsFailure)
        {
            return Result.Failure<ContractResponse>(result.Error);
        }

        _contractRepository.Update(contract);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(contract.ToResponse());
    }
}
