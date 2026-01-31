using ContratacaoService.Domain.Entities;

namespace ContratacaoService.Domain.Repositories;

/// <summary>
/// Repository interface for Contract aggregate.
/// </summary>
public interface IContractRepository
{
    Task<Contract?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Contract?> GetByProposalIdAsync(Guid proposalId, CancellationToken cancellationToken = default);
    Task<Contract?> GetByNumberAsync(string contractNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Contract>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Contract>> GetByHolderCpfAsync(string cpf, CancellationToken cancellationToken = default);
    Task<bool> ExistsByProposalIdAsync(Guid proposalId, CancellationToken cancellationToken = default);
    Task AddAsync(Contract contract, CancellationToken cancellationToken = default);
    void Update(Contract contract);
}
