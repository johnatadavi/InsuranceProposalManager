using Refit;

namespace ContratacaoService.Infrastructure.ExternalServices;

/// <summary>
/// Refit interface for PropostaService API communication.
/// Provides type-safe HTTP client with automatic serialization.
/// </summary>
[Headers("Content-Type: application/json")]
public interface IProposalApi
{
    /// <summary>
    /// Gets a proposal by its unique identifier.
    /// </summary>
    /// <param name="proposalId">The proposal ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>API response containing the proposal data.</returns>
    [Get("/api/proposals/{proposalId}")]
    Task<ApiResponse<ProposalApiResponse>> GetByIdAsync(
        Guid proposalId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a proposal as contracted.
    /// </summary>
    /// <param name="proposalId">The proposal ID to mark as contracted.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [Put("/api/proposals/{proposalId}/mark-contracted")]
    Task<ApiResponse<object>> MarkAsContractedAsync(
        Guid proposalId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Response model from PropostaService API.
/// </summary>
public sealed record ProposalApiResponse(
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
    string Status);
