using Microsoft.EntityFrameworkCore;
using PropostaService.Domain.Entities;
using PropostaService.Domain.Repositories;

namespace PropostaService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Proposal aggregate.
/// </summary>
public sealed class ProposalRepository : IProposalRepository
{
    private readonly PropostaDbContext _context;

    public ProposalRepository(PropostaDbContext context)
    {
        _context = context;
    }

    public async Task<Proposal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Proposals
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Proposal?> GetByNumberAsync(string proposalNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Proposals
            .FirstOrDefaultAsync(p => p.ProposalNumber == proposalNumber, cancellationToken);
    }

    public async Task<IReadOnlyList<Proposal>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Proposals
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Proposal>> GetByHolderCpfAsync(string cpf, CancellationToken cancellationToken = default)
    {
        return await _context.Proposals
            .AsNoTracking()
            .Where(p => p.HolderCpf.Value == cpf)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Proposal proposal, CancellationToken cancellationToken = default)
    {
        await _context.Proposals.AddAsync(proposal, cancellationToken);
    }

    public void Update(Proposal proposal)
    {
        _context.Proposals.Update(proposal);
    }

    public void Delete(Proposal proposal)
    {
        _context.Proposals.Remove(proposal);
    }
}
