using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

/// <summary>
/// Implementation of ICartRepository using Entity Framework Core.
/// </summary>
public class CartRepository : ICartRepository
{
    private readonly DefaultContext _context;

    public CartRepository(DefaultContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<Cart> CreateAsync(Cart cart, CancellationToken cancellationToken = default)
    {
        await _context.Carts.AddAsync(cart, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return cart;
    }

    /// <inheritdoc/>
    public async Task<Cart?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    /// <inheritdoc/>
    public IQueryable<Cart> GetAllQueryable()
    {
        return _context.Carts
            .Include(c => c.Items)
            .AsNoTracking()
            .AsQueryable();
    }

    /// <inheritdoc/>
    public async Task<Cart> UpdateAsync(Cart cart, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == cart.Id, cancellationToken);

        if (existing is null)
            throw new InvalidOperationException($"Cart {cart.Id} not found for update.");

        // Remove old items explicitly
        foreach (var item in existing.Items.ToList())
            _context.Entry(item).State = EntityState.Deleted;

        await _context.SaveChangesAsync(cancellationToken);

        // Update scalar properties
        existing.UserId = cart.UserId;
        existing.Date = cart.Date;
        existing.UpdatedAt = cart.UpdatedAt;

        // Add new items with guaranteed new IDs
        foreach (var item in cart.Items)
        {
            var newItem = new CartItem
            {
                Id = Guid.NewGuid(),
                CartId = existing.Id,
                ProductId = item.ProductId
            };
            newItem.UpdateQuantity(item.Quantity);
            _context.CartItems.Add(newItem);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return existing;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cart = await GetByIdAsync(id, cancellationToken);
        if (cart is null)
            return false;

        _context.Carts.Remove(cart);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
