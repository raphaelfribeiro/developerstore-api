using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Validation;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Represents a sale record in the system.
/// Follows DDD principles with business rules centralized in the entity.
/// Uses the External Identities pattern for Customer and Branch references.
/// </summary>
public class Sale : BaseEntity
{
    /// <summary>
    /// Unique sequential sale number for human-readable identification.
    /// </summary>
    public string SaleNumber { get; set; } = string.Empty;

    /// <summary>
    /// Date and time when the sale was made.
    /// </summary>
    public DateTime SaleDate { get; set; }

    /// <summary>
    /// External identity: customer unique identifier (denormalized reference).
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// External identity: customer name (denormalized for read performance).
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// External identity: branch unique identifier (denormalized reference).
    /// </summary>
    public Guid BranchId { get; set; }

    /// <summary>
    /// External identity: branch name (denormalized for read performance).
    /// </summary>
    public string BranchName { get; set; } = string.Empty;

    /// <summary>
    /// Total amount of the sale, calculated from all active items.
    /// </summary>
    public decimal TotalAmount { get; private set; }

    /// <summary>
    /// Indicates whether the sale has been cancelled.
    /// </summary>
    public bool IsCancelled { get; private set; }

    /// <summary>
    /// Date and time when the sale was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date and time of the last update.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    private readonly List<SaleItem> _items = new();

    /// <summary>
    /// Read-only collection of items belonging to this sale.
    /// </summary>
    public IReadOnlyCollection<SaleItem> Items => _items.AsReadOnly();

    public Sale()
    {
        CreatedAt = DateTime.UtcNow;
        IsCancelled = false;
    }

    /// <summary>
    /// Adds an item to the sale, applying quantity-based discount rules.
    /// </summary>
    /// <param name="item">The sale item to add.</param>
    /// <exception cref="DomainException">Thrown when quantity exceeds 20 units per product.</exception>
    public void AddItem(SaleItem item)
    {
        ValidateItemQuantity(item.Quantity);
        item.ApplyDiscount(CalculateDiscount(item.Quantity));
        _items.Add(item);
        RecalculateTotalAmount();
    }

    /// <summary>
    /// Cancels the entire sale and all its items.
    /// </summary>
    public void Cancel()
    {
        IsCancelled = true;
        foreach (var item in _items)
            item.Cancel();

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cancels a specific item within the sale.
    /// </summary>
    /// <param name="itemId">The unique identifier of the item to cancel.</param>
    /// <exception cref="DomainException">Thrown when the item is not found.</exception>
    public void CancelItem(Guid itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new DomainException($"Item {itemId} not found in sale {Id}.");

        item.Cancel();
        RecalculateTotalAmount();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Recalculates the total amount based on active (non-cancelled) items.
    /// </summary>
    public void RecalculateTotalAmount()
    {
        TotalAmount = _items
            .Where(i => !i.IsCancelled)
            .Sum(i => i.TotalAmount);
    }

    /// <summary>
    /// Performs validation of the sale entity using SaleValidator rules.
    /// </summary>
    public ValidationResultDetail Validate()
    {
        var validator = new SaleValidator();
        var result = validator.Validate(this);
        return new ValidationResultDetail
        {
            IsValid = result.IsValid,
            Errors = result.Errors.Select(o => (ValidationErrorDetail)o)
        };
    }

    /// <summary>
    /// Business rule: calculates the discount percentage based on quantity.
    /// - Below 4 items: no discount (0%)
    /// - 4 to 9 items: 10% discount
    /// - 10 to 20 items: 20% discount
    /// </summary>
    public static decimal CalculateDiscount(int quantity)
    {
        return quantity switch
        {
            >= 10 and <= 20 => 0.20m,
            >= 4 => 0.10m,
            _ => 0m
        };
    }

    /// <summary>
    /// Business rule: validates that quantity does not exceed the maximum of 20 per product.
    /// </summary>
    /// <param name="quantity">The quantity to validate.</param>
    /// <exception cref="DomainException">Thrown when quantity exceeds 20.</exception>
    private static void ValidateItemQuantity(int quantity)
    {
        if (quantity > 20)
            throw new DomainException(
                $"Cannot sell more than 20 identical items. Requested: {quantity}.");

        if (quantity <= 0)
            throw new DomainException(
                $"Item quantity must be greater than zero. Received: {quantity}.");
    }
}
