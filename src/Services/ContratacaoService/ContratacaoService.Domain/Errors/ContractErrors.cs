using BuildingBlocks.Domain.Results;

namespace ContratacaoService.Domain.Errors;

/// <summary>
/// Domain errors specific to Contract.
/// </summary>
public static class ContractErrors
{
    public static Error NotFound(Guid contractId) => new(
        "Contract.NotFound",
        $"Contract with ID '{contractId}' was not found.");

    public static Error ProposalNotFound(Guid proposalId) => new(
        "Contract.ProposalNotFound",
        $"Proposal with ID '{proposalId}' was not found.");

    public static readonly Error ProposalNotApproved = new(
        "Contract.ProposalNotApproved",
        "Only approved proposals can be contracted.");

    public static readonly Error ProposalAlreadyContracted = new(
        "Contract.ProposalAlreadyContracted",
        "This proposal has already been contracted.");

    public static readonly Error AlreadyCancelled = new(
        "Contract.AlreadyCancelled",
        "The contract has already been cancelled.");

    public static readonly Error AlreadyExpired = new(
        "Contract.AlreadyExpired",
        "The contract has already expired.");

    public static readonly Error ExternalServiceUnavailable = new(
        "Contract.ExternalServiceUnavailable",
        "The proposal service is temporarily unavailable. Please try again later.");
}
