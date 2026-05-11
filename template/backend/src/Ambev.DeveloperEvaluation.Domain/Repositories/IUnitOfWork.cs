namespace Ambev.DeveloperEvaluation.Domain.Repositories;

/// <summary>
/// Represents the Unit of Work pattern contract.
/// Callers should invoke CommitAsync after all repository operations
/// to persist the accumulated changes in a single atomic transaction.
/// </summary>
public interface IUnitOfWork
{
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
}
