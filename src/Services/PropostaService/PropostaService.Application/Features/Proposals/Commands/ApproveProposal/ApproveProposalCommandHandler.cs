using BuildingBlocks.Application.Messaging;
using BuildingBlocks.Domain.Results;
using PropostaService.Application.Abstractions;
using PropostaService.Application.DTOs;
using PropostaService.Application.Mappers;
using PropostaService.Domain.Errors;
using PropostaService.Domain.Repositories;

namespace PropostaService.Application.Features.Proposals.Commands.ApproveProposal;

/// <summary>
/// Handler for ApproveProposalCommand.
/// </summary>
public sealed class ApproveProposalCommandHandler 
    : ICommandHandler<ApproveProposalCommand, ProposalResponse>
{
    private readonly IProposalRepository _proposalRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ApproveProposalCommandHandler(
        IProposalRepository proposalRepository,
        IUnitOfWork unitOfWork)
    {
        _proposalRepository = proposalRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProposalResponse>> Handle(
        ApproveProposalCommand request,
        CancellationToken cancellationToken)
    {
        var proposal = await _proposalRepository.GetByIdAsync(request.ProposalId, cancellationToken);

        if (proposal is null)
        {
            return Result.Failure<ProposalResponse>(ProposalErrors.NotFound(request.ProposalId));
        }

        var result = proposal.Approve();

        if (result.IsFailure)
        {
            return Result.Failure<ProposalResponse>(result.Error);
        }

        _proposalRepository.Update(proposal);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(proposal.ToResponse());
    }
}
