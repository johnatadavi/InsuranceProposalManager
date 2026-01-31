using Testcontainers.PostgreSql;
using Xunit;

namespace IntegrationTests.Fixtures;

/// <summary>
/// Shared PostgreSQL container fixture for integration tests.
/// The container is started once and shared across all tests in the collection.
/// </summary>
public class PostgresContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;

    public string ConnectionString => _container.GetConnectionString();

    public PostgresContainerFixture()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("test_db")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .WithCleanUp(true)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}

/// <summary>
/// Collection definition for tests that share the PostgreSQL container.
/// </summary>
[CollectionDefinition(nameof(PostgresContainerCollection))]
public class PostgresContainerCollection : ICollectionFixture<PostgresContainerFixture>
{
}
