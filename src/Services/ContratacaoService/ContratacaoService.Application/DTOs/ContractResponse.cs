namespace ContratacaoService.Application.DTOs;

/// <summary>
/// DTO for Contract response.
/// </summary>
public sealed record ContractResponse(
    Guid Id,
    string ContractNumber,
    Guid ProposalId,
    string ProposalNumber,
    string HolderCpf,
    string HolderName,
    string InsuranceType,
    decimal CoverageAmount,
    decimal PremiumAmount,
    string Currency,
    DateOnly CoverageStartDate,
    DateOnly CoverageEndDate,
    string Status,
    DateTime ContractedAt,
    DateTime? CancelledAt,
    string? CancellationReason);
