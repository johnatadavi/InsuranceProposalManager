using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IntegrationTests.Factories;
using IntegrationTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using PropostaService.Domain.Enums;
using PropostaService.Infrastructure.Persistence;
using Respawn;
using Xunit;

namespace IntegrationTests.PropostaService;

/// <summary>
/// Integration tests for PropostaService API.
/// Uses Testcontainers with real PostgreSQL database.
/// Tests the complete flow: Controller -> UseCase -> Repository -> Database.
/// </summary>
[Collection(nameof(PostgresContainerCollection))]
[Trait("Category", "RequiresDocker")]
public class PropostaServiceIntegrationTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _postgresFixture;
    private PropostaServiceWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;
    private Respawner? _respawner;

    public PropostaServiceIntegrationTests(PostgresContainerFixture postgresFixture)
    {
        _postgresFixture = postgresFixture;
    }

    public async Task InitializeAsync()
    {
        _factory = new PropostaServiceWebApplicationFactory(_postgresFixture);
        await _factory.InitializeAsync();
        _client = _factory.CreateClient();

        // Initialize Respawn for database cleanup between tests
        await using var connection = new NpgsqlConnection(_postgresFixture.ConnectionString);
        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"]
        });
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    private async Task ResetDatabaseAsync()
    {
        if (_respawner is not null)
        {
            await using var connection = new NpgsqlConnection(_postgresFixture.ConnectionString);
            await connection.OpenAsync();
            await _respawner.ResetAsync(connection);
        }
    }

    #region Create Proposal Tests

    [Fact]
    public async Task CreateProposal_WithValidData_ShouldReturnCreatedAndPersistToDatabase()
    {
        // Arrange
        await ResetDatabaseAsync();

        var request = new
        {
            HolderCpf = "12345678909",
            HolderName = "João Silva",
            HolderEmail = "joao@email.com",
            InsuranceType = InsuranceType.Life,
            CoverageAmount = 100000m,
            PremiumAmount = 500m,
            CoverageStartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            CoverageEndDate = DateOnly.FromDateTime(DateTime.Today.AddYears(1)),
            Description = "Integration test proposal"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/proposals", request);

        // Assert - HTTP Response
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdProposal = await response.Content.ReadFromJsonAsync<ProposalResponse>();
        createdProposal.Should().NotBeNull();
        createdProposal!.Id.Should().NotBe(Guid.Empty);
        createdProposal.HolderName.Should().Be("João Silva");
        createdProposal.Status.Should().Be("UnderAnalysis");
        createdProposal.ProposalNumber.Should().StartWith("PROP-");

        // Assert - Database Persistence
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PropostaDbContext>();
        var proposalInDb = await dbContext.Proposals.FindAsync(createdProposal.Id);

        proposalInDb.Should().NotBeNull();
        proposalInDb!.HolderName.Should().Be("João Silva");
        proposalInDb.CoverageAmount.Should().Be(100000m);
    }

    [Fact]
    public async Task CreateProposal_WithInvalidCpf_ShouldReturnBadRequest()
    {
        // Arrange
        await ResetDatabaseAsync();

        var request = new
        {
            HolderCpf = "12345678901", // Invalid CPF
            HolderName = "Test User",
            HolderEmail = "test@email.com",
            InsuranceType = InsuranceType.Life,
            CoverageAmount = 50000m,
            PremiumAmount = 250m,
            CoverageStartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            CoverageEndDate = DateOnly.FromDateTime(DateTime.Today.AddYears(1)),
            Description = "Test"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/proposals", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateProposal_WithInvalidCoverageAmount_ShouldReturnBadRequest()
    {
        // Arrange
        await ResetDatabaseAsync();

        var request = new
        {
            HolderCpf = "12345678909",
            HolderName = "Test User",
            HolderEmail = "test@email.com",
            InsuranceType = InsuranceType.Life,
            CoverageAmount = 0m, // Invalid
            PremiumAmount = 250m,
            CoverageStartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            CoverageEndDate = DateOnly.FromDateTime(DateTime.Today.AddYears(1)),
            Description = "Test"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/proposals", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Get Proposal Tests

    [Fact]
    public async Task GetProposalById_WithExistingId_ShouldReturnProposal()
    {
        // Arrange
        await ResetDatabaseAsync();

        // First create a proposal
        var createRequest = new
        {
            HolderCpf = "12345678909",
            HolderName = "Maria Santos",
            HolderEmail = "maria@email.com",
            InsuranceType = InsuranceType.Auto,
            CoverageAmount = 75000m,
            PremiumAmount = 350m,
            CoverageStartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            CoverageEndDate = DateOnly.FromDateTime(DateTime.Today.AddYears(1)),
            Description = "Test proposal"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/proposals", createRequest);
        var createdProposal = await createResponse.Content.ReadFromJsonAsync<ProposalResponse>();

        // Act
        var response = await _client.GetAsync($"/api/proposals/{createdProposal!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var proposal = await response.Content.ReadFromJsonAsync<ProposalResponse>();
        proposal.Should().NotBeNull();
        proposal!.Id.Should().Be(createdProposal.Id);
        proposal.HolderName.Should().Be("Maria Santos");
    }

    [Fact]
    public async Task GetProposalById_WithNonExistingId_ShouldReturnNotFound()
    {
        // Arrange
        await ResetDatabaseAsync();
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/proposals/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllProposals_ShouldReturnListOfProposals()
    {
        // Arrange
        await ResetDatabaseAsync();

        // Create multiple proposals
        var proposal1 = new
        {
            HolderCpf = "12345678909",
            HolderName = "User 1",
            HolderEmail = "user1@email.com",
            InsuranceType = InsuranceType.Life,
            CoverageAmount = 100000m,
            PremiumAmount = 500m,
            CoverageStartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            CoverageEndDate = DateOnly.FromDateTime(DateTime.Today.AddYears(1)),
            Description = "Proposal 1"
        };

        var proposal2 = new
        {
            HolderCpf = "98765432100",
            HolderName = "User 2",
            HolderEmail = "user2@email.com",
            InsuranceType = InsuranceType.Home,
            CoverageAmount = 200000m,
            PremiumAmount = 800m,
            CoverageStartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            CoverageEndDate = DateOnly.FromDateTime(DateTime.Today.AddYears(1)),
            Description = "Proposal 2"
        };

        await _client.PostAsJsonAsync("/api/proposals", proposal1);
        await _client.PostAsJsonAsync("/api/proposals", proposal2);

        // Act
        var response = await _client.GetAsync("/api/proposals");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var proposals = await response.Content.ReadFromJsonAsync<ProposalResponse[]>();
        proposals.Should().NotBeNull();
        proposals!.Length.Should().BeGreaterOrEqualTo(2);
    }

    #endregion

    #region Approve Proposal Tests

    [Fact]
    public async Task ApproveProposal_WithValidProposal_ShouldApproveAndPersist()
    {
        // Arrange
        await ResetDatabaseAsync();

        var createRequest = new
        {
            HolderCpf = "12345678909",
            HolderName = "Test User",
            HolderEmail = "test@email.com",
            InsuranceType = InsuranceType.Life,
            CoverageAmount = 100000m,
            PremiumAmount = 500m,
            CoverageStartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            CoverageEndDate = DateOnly.FromDateTime(DateTime.Today.AddYears(1)),
            Description = "Test"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/proposals", createRequest);
        var createdProposal = await createResponse.Content.ReadFromJsonAsync<ProposalResponse>();

        // Act
        var response = await _client.PutAsync($"/api/proposals/{createdProposal!.Id}/approve", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var approvedProposal = await response.Content.ReadFromJsonAsync<ProposalResponse>();
        approvedProposal.Should().NotBeNull();
        approvedProposal!.Status.Should().Be("Approved");

        // Verify in database
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PropostaDbContext>();
        var proposalInDb = await dbContext.Proposals.FindAsync(createdProposal.Id);
        proposalInDb!.Status.Should().Be(ProposalStatus.Approved);
    }

    [Fact]
    public async Task ApproveProposal_WithAlreadyApprovedProposal_ShouldReturnBadRequest()
    {
        // Arrange
        await ResetDatabaseAsync();

        var createRequest = new
        {
            HolderCpf = "12345678909",
            HolderName = "Test User",
            HolderEmail = "test@email.com",
            InsuranceType = InsuranceType.Life,
            CoverageAmount = 100000m,
            PremiumAmount = 500m,
            CoverageStartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            CoverageEndDate = DateOnly.FromDateTime(DateTime.Today.AddYears(1)),
            Description = "Test"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/proposals", createRequest);
        var createdProposal = await createResponse.Content.ReadFromJsonAsync<ProposalResponse>();

        // First approval
        await _client.PutAsync($"/api/proposals/{createdProposal!.Id}/approve", null);

        // Act - Second approval attempt
        var response = await _client.PutAsync($"/api/proposals/{createdProposal.Id}/approve", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Reject Proposal Tests

    [Fact]
    public async Task RejectProposal_WithValidProposal_ShouldRejectAndPersist()
    {
        // Arrange
        await ResetDatabaseAsync();

        var createRequest = new
        {
            HolderCpf = "12345678909",
            HolderName = "Test User",
            HolderEmail = "test@email.com",
            InsuranceType = InsuranceType.Life,
            CoverageAmount = 100000m,
            PremiumAmount = 500m,
            CoverageStartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            CoverageEndDate = DateOnly.FromDateTime(DateTime.Today.AddYears(1)),
            Description = "Test"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/proposals", createRequest);
        var createdProposal = await createResponse.Content.ReadFromJsonAsync<ProposalResponse>();

        var rejectRequest = new { Reason = "Risk assessment failed" };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/proposals/{createdProposal!.Id}/reject", rejectRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var rejectedProposal = await response.Content.ReadFromJsonAsync<ProposalResponse>();
        rejectedProposal.Should().NotBeNull();
        rejectedProposal!.Status.Should().Be("Rejected");

        // Verify in database
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PropostaDbContext>();
        var proposalInDb = await dbContext.Proposals.FindAsync(createdProposal.Id);
        proposalInDb!.Status.Should().Be(ProposalStatus.Rejected);
        proposalInDb.RejectionReason.Should().Be("Risk assessment failed");
    }

    #endregion

    #region Health Check Tests

    [Fact]
    public async Task HealthCheck_ShouldReturnHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion
}

/// <summary>
/// DTO for proposal responses in integration tests.
/// </summary>
public record ProposalResponse
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
}
