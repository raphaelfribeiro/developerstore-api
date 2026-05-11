using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.ORM;

/// <summary>
/// EF Core implementation of IUnitOfWork.
/// Delegates CommitAsync to DefaultContext.SaveChangesAsync so all tracked
/// changes from the current request are persisted in a single transaction.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly DefaultContext _context;

    public UnitOfWork(DefaultContext context)
    {
        _context = context;
    }

    public Task<int> CommitAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
