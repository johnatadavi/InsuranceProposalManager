using BuildingBlocks.Application.Messaging;
using BuildingBlocks.Domain.Results;
using ContratacaoService.Application.Abstractions;
using ContratacaoService.Application.DTOs;
using ContratacaoService.Application.Mappers;
using ContratacaoService.Domain.Entities;
using ContratacaoService.Domain.Errors;
using ContratacaoService.Domain.Repositories;

namespace ContratacaoService.Application.Features.Contracts.Commands.CreateContract;

/// <summary>
/// Handler for CreateContractCommand.
/// Orchestrates the contract creation use case.
/// </summary>
public sealed class CreateContractCommandHandler 
    : ICommandHandler<CreateContractCommand, ContractResponse>
{
    private readonly IContractRepository _contractRepository;
    private readonly IProposalService _proposalService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateContractCommandHandler(
        IContractRepository contractRepository,
        IProposalService proposalService,
        IUnitOfWork unitOfWork)
    {
        _contractRepository = contractRepository;
        _proposalService = proposalService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ContractResponse>> Handle(
        CreateContractCommand request,
        CancellationToken cancellationToken)
    {
        // Check if contract already exists for this proposal
        var existingContract = await _contractRepository.ExistsByProposalIdAsync(
            request.ProposalId, cancellationToken);

        if (existingContract)
        {
            return Result.Failure<ContractResponse>(ContractErrors.ProposalAlreadyContracted);
        }

        // Get proposal from PropostaService
        var proposalResult = await _proposalService.GetProposalByIdAsync(
            request.ProposalId, cancellationToken);

        if (proposalResult.IsFailure)
        {
            return Result.Failure<ContractResponse>(proposalResult.Error);
        }

        var proposal = proposalResult.Value;

        // Validate proposal can be contracted
        if (!proposal.CanBeContracted)
        {
            return Result.Failure<ContractResponse>(ContractErrors.ProposalNotApproved);
        }

        // Create the contract
        var contractResult = Contract.Create(
            proposal.Id,
            proposal.ProposalNumber,
            proposal.HolderCpf,
            proposal.HolderName,
            proposal.InsuranceType,
            proposal.CoverageAmount,
            proposal.PremiumAmount,
            proposal.Currency,
            proposal.CoverageStartDate,
            proposal.CoverageEndDate);

        if (contractResult.IsFailure)
        {
            return Result.Failure<ContractResponse>(contractResult.Error);
        }

        var contract = contractResult.Value;

        // Save contract
        await _contractRepository.AddAsync(contract, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Mark proposal as contracted (fire and forget via messaging or sync call)
        await _proposalService.MarkProposalAsContractedAsync(request.ProposalId, cancellationToken);

        return Result.Success(contract.ToResponse());
    }
}
