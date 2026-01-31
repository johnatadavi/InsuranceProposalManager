using IntegrationTests.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PropostaService.Infrastructure.Persistence;
using Xunit;

namespace IntegrationTests.Factories;

/// <summary>
/// Custom WebApplicationFactory for PropostaService that uses Testcontainers PostgreSQL.
/// </summary>
public class PropostaServiceWebApplicationFactory : WebApplicationFactory<global::PropostaService.Api.Program>, IAsyncLifetime
{
    private readonly PostgresContainerFixture _postgresFixture;
    private IServiceScope? _scope;

    public PropostaServiceWebApplicationFactory(PostgresContainerFixture postgresFixture)
    {
        _postgresFixture = postgresFixture;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registrations
            RemoveDbContextRegistrations(services);

            // Add DbContext with Testcontainers connection string
            services.AddDbContext<PropostaDbContext>((sp, options) =>
            {
                options.UseNpgsql(_postgresFixture.ConnectionString);
            });
        });
    }

    private static void RemoveDbContextRegistrations(IServiceCollection services)
    {
        var descriptorsToRemove = services
            .Where(d => d.ServiceType == typeof(DbContextOptions<PropostaDbContext>) ||
                       d.ServiceType == typeof(PropostaDbContext))
            .ToList();

        foreach (var descriptor in descriptorsToRemove)
        {
            services.Remove(descriptor);
        }
    }

    public async Task InitializeAsync()
    {
        // Ensure database is created and migrated
        _scope = Services.CreateScope();
        var dbContext = _scope.ServiceProvider.GetRequiredService<PropostaDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync()
    {
        _scope?.Dispose();
        await base.DisposeAsync();
    }
}
