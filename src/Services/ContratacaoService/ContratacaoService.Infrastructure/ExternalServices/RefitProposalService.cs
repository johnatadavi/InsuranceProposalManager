using BuildingBlocks.Domain.Results;
using ContratacaoService.Application.Abstractions;
using ContratacaoService.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace ContratacaoService.Infrastructure.ExternalServices;

/// <summary>
/// Refit-based implementation of IProposalService.
/// Uses typed HTTP client with automatic retry and circuit breaker via Polly.
/// 
/// Compared to manual HttpClient implementation:
/// - 80% less code
/// - Automatic JSON serialization/deserialization
/// - Built-in resilience with Microsoft.Extensions.Http.Resilience
/// </summary>
public sealed class RefitProposalService : IProposalService
{
    private readonly IProposalApi _proposalApi;
    private readonly ILogger<RefitProposalService> _logger;

    public RefitProposalService(
        IProposalApi proposalApi,
        ILogger<RefitProposalService> logger)
    {
        _proposalApi = proposalApi;
        _logger = logger;
    }

    public async Task<Result<ProposalDto>> GetProposalByIdAsync(
        Guid proposalId,
        CancellationToken cancellationToken = default)
    {
        var response = await _proposalApi.GetByIdAsync(proposalId, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return Result.Failure<ProposalDto>(ContractErrors.ProposalNotFound(proposalId));
            }

            _logger.LogWarning(
                "Failed to get proposal {ProposalId}. Status: {StatusCode}",
                proposalId,
                response.StatusCode);

            return Result.Failure<ProposalDto>(ContractErrors.ExternalServiceUnavailable);
        }

        var proposal = response.Content;
        if (proposal is null)
        {
            return Result.Failure<ProposalDto>(ContractErrors.ProposalNotFound(proposalId));
        }

        return Result.Success(new ProposalDto(
            proposal.Id,
            proposal.ProposalNumber,
            proposal.HolderCpf,
            proposal.HolderName,
            proposal.HolderEmail,
            proposal.InsuranceType,
            proposal.CoverageAmount,
            proposal.PremiumAmount,
            proposal.Currency,
            proposal.CoverageStartDate,
            proposal.CoverageEndDate,
            proposal.Status,
            proposal.Status == "Approved"));
    }

    public async Task<Result> MarkProposalAsContractedAsync(
        Guid proposalId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _proposalApi.MarkAsContractedAsync(proposalId, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Failed to mark proposal {ProposalId} as contracted. Status: {StatusCode}",
                    proposalId,
                    response.StatusCode);
                // Don't fail - eventual consistency via events
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Error marking proposal {ProposalId} as contracted. Will be retried via events.",
                proposalId);

            // Don't fail the contract creation
            return Result.Success();
        }
    }
}
