using BuildingBlocks.Domain.Results;

namespace ContratacaoService.Application.Abstractions;

/// <summary>
/// Port interface for communicating with PropostaService.
/// This abstracts the external service - can be HTTP, gRPC, or messaging.
/// </summary>
public interface IProposalService
{
    /// <summary>
    /// Gets proposal details by ID.
    /// </summary>
    Task<Result<ProposalDto>> GetProposalByIdAsync(Guid proposalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies the Proposta service that a proposal has been contracted.
    /// </summary>
    Task<Result> MarkProposalAsContractedAsync(Guid proposalId, CancellationToken cancellationToken = default);
}

/// <summary>
/// DTO representing proposal data from PropostaService.
/// </summary>
public sealed record ProposalDto(
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
    string Status,
    bool CanBeContracted);
