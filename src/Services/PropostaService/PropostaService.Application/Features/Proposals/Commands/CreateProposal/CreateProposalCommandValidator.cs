using FluentValidation;

namespace PropostaService.Application.Features.Proposals.Commands.CreateProposal;

/// <summary>
/// Validator for CreateProposalCommand.
/// </summary>
public sealed class CreateProposalCommandValidator : AbstractValidator<CreateProposalCommand>
{
    public CreateProposalCommandValidator()
    {
        RuleFor(x => x.HolderCpf)
            .NotEmpty().WithMessage("CPF is required")
            .Length(11, 14).WithMessage("CPF must have 11 digits");

        RuleFor(x => x.HolderName)
            .NotEmpty().WithMessage("Holder name is required")
            .MaximumLength(200).WithMessage("Holder name must not exceed 200 characters");

        RuleFor(x => x.HolderEmail)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.InsuranceType)
            .IsInEnum().WithMessage("Invalid insurance type");

        RuleFor(x => x.CoverageAmount)
            .GreaterThan(0).WithMessage("Coverage amount must be greater than zero");

        RuleFor(x => x.PremiumAmount)
            .GreaterThan(0).WithMessage("Premium amount must be greater than zero");

        RuleFor(x => x.CoverageStartDate)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Coverage start date cannot be in the past");

        RuleFor(x => x.CoverageEndDate)
            .GreaterThan(x => x.CoverageStartDate)
            .WithMessage("Coverage end date must be after start date");
    }
}
