using PropostaService.Domain.Enums;

namespace PropostaService.Application.DTOs;

/// <summary>
/// DTO for Proposal response.
/// </summary>
public sealed record ProposalResponse(
    Guid Id,
    string ProposalNumber,
    string HolderCpf,
    string HolderName,
    string HolderEmail,
    string InsuranceType,
    decimal CoverageAmount,
    decimal PremiumAmount,
    string Currency,
    DateOnly CoverageStartDate,
    DateOnly CoverageEndDate,
    string? Description,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? ApprovedAt,
    DateTime? RejectedAt,
    DateTime? ContractedAt,
    string? RejectionReason);
