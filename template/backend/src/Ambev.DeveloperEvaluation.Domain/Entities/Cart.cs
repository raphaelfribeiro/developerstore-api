using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Validation;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Represents a shopping cart associated with a user.
/// A cart holds product references and quantities prior to a sale being finalized.
/// </summary>
public class Cart : BaseEntity
{
    /// <summary>
    /// External identity: user unique identifier.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Date associated with the cart (creation or last modification date).
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Date and time when the cart was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date and time of the last update.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    private readonly List<CartItem> _items = new();

    /// <summary>
    /// Read-only collection of items in this cart.
    /// </summary>
    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    public Cart()
    {
        Date = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Adds or updates an item in the cart.
    /// If the product already exists in the cart, its quantity is updated.
    /// </summary>
    /// <param name="item">The cart item to add.</param>
    public void AddItem(CartItem item)
    {
        var existing = _items.FirstOrDefault(i => i.ProductId == item.ProductId);
        if (existing is not null)
        {
            existing.UpdateQuantity(item.Quantity);
        }
        else
        {
            item.CartId = Id;
            _items.Add(item);
        }

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Replaces all items in the cart with the provided list.
    /// </summary>
    /// <param name="items">The new list of cart items.</param>
    public void UpdateItems(IEnumerable<CartItem> items)
    {
        _items.Clear();
        foreach (var item in items)
        {
            item.CartId = Id;
            _items.Add(item);
        }

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Performs validation of the cart entity using CartValidator rules.
    /// </summary>
    public ValidationResultDetail Validate()
    {
        var validator = new CartValidator();
        var result = validator.Validate(this);
        return new ValidationResultDetail
        {
            IsValid = result.IsValid,
            Errors = result.Errors.Select(o => (ValidationErrorDetail)o)
        };
    }
}
