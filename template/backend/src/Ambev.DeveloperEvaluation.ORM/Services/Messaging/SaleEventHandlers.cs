using Ambev.DeveloperEvaluation.Domain.Events;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Ambev.DeveloperEvaluation.ORM.Services.Messaging;

public class SaleCreatedEventHandler : IHandleMessages<SaleCreatedEvent>
{
    private readonly ILogger<SaleCreatedEventHandler> _logger;

    public SaleCreatedEventHandler(ILogger<SaleCreatedEventHandler> logger) => _logger = logger;

    public Task Handle(SaleCreatedEvent message)
    {
        _logger.LogInformation(
            "SaleCreated: SaleId={SaleId} SaleNumber={SaleNumber} CustomerId={CustomerId} TotalAmount={TotalAmount}",
            message.SaleId, message.SaleNumber, message.CustomerId, message.TotalAmount);
        return Task.CompletedTask;
    }
}

public class SaleModifiedEventHandler : IHandleMessages<SaleModifiedEvent>
{
    private readonly ILogger<SaleModifiedEventHandler> _logger;

    public SaleModifiedEventHandler(ILogger<SaleModifiedEventHandler> logger) => _logger = logger;

    public Task Handle(SaleModifiedEvent message)
    {
        _logger.LogInformation(
            "SaleModified: SaleId={SaleId} SaleNumber={SaleNumber} TotalAmount={TotalAmount}",
            message.SaleId, message.SaleNumber, message.TotalAmount);
        return Task.CompletedTask;
    }
}

public class SaleCancelledEventHandler : IHandleMessages<SaleCancelledEvent>
{
    private readonly ILogger<SaleCancelledEventHandler> _logger;

    public SaleCancelledEventHandler(ILogger<SaleCancelledEventHandler> logger) => _logger = logger;

    public Task Handle(SaleCancelledEvent message)
    {
        _logger.LogInformation(
            "SaleCancelled: SaleId={SaleId} SaleNumber={SaleNumber}",
            message.SaleId, message.SaleNumber);
        return Task.CompletedTask;
    }
}

public class ItemCancelledEventHandler : IHandleMessages<ItemCancelledEvent>
{
    private readonly ILogger<ItemCancelledEventHandler> _logger;

    public ItemCancelledEventHandler(ILogger<ItemCancelledEventHandler> logger) => _logger = logger;

    public Task Handle(ItemCancelledEvent message)
    {
        _logger.LogInformation(
            "ItemCancelled: SaleId={SaleId} ItemId={ItemId} ProductId={ProductId}",
            message.SaleId, message.ItemId, message.ProductId);
        return Task.CompletedTask;
    }
}
