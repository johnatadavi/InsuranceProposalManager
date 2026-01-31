using MediatR;
using Microsoft.EntityFrameworkCore;
using PropostaService.Application.Abstractions;
using PropostaService.Domain.Entities;
using BuildingBlocks.Infrastructure.Persistence;

namespace PropostaService.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for PropostaService.
/// </summary>
public sealed class PropostaDbContext : BaseDbContext, IUnitOfWork
{
    public PropostaDbContext(DbContextOptions<PropostaDbContext> options, IPublisher publisher)
        : base(options, publisher)
    {
    }

    public DbSet<Proposal> Proposals => Set<Proposal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PropostaDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
