using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PropostaService.Application.Abstractions;
using PropostaService.Domain.Repositories;
using PropostaService.Infrastructure.Persistence;
using PropostaService.Infrastructure.Persistence.Repositories;

namespace PropostaService.Infrastructure;

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
        
        return services;
    }

    private static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PropostaDb");
        var databaseProvider = configuration.GetValue<string>("DatabaseProvider") ?? "PostgreSQL";

        services.AddDbContext<PropostaDbContext>((sp, options) =>
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

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<PropostaDbContext>());
        services.AddScoped<IProposalRepository, ProposalRepository>();

        return services;
    }
}
