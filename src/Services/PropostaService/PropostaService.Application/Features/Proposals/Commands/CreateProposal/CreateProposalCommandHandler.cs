using BuildingBlocks.Application.Messaging;
using BuildingBlocks.Domain.Results;
using PropostaService.Application.Abstractions;
using PropostaService.Application.DTOs;
using PropostaService.Application.Mappers;
using PropostaService.Domain.Entities;
using PropostaService.Domain.Repositories;

namespace PropostaService.Application.Features.Proposals.Commands.CreateProposal;

/// <summary>
/// Handler for CreateProposalCommand.
/// Orchestrates the proposal creation use case.
/// </summary>
public sealed class CreateProposalCommandHandler 
    : ICommandHandler<CreateProposalCommand, ProposalResponse>
{
    private readonly IProposalRepository _proposalRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProposalCommandHandler(
        IProposalRepository proposalRepository,
        IUnitOfWork unitOfWork)
    {
        _proposalRepository = proposalRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProposalResponse>> Handle(
        CreateProposalCommand request,
        CancellationToken cancellationToken)
    {
        var proposalResult = Proposal.Create(
            request.HolderCpf,
            request.HolderName,
            request.HolderEmail,
            request.InsuranceType,
            request.CoverageAmount,
            request.PremiumAmount,
            request.CoverageStartDate,
            request.CoverageEndDate,
            request.Description);

        if (proposalResult.IsFailure)
        {
            return Result.Failure<ProposalResponse>(proposalResult.Error);
        }

        var proposal = proposalResult.Value;

        await _proposalRepository.AddAsync(proposal, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(proposal.ToResponse());
    }
}
