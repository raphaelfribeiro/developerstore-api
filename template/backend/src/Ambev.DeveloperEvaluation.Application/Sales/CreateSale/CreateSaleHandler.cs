using AutoMapper;
using FluentValidation;
using MediatR;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

/// <summary>
/// Handles the CreateSaleCommand: validates, applies business rules,
/// persists the sale and publishes the SaleCreatedEvent.
/// </summary>
public class CreateSaleHandler : IRequestHandler<CreateSaleCommand, CreateSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly IMapper _mapper;

    public CreateSaleHandler(
        ISaleRepository saleRepository,
        IEventPublisher eventPublisher,
        IMapper mapper)
    {
        _saleRepository = saleRepository;
        _eventPublisher = eventPublisher;
        _mapper = mapper;
    }

    public async Task<CreateSaleResult> Handle(CreateSaleCommand command, CancellationToken cancellationToken)
    {
        await ValidateCommandAsync(command, cancellationToken);

        var sale = BuildSale(command);

        var created = await _saleRepository.CreateAsync(sale, cancellationToken);

        await _eventPublisher.PublishAsync(
            new SaleCreatedEvent(created.Id, created.SaleNumber, created.CustomerId, created.TotalAmount),
            cancellationToken);

        return _mapper.Map<CreateSaleResult>(created);
    }

    private static async Task ValidateCommandAsync(CreateSaleCommand command, CancellationToken cancellationToken)
    {
        var validator = new CreateSaleValidator();
        var result = await validator.ValidateAsync(command, cancellationToken);
        if (!result.IsValid)
            throw new ValidationException(result.Errors);
    }

    private Sale BuildSale(CreateSaleCommand command)
    {
        var sale = new Sale
        {
            SaleNumber = command.SaleNumber,
            SaleDate = command.SaleDate,
            CustomerId = command.CustomerId,
            CustomerName = command.CustomerName,
            BranchId = command.BranchId,
            BranchName = command.BranchName
        };

        foreach (var itemCmd in command.Items)
        {
            var item = _mapper.Map<SaleItem>(itemCmd);
            sale.AddItem(item); // applies discount rules internally
        }

        return sale;
    }
}
