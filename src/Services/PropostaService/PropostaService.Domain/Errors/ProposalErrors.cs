using BuildingBlocks.Domain.Results;

namespace PropostaService.Domain.Errors;

/// <summary>
/// Domain errors specific to Proposal.
/// </summary>
public static class ProposalErrors
{
    public static Error NotFound(Guid proposalId) => new(
        "Proposal.NotFound",
        $"Proposal with ID '{proposalId}' was not found.");

    public static readonly Error AlreadyApproved = new(
        "Proposal.AlreadyApproved",
        "The proposal has already been approved.");

    public static readonly Error AlreadyRejected = new(
        "Proposal.AlreadyRejected",
        "The proposal has already been rejected.");

    public static readonly Error AlreadyContracted = new(
        "Proposal.AlreadyContracted",
        "The proposal has already been contracted.");

    public static readonly Error NotApproved = new(
        "Proposal.NotApproved",
        "Only approved proposals can be contracted.");

    public static readonly Error InvalidStatusTransition = new(
        "Proposal.InvalidStatusTransition",
        "The status transition is not allowed.");

    public static readonly Error InvalidCoverageAmount = new(
        "Proposal.InvalidCoverageAmount",
        "Coverage amount must be greater than zero.");

    public static readonly Error InvalidPremiumAmount = new(
        "Proposal.InvalidPremiumAmount",
        "Premium amount must be greater than zero.");
}
