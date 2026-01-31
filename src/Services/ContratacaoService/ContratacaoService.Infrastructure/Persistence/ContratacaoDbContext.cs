using BuildingBlocks.Infrastructure.Persistence;
using ContratacaoService.Application.Abstractions;
using ContratacaoService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContratacaoService.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for ContratacaoService.
/// </summary>
public sealed class ContratacaoDbContext : BaseDbContext, IUnitOfWork
{
    public ContratacaoDbContext(DbContextOptions<ContratacaoDbContext> options, IPublisher publisher)
        : base(options, publisher)
    {
    }

    public DbSet<Contract> Contracts => Set<Contract>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ContratacaoDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
