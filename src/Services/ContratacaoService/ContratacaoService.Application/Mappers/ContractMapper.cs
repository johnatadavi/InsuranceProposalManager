using ContratacaoService.Application.DTOs;
using ContratacaoService.Domain.Entities;

namespace ContratacaoService.Application.Mappers;

/// <summary>
/// Extension methods for mapping domain entities to DTOs.
/// </summary>
public static class ContractMapper
{
    public static ContractResponse ToResponse(this Contract contract)
    {
        return new ContractResponse(
            contract.Id,
            contract.ContractNumber,
            contract.ProposalId,
            contract.ProposalNumber,
            contract.HolderCpf,
            contract.HolderName,
            contract.InsuranceType,
            contract.CoverageAmount,
            contract.PremiumAmount,
            contract.Currency,
            contract.CoverageStartDate,
            contract.CoverageEndDate,
            contract.Status.ToString(),
            contract.ContractedAt,
            contract.CancelledAt,
            contract.CancellationReason);
    }
}
