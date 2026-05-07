using FluentValidation;
using MediatR;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;

// ── Command ───────────────────────────────────────────────────────────────────

public record DeleteSaleCommand(Guid Id) : IRequest<DeleteSaleResult>;

// ── Validator ─────────────────────────────────────────────────────────────────

public class DeleteSaleValidator : AbstractValidator<DeleteSaleCommand>
{
    public DeleteSaleValidator()
    {
        RuleFor(c => c.Id).NotEmpty().WithMessage("Sale ID is required.");
    }
}

// ── Result ────────────────────────────────────────────────────────────────────

public class DeleteSaleResult
{
    public bool Success { get; set; }
}

// ── Handler ───────────────────────────────────────────────────────────────────

public class DeleteSaleHandler : IRequestHandler<DeleteSaleCommand, DeleteSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IEventPublisher _eventPublisher;

    public DeleteSaleHandler(ISaleRepository saleRepository, IEventPublisher eventPublisher)
    {
        _saleRepository = saleRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<DeleteSaleResult> Handle(DeleteSaleCommand command, CancellationToken cancellationToken)
    {
        var validation = await new DeleteSaleValidator().ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var sale = await _saleRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale with ID {command.Id} not found.");

        // Business: cancel the sale before removing (triggers SaleCancelledEvent)
        sale.Cancel();
        await _saleRepository.UpdateAsync(sale, cancellationToken);
        await _saleRepository.DeleteAsync(command.Id, cancellationToken);

        await _eventPublisher.PublishAsync(
            new SaleCancelledEvent(sale.Id, sale.SaleNumber),
            cancellationToken);

        return new DeleteSaleResult { Success = true };
    }
}
