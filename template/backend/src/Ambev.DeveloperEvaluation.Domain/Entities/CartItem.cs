using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Exceptions;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Represents a product entry within a shopping cart.
/// </summary>
public class CartItem : BaseEntity
{
    /// <summary>
    /// Reference to the parent cart.
    /// </summary>
    public Guid CartId { get; set; }

    /// <summary>
    /// External identity: product unique identifier.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Desired quantity of the product.
    /// </summary>
    public int Quantity { get; private set; }

    public CartItem()
    {
    }

    /// <summary>
    /// Updates the quantity of this cart item.
    /// </summary>
    /// <param name="quantity">New quantity value.</param>
    /// <exception cref="DomainException">Thrown when quantity is zero or negative.</exception>
    public void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException(
                $"Cart item quantity must be greater than zero. Received: {quantity}.");

        Quantity = quantity;
    }

    /// <summary>
    /// Factory method to create a valid CartItem.
    /// </summary>
    public static CartItem Create(Guid productId, int quantity)
    {
        if (quantity <= 0)
            throw new DomainException(
                $"Cart item quantity must be greater than zero. Received: {quantity}.");

        return new CartItem
        {
            ProductId = productId,
            Quantity = quantity
        };
    }
}
