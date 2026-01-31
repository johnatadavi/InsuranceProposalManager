using System.Net.Http.Json;
using System.Text.Json;
using BuildingBlocks.Domain.Results;
using ContratacaoService.Application.Abstractions;
using ContratacaoService.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace ContratacaoService.Infrastructure.ExternalServices;

/// <summary>
/// HTTP implementation of IProposalService.
/// Communicates with PropostaService via REST API.
/// </summary>
public sealed class HttpProposalService : IProposalService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpProposalService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public HttpProposalService(
        HttpClient httpClient,
        ILogger<HttpProposalService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<Result<ProposalDto>> GetProposalByIdAsync(
        Guid proposalId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"/api/proposals/{proposalId}",
                cancellationToken);

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

            var proposalResponse = await response.Content.ReadFromJsonAsync<ProposalApiResponse>(
                _jsonOptions,
                cancellationToken);

            if (proposalResponse is null)
            {
                return Result.Failure<ProposalDto>(ContractErrors.ProposalNotFound(proposalId));
            }

            var dto = new ProposalDto(
                proposalResponse.Id,
                proposalResponse.ProposalNumber,
                proposalResponse.HolderCpf,
                proposalResponse.HolderName,
                proposalResponse.HolderEmail,
                proposalResponse.InsuranceType,
                proposalResponse.CoverageAmount,
                proposalResponse.PremiumAmount,
                proposalResponse.Currency,
                proposalResponse.CoverageStartDate,
                proposalResponse.CoverageEndDate,
                proposalResponse.Status,
                proposalResponse.Status == "Approved");

            return Result.Success(dto);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while getting proposal {ProposalId}", proposalId);
            return Result.Failure<ProposalDto>(ContractErrors.ExternalServiceUnavailable);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Timeout while getting proposal {ProposalId}", proposalId);
            return Result.Failure<ProposalDto>(ContractErrors.ExternalServiceUnavailable);
        }
    }

    public async Task<Result> MarkProposalAsContractedAsync(
        Guid proposalId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PutAsync(
                $"/api/proposals/{proposalId}/mark-contracted",
                null,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Failed to mark proposal {ProposalId} as contracted. Status: {StatusCode}",
                    proposalId,
                    response.StatusCode);

                // We don't fail the contract creation if this fails
                // The proposal status will eventually be updated via events
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

    // Internal class to deserialize API response
    private sealed record ProposalApiResponse(
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
}
