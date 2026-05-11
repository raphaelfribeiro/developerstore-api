using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

/// <summary>
/// Repository interface for Sale entity operations.
/// </summary>
public interface ISaleRepository
{
    /// <summary>
    /// Creates a new sale in the repository.
    /// </summary>
    Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a sale by its unique identifier, including all items.
    /// </summary>
    Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a sale by its sale number.
    /// </summary>
    Task<Sale?> GetBySaleNumberAsync(string saleNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a queryable of all sales for pagination, filtering and ordering.
    /// </summary>
    IQueryable<Sale> GetAllQueryable();

    /// <summary>
    /// Updates an existing sale.
    /// </summary>
    Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a sale by its unique identifier.
    /// </summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

}
