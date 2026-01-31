using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ContractTests.Contracts;
using FluentAssertions;
using NSubstitute;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace ContractTests.Consumer;

/// <summary>
/// Consumer-side contract tests.
/// These tests verify that the ContratacaoService (Consumer) 
/// correctly consumes the PropostaService (Provider) API according to the contract.
/// Uses WireMock to simulate the Provider responses.
/// </summary>
public class ContratacaoServiceConsumerContractTests : IDisposable
{
    private readonly WireMockServer _mockServer;
    private readonly HttpClient _httpClient;

    public ContratacaoServiceConsumerContractTests()
    {
        _mockServer = WireMockServer.Start();
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_mockServer.Url!)
        };
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        _mockServer.Stop();
        _mockServer.Dispose();
    }

    [Fact]
    public async Task Consumer_CanDeserialize_ApprovedProposalFromProvider()
    {
        // Arrange - Set up mock server to return contract-compliant response
        var proposalId = Guid.NewGuid();
        var expectedJson = ProposalContract.CreateApprovedProposalJson(
            id: proposalId,
            proposalNumber: "PROP-20240201001",
            holderName: "Consumer Test User",
            coverageAmount: 150000m);

        _mockServer
            .Given(Request.Create()
                .WithPath($"/api/proposals/{proposalId}")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(expectedJson));

        // Act - Simulate what the Consumer (ContratacaoService) does
        var response = await _httpClient.GetAsync($"/api/proposals/{proposalId}");
        
        // Assert - Verify the Consumer can parse the response
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var proposal = JsonSerializer.Deserialize<ConsumerProposalDto>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        proposal.Should().NotBeNull();
        proposal!.Id.Should().Be(proposalId);
        proposal.ProposalNumber.Should().Be("PROP-20240201001");
        proposal.HolderName.Should().Be("Consumer Test User");
        proposal.CoverageAmount.Should().Be(150000m);
        proposal.Status.Should().Be(ProposalContract.ExpectedStatuses.Approved);
    }

    [Fact]
    public async Task Consumer_HandlesNotFound_WhenProposalDoesNotExist()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        _mockServer
            .Given(Request.Create()
                .WithPath($"/api/proposals/{nonExistingId}")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(404)
                .WithHeader("Content-Type", "application/json")
                .WithBody("{\"error\":\"Proposal not found\"}"));

        // Act
        var response = await _httpClient.GetAsync($"/api/proposals/{nonExistingId}");

        // Assert - Consumer should handle 404 gracefully
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Consumer_CanIdentify_ApprovedProposalForContracting()
    {
        // Arrange - Set up approved proposal
        var proposalId = Guid.NewGuid();
        var expectedJson = ProposalContract.CreateApprovedProposalJson(
            id: proposalId,
            holderName: "Ready for Contract");

        _mockServer
            .Given(Request.Create()
                .WithPath($"/api/proposals/{proposalId}")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(expectedJson));

        // Act
        var response = await _httpClient.GetAsync($"/api/proposals/{proposalId}");
        var content = await response.Content.ReadAsStringAsync();
        var proposal = JsonSerializer.Deserialize<ConsumerProposalDto>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        // Assert - Consumer logic: only approved proposals can be contracted
        proposal.Should().NotBeNull();
        var canBeContracted = proposal!.Status == ProposalContract.ExpectedStatuses.Approved;
        canBeContracted.Should().BeTrue("Consumer expects to contract only approved proposals");
    }

    [Fact]
    public async Task Consumer_RejectsContracting_WhenProposalNotApproved()
    {
        // Arrange - Set up a proposal that is under analysis (not approved)
        var proposalId = Guid.NewGuid();
        var underAnalysisProposal = new
        {
            id = proposalId,
            proposalNumber = "PROP-20240201002",
            holderCpf = "12345678909",
            holderName = "Under Analysis User",
            holderEmail = "analysis@test.com",
            insuranceType = "Life",
            coverageAmount = 100000m,
            premiumAmount = 500m,
            status = ProposalContract.ExpectedStatuses.UnderAnalysis, // NOT APPROVED
            coverageStartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)).ToString("yyyy-MM-dd"),
            coverageEndDate = DateOnly.FromDateTime(DateTime.Today.AddYears(1)).ToString("yyyy-MM-dd")
        };

        var json = JsonSerializer.Serialize(underAnalysisProposal, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        _mockServer
            .Given(Request.Create()
                .WithPath($"/api/proposals/{proposalId}")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(json));

        // Act
        var response = await _httpClient.GetAsync($"/api/proposals/{proposalId}");
        var content = await response.Content.ReadAsStringAsync();
        var proposal = JsonSerializer.Deserialize<ConsumerProposalDto>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        // Assert - Consumer should NOT contract proposals that aren't approved
        proposal.Should().NotBeNull();
        var canBeContracted = proposal!.Status == ProposalContract.ExpectedStatuses.Approved;
        canBeContracted.Should().BeFalse("Consumer should reject contracting non-approved proposals");
    }

    [Fact]
    public async Task Consumer_CanCallMarkAsContracted_Endpoint()
    {
        // Arrange
        var proposalId = Guid.NewGuid();

        _mockServer
            .Given(Request.Create()
                .WithPath($"/api/proposals/{proposalId}/mark-contracted")
                .UsingPut())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("{\"success\": true}"));

        // Act
        var response = await _httpClient.PutAsync($"/api/proposals/{proposalId}/mark-contracted", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public void ContractBreak_WhenRequiredFieldMissing_ShouldFail()
    {
        // Arrange - Invalid response missing required fields
        var invalidJson = """
        {
            "id": "00000000-0000-0000-0000-000000000001",
            "holderName": "Test User"
        }
        """;

        // Act
        using var jsonDoc = JsonDocument.Parse(invalidJson);
        var root = jsonDoc.RootElement;

        // Assert - Contract validation should fail
        var act = () => ProposalContract.ValidateProposalResponse(root);
        act.Should().Throw<Exception>("Missing required fields should break the contract");
    }
}

/// <summary>
/// DTO that represents what the Consumer expects from the Provider.
/// This is the Consumer's view of the contract.
/// </summary>
public record ConsumerProposalDto
{
    public Guid Id { get; init; }
    public string ProposalNumber { get; init; } = string.Empty;
    public string HolderCpf { get; init; } = string.Empty;
    public string HolderName { get; init; } = string.Empty;
    public string HolderEmail { get; init; } = string.Empty;
    public string InsuranceType { get; init; } = string.Empty;
    public decimal CoverageAmount { get; init; }
    public decimal PremiumAmount { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? CoverageStartDate { get; init; }
    public string? CoverageEndDate { get; init; }
}
