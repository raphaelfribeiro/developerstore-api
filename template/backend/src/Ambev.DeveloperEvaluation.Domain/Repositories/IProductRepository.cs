using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

/// <summary>
/// Repository interface for Product entity operations.
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Creates a new product in the repository.
    /// </summary>
    Task<Product> CreateAsync(Product product, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a product by its unique identifier.
    /// </summary>
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a queryable of all products for pagination, filtering and ordering.
    /// </summary>
    IQueryable<Product> GetAllQueryable();

    /// <summary>
    /// Returns a queryable of products filtered by category.
    /// </summary>
    IQueryable<Product> GetByCategoryQueryable(string category);

    /// <summary>
    /// Returns all distinct product category names.
    /// </summary>
    Task<IEnumerable<string>> GetCategoriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    Task<Product> UpdateAsync(Product product, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a product by its unique identifier.
    /// </summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
