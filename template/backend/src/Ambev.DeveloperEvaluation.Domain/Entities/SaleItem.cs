using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Exceptions;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Represents an individual item within a sale.
/// Contains product reference (External Identity), quantity, pricing and discount information.
/// </summary>
public class SaleItem : BaseEntity
{
    /// <summary>
    /// Reference to the parent sale.
    /// </summary>
    public Guid SaleId { get; set; }

    /// <summary>
    /// External identity: product unique identifier.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// External identity: product name (denormalized for read performance).
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Quantity of the product in this sale item.
    /// Business rule: maximum 20 per product.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Unit price of the product at the time of the sale.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Discount percentage applied to this item (0, 0.10, or 0.20).
    /// </summary>
    public decimal Discount { get; private set; }

    /// <summary>
    /// Total amount for this item after discount: (UnitPrice * Quantity) * (1 - Discount).
    /// </summary>
    public decimal TotalAmount { get; private set; }

    /// <summary>
    /// Indicates whether this item has been individually cancelled.
    /// </summary>
    public bool IsCancelled { get; private set; }

    public SaleItem()
    {
        IsCancelled = false;
    }

    /// <summary>
    /// Applies the given discount and recalculates the total amount.
    /// </summary>
    /// <param name="discount">Discount percentage (e.g. 0.10 for 10%).</param>
    /// <exception cref="DomainException">Thrown when a discount is applied to fewer than 4 items.</exception>
    public void ApplyDiscount(decimal discount)
    {
        if (discount > 0 && Quantity < 4)
            throw new DomainException(
                $"Discount cannot be applied to fewer than 4 identical items. Quantity: {Quantity}.");

        Discount = discount;
        RecalculateTotalAmount();
    }

    /// <summary>
    /// Cancels this sale item.
    /// </summary>
    public void Cancel()
    {
        IsCancelled = true;
    }

    /// <summary>
    /// Recalculates the total amount based on current quantity, unit price and discount.
    /// </summary>
    private void RecalculateTotalAmount()
    {
        TotalAmount = UnitPrice * Quantity * (1 - Discount);
    }
}
