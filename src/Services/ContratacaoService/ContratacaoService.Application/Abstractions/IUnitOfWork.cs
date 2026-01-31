namespace ContratacaoService.Application.Abstractions;

/// <summary>
/// Unit of Work interface for transaction management.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
