namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>
/// Event raised when a new sale is successfully created.
/// </summary>
public class SaleCreatedEvent
{
    public Guid SaleId { get; }
    public string SaleNumber { get; }
    public Guid CustomerId { get; }
    public decimal TotalAmount { get; }
    public DateTime OccurredAt { get; }

    public SaleCreatedEvent(Guid saleId, string saleNumber, Guid customerId, decimal totalAmount)
    {
        SaleId = saleId;
        SaleNumber = saleNumber;
        CustomerId = customerId;
        TotalAmount = totalAmount;
        OccurredAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Event raised when an existing sale is modified (updated).
/// </summary>
public class SaleModifiedEvent
{
    public Guid SaleId { get; }
    public string SaleNumber { get; }
    public decimal TotalAmount { get; }
    public DateTime OccurredAt { get; }

    public SaleModifiedEvent(Guid saleId, string saleNumber, decimal totalAmount)
    {
        SaleId = saleId;
        SaleNumber = saleNumber;
        TotalAmount = totalAmount;
        OccurredAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Event raised when an entire sale is cancelled.
/// </summary>
public class SaleCancelledEvent
{
    public Guid SaleId { get; }
    public string SaleNumber { get; }
    public DateTime OccurredAt { get; }

    public SaleCancelledEvent(Guid saleId, string saleNumber)
    {
        SaleId = saleId;
        SaleNumber = saleNumber;
        OccurredAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Event raised when a specific item within a sale is cancelled.
/// </summary>
public class ItemCancelledEvent
{
    public Guid SaleId { get; }
    public Guid ItemId { get; }
    public Guid ProductId { get; }
    public DateTime OccurredAt { get; }

    public ItemCancelledEvent(Guid saleId, Guid itemId, Guid productId)
    {
        SaleId = saleId;
        ItemId = itemId;
        ProductId = productId;
        OccurredAt = DateTime.UtcNow;
    }
}
