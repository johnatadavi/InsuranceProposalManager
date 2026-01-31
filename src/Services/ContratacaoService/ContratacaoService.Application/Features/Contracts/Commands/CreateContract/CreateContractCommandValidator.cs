using FluentValidation;

namespace ContratacaoService.Application.Features.Contracts.Commands.CreateContract;

/// <summary>
/// Validator for CreateContractCommand.
/// </summary>
public sealed class CreateContractCommandValidator : AbstractValidator<CreateContractCommand>
{
    public CreateContractCommandValidator()
    {
        RuleFor(x => x.ProposalId)
            .NotEmpty().WithMessage("Proposal ID is required");
    }
}
