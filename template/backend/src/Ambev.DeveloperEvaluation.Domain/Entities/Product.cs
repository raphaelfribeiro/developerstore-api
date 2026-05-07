using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Validation;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Represents a product available in the store.
/// Follows the External Identities pattern — products can be referenced
/// from other domains using their Id and denormalized description fields.
/// </summary>
public class Product : BaseEntity
{
    /// <summary>
    /// Product title / name.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Product price.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Detailed product description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Product category name.
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// URL of the product image.
    /// </summary>
    public string Image { get; set; } = string.Empty;

    /// <summary>
    /// Aggregate customer rating for this product.
    /// Stored as an owned entity (value object).
    /// </summary>
    public ProductRating Rating { get; set; } = new();

    /// <summary>
    /// Date and time when the product was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date and time of the last update.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    public Product()
    {
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Performs validation of the product entity using ProductValidator rules.
    /// </summary>
    public ValidationResultDetail Validate()
    {
        var validator = new ProductValidator();
        var result = validator.Validate(this);
        return new ValidationResultDetail
        {
            IsValid = result.IsValid,
            Errors = result.Errors.Select(o => (ValidationErrorDetail)o)
        };
    }
}

/// <summary>
/// Value object representing the aggregate customer rating of a product.
/// Owned by the Product entity and persisted in the same table.
/// </summary>
public class ProductRating
{
    /// <summary>
    /// Average customer rating (0.0 - 5.0).
    /// </summary>
    public decimal Rate { get; set; }

    /// <summary>
    /// Total number of customer ratings received.
    /// </summary>
    public int Count { get; set; }
}
