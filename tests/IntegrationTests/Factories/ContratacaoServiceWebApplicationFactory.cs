using IntegrationTests.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ContratacaoService.Infrastructure.Persistence;
using Xunit;

namespace IntegrationTests.Factories;

/// <summary>
/// Custom WebApplicationFactory for ContratacaoService that uses Testcontainers PostgreSQL.
/// </summary>
public class ContratacaoServiceWebApplicationFactory : WebApplicationFactory<global::ContratacaoService.Api.Program>, IAsyncLifetime
{
    private readonly PostgresContainerFixture _postgresFixture;
    private readonly string? _propostaServiceUrl;
    private IServiceScope? _scope;

    public ContratacaoServiceWebApplicationFactory(
        PostgresContainerFixture postgresFixture,
        string? propostaServiceUrl = null)
    {
        _postgresFixture = postgresFixture;
        _propostaServiceUrl = propostaServiceUrl;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registrations
            RemoveDbContextRegistrations(services);

            // Add DbContext with Testcontainers connection string
            services.AddDbContext<ContratacaoDbContext>((sp, options) =>
            {
                options.UseNpgsql(_postgresFixture.ConnectionString);
            });
        });

        // Configure PropostaService URL if provided
        if (!string.IsNullOrEmpty(_propostaServiceUrl))
        {
            builder.UseSetting("Services:PropostaService:Url", _propostaServiceUrl);
        }
    }

    private static void RemoveDbContextRegistrations(IServiceCollection services)
    {
        var descriptorsToRemove = services
            .Where(d => d.ServiceType == typeof(DbContextOptions<ContratacaoDbContext>) ||
                       d.ServiceType == typeof(ContratacaoDbContext))
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
        var dbContext = _scope.ServiceProvider.GetRequiredService<ContratacaoDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync()
    {
        _scope?.Dispose();
        await base.DisposeAsync();
    }
}
