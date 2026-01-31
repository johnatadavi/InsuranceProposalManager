using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IntegrationTests.Factories;
using IntegrationTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using ContratacaoService.Domain.Enums;
using ContratacaoService.Infrastructure.Persistence;
using Respawn;
using Xunit;

namespace IntegrationTests.ContratacaoService;

/// <summary>
/// Integration tests for ContratacaoService API.
/// Uses Testcontainers with real PostgreSQL database.
/// Tests the complete flow: Controller -> UseCase -> Repository -> Database.
/// </summary>
[Collection(nameof(PostgresContainerCollection))]
[Trait("Category", "RequiresDocker")]
public class ContratacaoServiceIntegrationTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _postgresFixture;
    private ContratacaoServiceWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;
    private Respawner? _respawner;

    public ContratacaoServiceIntegrationTests(PostgresContainerFixture postgresFixture)
    {
        _postgresFixture = postgresFixture;
    }

    public async Task InitializeAsync()
    {
        _factory = new ContratacaoServiceWebApplicationFactory(_postgresFixture);
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

    #region Get Contract Tests

    [Fact]
    public async Task GetContractById_WithNonExistingId_ShouldReturnNotFound()
    {
        // Arrange
        await ResetDatabaseAsync();
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/contracts/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllContracts_WhenEmpty_ShouldReturnEmptyList()
    {
        // Arrange
        await ResetDatabaseAsync();

        // Act
        var response = await _client.GetAsync("/api/contracts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var contracts = await response.Content.ReadFromJsonAsync<ContractResponse[]>();
        contracts.Should().NotBeNull();
        contracts!.Should().BeEmpty();
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
/// DTO for contract responses in integration tests.
/// </summary>
public record ContractResponse
{
    public Guid Id { get; init; }
    public string ContractNumber { get; init; } = string.Empty;
    public Guid ProposalId { get; init; }
    public string HolderName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime EffectiveDate { get; init; }
    public DateTime? ExpirationDate { get; init; }
}
