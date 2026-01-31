using ContratacaoService.Domain.Entities;
using ContratacaoService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ContratacaoService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Contract aggregate.
/// </summary>
public sealed class ContractRepository : IContractRepository
{
    private readonly ContratacaoDbContext _context;

    public ContractRepository(ContratacaoDbContext context)
    {
        _context = context;
    }

    public async Task<Contract?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Contracts
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Contract?> GetByProposalIdAsync(Guid proposalId, CancellationToken cancellationToken = default)
    {
        return await _context.Contracts
            .FirstOrDefaultAsync(c => c.ProposalId == proposalId, cancellationToken);
    }

    public async Task<Contract?> GetByNumberAsync(string contractNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Contracts
            .FirstOrDefaultAsync(c => c.ContractNumber == contractNumber, cancellationToken);
    }

    public async Task<IReadOnlyList<Contract>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Contracts
            .AsNoTracking()
            .OrderByDescending(c => c.ContractedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Contract>> GetByHolderCpfAsync(string cpf, CancellationToken cancellationToken = default)
    {
        return await _context.Contracts
            .AsNoTracking()
            .Where(c => c.HolderCpf == cpf)
            .OrderByDescending(c => c.ContractedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByProposalIdAsync(Guid proposalId, CancellationToken cancellationToken = default)
    {
        return await _context.Contracts
            .AnyAsync(c => c.ProposalId == proposalId, cancellationToken);
    }

    public async Task AddAsync(Contract contract, CancellationToken cancellationToken = default)
    {
        await _context.Contracts.AddAsync(contract, cancellationToken);
    }

    public void Update(Contract contract)
    {
        _context.Contracts.Update(contract);
    }
}
