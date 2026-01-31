using PropostaService.Application.DTOs;
using PropostaService.Domain.Entities;

namespace PropostaService.Application.Mappers;

/// <summary>
/// Extension methods for mapping domain entities to DTOs.
/// </summary>
public static class ProposalMapper
{
    public static ProposalResponse ToResponse(this Proposal proposal)
    {
        return new ProposalResponse(
            proposal.Id,
            proposal.ProposalNumber,
            proposal.HolderCpf.ToFormattedString(),
            proposal.HolderName,
            proposal.HolderEmail.Value,
            proposal.InsuranceType.ToString(),
            proposal.CoverageAmount.Amount,
            proposal.PremiumAmount.Amount,
            proposal.CoverageAmount.Currency,
            proposal.CoveragePeriod.StartDate,
            proposal.CoveragePeriod.EndDate,
            proposal.Description,
            proposal.Status.ToString(),
            proposal.CreatedAt,
            proposal.UpdatedAt,
            proposal.ApprovedAt,
            proposal.RejectedAt,
            proposal.ContractedAt,
            proposal.RejectionReason);
    }
}
