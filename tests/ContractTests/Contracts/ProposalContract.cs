using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace ContractTests.Contracts;

/// <summary>
/// Defines the contract that the ContratacaoService (Consumer) expects 
/// from the PropostaService (Provider) for proposal data.
/// </summary>
public static class ProposalContract
{
    /// <summary>
    /// The expected JSON structure for a Proposal response.
    /// This is what the ContratacaoService expects to receive.
    /// </summary>
    public static class ExpectedFields
    {
        public const string Id = "id";
        public const string ProposalNumber = "proposalNumber";
        public const string HolderCpf = "holderCpf";
        public const string HolderName = "holderName";
        public const string HolderEmail = "holderEmail";
        public const string InsuranceType = "insuranceType";
        public const string CoverageAmount = "coverageAmount";
        public const string PremiumAmount = "premiumAmount";
        public const string Status = "status";
        public const string CoverageStartDate = "coverageStartDate";
        public const string CoverageEndDate = "coverageEndDate";
    }

    /// <summary>
    /// Expected status values that the Consumer understands.
    /// </summary>
    public static class ExpectedStatuses
    {
        public const string UnderAnalysis = "UnderAnalysis";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
        public const string Contracted = "Contracted";
    }

    /// <summary>
    /// Creates a sample approved proposal JSON that represents the Provider's response.
    /// </summary>
    public static string CreateApprovedProposalJson(
        Guid? id = null,
        string? proposalNumber = null,
        string holderCpf = "12345678909",
        string holderName = "Test User",
        string holderEmail = "test@email.com",
        string insuranceType = "Life",
        decimal coverageAmount = 100000m,
        decimal premiumAmount = 500m)
    {
        var proposal = new
        {
            id = id ?? Guid.NewGuid(),
            proposalNumber = proposalNumber ?? $"PROP-{DateTime.Now:yyyyMMddHHmmss}",
            holderCpf,
            holderName,
            holderEmail,
            insuranceType,
            coverageAmount,
            premiumAmount,
            status = ExpectedStatuses.Approved,
            coverageStartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)).ToString("yyyy-MM-dd"),
            coverageEndDate = DateOnly.FromDateTime(DateTime.Today.AddYears(1)).ToString("yyyy-MM-dd"),
            createdAt = DateTime.UtcNow,
            approvedAt = DateTime.UtcNow
        };

        return JsonSerializer.Serialize(proposal, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });
    }

    /// <summary>
    /// Validates that a JSON response contains all required fields for the contract.
    /// </summary>
    public static void ValidateProposalResponse(JsonElement response)
    {
        // Check required fields exist
        response.TryGetProperty(ExpectedFields.Id, out _).Should().BeTrue($"Response must contain '{ExpectedFields.Id}'");
        response.TryGetProperty(ExpectedFields.ProposalNumber, out _).Should().BeTrue($"Response must contain '{ExpectedFields.ProposalNumber}'");
        response.TryGetProperty(ExpectedFields.HolderCpf, out _).Should().BeTrue($"Response must contain '{ExpectedFields.HolderCpf}'");
        response.TryGetProperty(ExpectedFields.HolderName, out _).Should().BeTrue($"Response must contain '{ExpectedFields.HolderName}'");
        response.TryGetProperty(ExpectedFields.HolderEmail, out _).Should().BeTrue($"Response must contain '{ExpectedFields.HolderEmail}'");
        response.TryGetProperty(ExpectedFields.InsuranceType, out _).Should().BeTrue($"Response must contain '{ExpectedFields.InsuranceType}'");
        response.TryGetProperty(ExpectedFields.CoverageAmount, out _).Should().BeTrue($"Response must contain '{ExpectedFields.CoverageAmount}'");
        response.TryGetProperty(ExpectedFields.PremiumAmount, out _).Should().BeTrue($"Response must contain '{ExpectedFields.PremiumAmount}'");
        response.TryGetProperty(ExpectedFields.Status, out _).Should().BeTrue($"Response must contain '{ExpectedFields.Status}'");
    }

    /// <summary>
    /// Validates that the status value is one of the expected statuses.
    /// </summary>
    public static void ValidateStatusValue(string status)
    {
        var validStatuses = new[]
        {
            ExpectedStatuses.UnderAnalysis,
            ExpectedStatuses.Approved,
            ExpectedStatuses.Rejected,
            ExpectedStatuses.Contracted
        };

        validStatuses.Should().Contain(status, $"Status '{status}' is not a valid contract status");
    }
}
