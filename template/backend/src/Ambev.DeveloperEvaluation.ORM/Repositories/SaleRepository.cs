using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

/// <summary>
/// Implementation of ISaleRepository using Entity Framework Core.
/// </summary>
public class SaleRepository : ISaleRepository
{
    private readonly DefaultContext _context;

    public SaleRepository(DefaultContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        await _context.Sales.AddAsync(sale, cancellationToken);
        return sale;
    }

    /// <inheritdoc/>
    public async Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Sale?> GetBySaleNumberAsync(string saleNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.SaleNumber == saleNumber, cancellationToken);
    }

    /// <inheritdoc/>
    public IQueryable<Sale> GetAllQueryable()
    {
        return _context.Sales
            .Include(s => s.Items)
            .AsNoTracking()
            .AsQueryable();
    }

    /// <inheritdoc/>
    public async Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == sale.Id, cancellationToken);

        if (existing is null)
            throw new InvalidOperationException($"Sale {sale.Id} not found for update.");

        // Remove old items
        _context.SaleItems.RemoveRange(existing.Items);

        // Update scalar properties
        existing.SaleNumber = sale.SaleNumber;
        existing.SaleDate = sale.SaleDate;
        existing.CustomerId = sale.CustomerId;
        existing.CustomerName = sale.CustomerName;
        existing.BranchId = sale.BranchId;
        existing.BranchName = sale.BranchName;
        existing.UpdatedAt = sale.UpdatedAt;

        // Add new items
        foreach (var item in sale.Items)
        {
            item.SaleId = existing.Id;
            _context.SaleItems.Add(item);
        }

        // Recalculate total
        existing.RecalculateTotalAmount();

        return existing;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sale = await GetByIdAsync(id, cancellationToken);
        if (sale is null)
            return false;

        _context.Sales.Remove(sale);
        return true;
    }
}
