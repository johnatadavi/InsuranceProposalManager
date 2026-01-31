namespace PropostaService.Domain.Enums;

/// <summary>
/// Represents the possible statuses of an insurance proposal.
/// </summary>
public enum ProposalStatus
{
    /// <summary>
    /// Proposal is under analysis
    /// </summary>
    UnderAnalysis = 1,

    /// <summary>
    /// Proposal has been approved
    /// </summary>
    Approved = 2,

    /// <summary>
    /// Proposal has been rejected
    /// </summary>
    Rejected = 3,

    /// <summary>
    /// Proposal has been contracted
    /// </summary>
    Contracted = 4
}
