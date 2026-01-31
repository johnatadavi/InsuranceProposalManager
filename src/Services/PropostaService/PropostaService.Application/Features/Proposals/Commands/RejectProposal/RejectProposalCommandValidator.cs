using FluentValidation;

namespace PropostaService.Application.Features.Proposals.Commands.RejectProposal;

/// <summary>
/// Validator for RejectProposalCommand.
/// </summary>
public sealed class RejectProposalCommandValidator : AbstractValidator<RejectProposalCommand>
{
    public RejectProposalCommandValidator()
    {
        RuleFor(x => x.ProposalId)
            .NotEmpty().WithMessage("Proposal ID is required");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Rejection reason is required")
            .MaximumLength(1000).WithMessage("Rejection reason must not exceed 1000 characters");
    }
}
