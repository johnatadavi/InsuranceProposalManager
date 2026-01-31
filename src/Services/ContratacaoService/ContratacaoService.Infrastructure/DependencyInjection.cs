using ContratacaoService.Application.Abstractions;
using ContratacaoService.Domain.Repositories;
using ContratacaoService.Infrastructure.ExternalServices;
using ContratacaoService.Infrastructure.Persistence;
using ContratacaoService.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace ContratacaoService.Infrastructure;

/// <summary>
/// Dependency injection configuration for the Infrastructure layer.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddPersistence(configuration);
        services.AddExternalServices(configuration);

        return services;
    }

    private static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ContratacaoDb");
        var databaseProvider = configuration.GetValue<string>("DatabaseProvider") ?? "PostgreSQL";

        services.AddDbContext<ContratacaoDbContext>((sp, options) =>
        {
            if (databaseProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(3);
                    sqlOptions.CommandTimeout(30);
                });
            }
            else
            {
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(3);
                    npgsqlOptions.CommandTimeout(30);
                });
            }
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ContratacaoDbContext>());
        services.AddScoped<IContractRepository, ContractRepository>();

        return services;
    }

    private static IServiceCollection AddExternalServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var propostaServiceUrl = configuration.GetValue<string>("Services:PropostaService:Url")
            ?? "http://localhost:5010";

        // Configure Refit with resilience (retry, circuit breaker, timeout)
        services.AddRefitClient<IProposalApi>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri(propostaServiceUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddStandardResilienceHandler();

        // Register the service implementation
        services.AddScoped<IProposalService, RefitProposalService>();

        return services;
    }
}
