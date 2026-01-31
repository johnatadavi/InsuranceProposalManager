using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ContractTests.Contracts;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PropostaService.Domain.Enums;
using PropostaService.Infrastructure.Persistence;
using Testcontainers.PostgreSql;
using Xunit;

namespace ContractTests.Provider;

/// <summary>
/// Provider-side contract tests.
/// These tests verify that the PropostaService (Provider) fulfills 
/// the contract expected by the ContratacaoService (Consumer).
/// </summary>
[Trait("Category", "RequiresDocker")]
public class PropostaServiceProviderContractTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;
    private WebApplicationFactory<PropostaService.Api.Program> _factory = null!;
    private HttpClient _client = null!;

    public PropostaServiceProviderContractTests()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("provider_contract_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        _factory = new WebApplicationFactory<PropostaService.Api.Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureServices(services =>
                {
                    // Remove existing DbContext
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<PropostaDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    // Add test database
                    services.AddDbContext<PropostaDbContext>(options =>
                        options.UseNpgsql(_postgresContainer.GetConnectionString()));
                });
            });

        _client = _factory.CreateClient();

        // Ensure database is created
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PropostaDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _postgresContainer.DisposeAsync();
    }

    [Fact]
    public async Task GetProposalById_ShouldReturn_ContractCompliantResponse()
    {
        // Arrange - Create a proposal first
        var createRequest = new
        {
            HolderCpf = "12345678909",
            HolderName = "Contract Test User",
            HolderEmail = "contract@test.com",
            InsuranceType = InsuranceType.Life,
            CoverageAmount = 100000m,
            PremiumAmount = 500m,
            CoverageStartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            CoverageEndDate = DateOnly.FromDateTime(DateTime.Today.AddYears(1)),
            Description = "Contract test proposal"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/proposals", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createContent = await createResponse.Content.ReadAsStringAsync();
        using var createDoc = JsonDocument.Parse(createContent);
        var proposalId = createDoc.RootElement.GetProperty("id").GetGuid();

        // Act - Get the proposal
        var response = await _client.GetAsync($"/api/proposals/{proposalId}");

        // Assert - Verify contract compliance
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;

        // Validate contract fields
        ProposalContract.ValidateProposalResponse(root);

        // Validate specific field types
        root.GetProperty(ProposalContract.ExpectedFields.Id).GetGuid().Should().NotBe(Guid.Empty);
        root.GetProperty(ProposalContract.ExpectedFields.ProposalNumber).GetString().Should().StartWith("PROP-");
        root.GetProperty(ProposalContract.ExpectedFields.HolderCpf).GetString().Should().NotBeNullOrEmpty();
        root.GetProperty(ProposalContract.ExpectedFields.HolderName).GetString().Should().Be("Contract Test User");
        root.GetProperty(ProposalContract.ExpectedFields.CoverageAmount).GetDecimal().Should().BePositive();
        root.GetProperty(ProposalContract.ExpectedFields.PremiumAmount).GetDecimal().Should().BePositive();

        // Validate status is a known value
        var status = root.GetProperty(ProposalContract.ExpectedFields.Status).GetString()!;
        ProposalContract.ValidateStatusValue(status);
    }

    [Fact]
    public async Task GetApprovedProposal_ShouldReturn_ApprovedStatus()
    {
        // Arrange - Create and approve a proposal
        var createRequest = new
        {
            HolderCpf = "12345678909",
            HolderName = "Approved User",
            HolderEmail = "approved@test.com",
            InsuranceType = InsuranceType.Auto,
            CoverageAmount = 75000m,
            PremiumAmount = 350m,
            CoverageStartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            CoverageEndDate = DateOnly.FromDateTime(DateTime.Today.AddYears(1)),
            Description = "Test for approval"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/proposals", createRequest);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        using var createDoc = JsonDocument.Parse(createContent);
        var proposalId = createDoc.RootElement.GetProperty("id").GetGuid();

        // Approve the proposal
        var approveResponse = await _client.PutAsync($"/api/proposals/{proposalId}/approve", null);
        approveResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act - Get the approved proposal
        var response = await _client.GetAsync($"/api/proposals/{proposalId}");

        // Assert - Verify the contract for approved proposals
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;

        // Validate contract
        ProposalContract.ValidateProposalResponse(root);

        // Verify approved status - THIS IS WHAT THE CONSUMER EXPECTS
        var status = root.GetProperty(ProposalContract.ExpectedFields.Status).GetString();
        status.Should().Be(ProposalContract.ExpectedStatuses.Approved,
            "ContratacaoService expects 'Approved' status for proposals that can be contracted");
    }

    [Fact]
    public async Task GetAllProposals_ShouldReturn_ArrayWithContractCompliantItems()
    {
        // Arrange - Create a proposal
        var createRequest = new
        {
            HolderCpf = "12345678909",
            HolderName = "List Test User",
            HolderEmail = "list@test.com",
            InsuranceType = InsuranceType.Health,
            CoverageAmount = 50000m,
            PremiumAmount = 200m,
            CoverageStartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            CoverageEndDate = DateOnly.FromDateTime(DateTime.Today.AddYears(1)),
            Description = "List test"
        };

        await _client.PostAsJsonAsync("/api/proposals", createRequest);

        // Act
        var response = await _client.GetAsync("/api/proposals");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;

        root.ValueKind.Should().Be(JsonValueKind.Array);
        root.GetArrayLength().Should().BeGreaterThan(0);

        // Validate each item in the array follows the contract
        foreach (var item in root.EnumerateArray())
        {
            ProposalContract.ValidateProposalResponse(item);
        }
    }

    [Fact]
    public async Task GetNonExistingProposal_ShouldReturn_NotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/proposals/{nonExistingId}");

        // Assert - Consumer expects 404 for non-existing resources
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
