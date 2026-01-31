using PropostaService.Domain.Entities;

namespace PropostaService.Domain.Repositories;

/// <summary>
/// Repository interface for Proposal aggregate.
/// Defines the contract - implementation is in Infrastructure layer.
/// </summary>
public interface IProposalRepository
{
    Task<Proposal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Proposal?> GetByNumberAsync(string proposalNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Proposal>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Proposal>> GetByHolderCpfAsync(string cpf, CancellationToken cancellationToken = default);
    Task AddAsync(Proposal proposal, CancellationToken cancellationToken = default);
    void Update(Proposal proposal);
    void Delete(Proposal proposal);
}
