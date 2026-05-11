using FluentValidation;
using MediatR;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

// ── Command ───────────────────────────────────────────────────────────────────

public record CancelSaleItemCommand(Guid SaleId, Guid ItemId) : IRequest<CancelSaleItemResult>;

// ── Validator ─────────────────────────────────────────────────────────────────

public class CancelSaleItemValidator : AbstractValidator<CancelSaleItemCommand>
{
    public CancelSaleItemValidator()
    {
        RuleFor(c => c.SaleId).NotEmpty().WithMessage("Sale ID is required.");
        RuleFor(c => c.ItemId).NotEmpty().WithMessage("Item ID is required.");
    }
}

// ── Result ────────────────────────────────────────────────────────────────────

public class CancelSaleItemResult
{
    public bool Success { get; set; }
    public Guid SaleId { get; set; }
    public Guid ItemId { get; set; }
    public decimal NewSaleTotal { get; set; }
}

// ── Handler ───────────────────────────────────────────────────────────────────

public class CancelSaleItemHandler : IRequestHandler<CancelSaleItemCommand, CancelSaleItemResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;

    public CancelSaleItemHandler(ISaleRepository saleRepository, IUnitOfWork unitOfWork, IEventPublisher eventPublisher)
    {
        _saleRepository = saleRepository;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
    }

    public async Task<CancelSaleItemResult> Handle(CancelSaleItemCommand command, CancellationToken cancellationToken)
    {
        var validation = await new CancelSaleItemValidator().ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var sale = await _saleRepository.GetByIdAsync(command.SaleId, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale with ID {command.SaleId} not found.");

        var item = sale.Items.FirstOrDefault(i => i.Id == command.ItemId)
            ?? throw new KeyNotFoundException($"Item with ID {command.ItemId} not found in sale {command.SaleId}.");

        sale.CancelItem(command.ItemId);
        await _unitOfWork.CommitAsync(cancellationToken);

        await _eventPublisher.PublishAsync(
            new ItemCancelledEvent(sale.Id, item.Id, item.ProductId),
            cancellationToken);

        return new CancelSaleItemResult
        {
            Success = true,
            SaleId = sale.Id,
            ItemId = item.Id,
            NewSaleTotal = sale.TotalAmount
        };
    }
}
