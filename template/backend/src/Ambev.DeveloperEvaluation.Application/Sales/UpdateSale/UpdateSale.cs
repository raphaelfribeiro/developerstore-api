using AutoMapper;
using FluentValidation;
using MediatR;
using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

// ── Command ───────────────────────────────────────────────────────────────────

public class UpdateSaleCommand : IRequest<UpdateSaleResult>
{
    public Guid Id { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public List<UpdateSaleItemCommand> Items { get; set; } = new();

    public ValidationResultDetail Validate()
    {
        var validator = new UpdateSaleValidator();
        var result = validator.Validate(this);
        return new ValidationResultDetail
        {
            IsValid = result.IsValid,
            Errors = result.Errors.Select(o => (ValidationErrorDetail)o)
        };
    }
}

public class UpdateSaleItemCommand
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

// ── Validator ─────────────────────────────────────────────────────────────────

public class UpdateSaleValidator : AbstractValidator<UpdateSaleCommand>
{
    public UpdateSaleValidator()
    {
        RuleFor(s => s.Id).NotEmpty();
        RuleFor(s => s.SaleNumber).NotEmpty().MaximumLength(50);
        RuleFor(s => s.SaleDate).NotEmpty();
        RuleFor(s => s.CustomerId).NotEmpty();
        RuleFor(s => s.CustomerName).NotEmpty().MaximumLength(100);
        RuleFor(s => s.BranchId).NotEmpty();
        RuleFor(s => s.BranchName).NotEmpty().MaximumLength(100);
        RuleFor(s => s.Items).NotEmpty();
        RuleForEach(s => s.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty();
            item.RuleFor(i => i.ProductName).NotEmpty().MaximumLength(200);
            item.RuleFor(i => i.Quantity).GreaterThan(0).LessThanOrEqualTo(20);
            item.RuleFor(i => i.UnitPrice).GreaterThan(0);
        });
    }
}

// ── Result ────────────────────────────────────────────────────────────────────

public class UpdateSaleResult
{
    public Guid Id { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public bool IsCancelled { get; set; }
    public List<UpdateSaleItemResult> Items { get; set; } = new();
}

public class UpdateSaleItemResult
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalAmount { get; set; }
}

// ── Profile ───────────────────────────────────────────────────────────────────

public class UpdateSaleProfile : Profile
{
    public UpdateSaleProfile()
    {
        CreateMap<UpdateSaleItemCommand, SaleItem>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()));
        CreateMap<Sale, UpdateSaleResult>();
        CreateMap<SaleItem, UpdateSaleItemResult>();
    }
}

// ── Handler ───────────────────────────────────────────────────────────────────

public class UpdateSaleHandler : IRequestHandler<UpdateSaleCommand, UpdateSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;
    private readonly IMapper _mapper;

    public UpdateSaleHandler(
        ISaleRepository saleRepository,
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher,
        IMapper mapper)
    {
        _saleRepository = saleRepository;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _mapper = mapper;
    }

    public async Task<UpdateSaleResult> Handle(UpdateSaleCommand command, CancellationToken cancellationToken)
    {
        await ValidateCommandAsync(command, cancellationToken);

        var sale = await _saleRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale with ID {command.Id} not found.");

        UpdateSaleHeader(sale, command);

        var newItems = _mapper.Map<List<SaleItem>>(command.Items);
        sale.ReplaceItems(newItems); // re-applies discount rules

        var updated = await _saleRepository.UpdateAsync(sale, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        await _eventPublisher.PublishAsync(
            new SaleModifiedEvent(updated.Id, updated.SaleNumber, updated.TotalAmount),
            cancellationToken);

        return _mapper.Map<UpdateSaleResult>(updated);
    }

    private static async Task ValidateCommandAsync(UpdateSaleCommand command, CancellationToken ct)
    {
        var result = await new UpdateSaleValidator().ValidateAsync(command, ct);
        if (!result.IsValid)
            throw new ValidationException(result.Errors);
    }

    private static void UpdateSaleHeader(Sale sale, UpdateSaleCommand command)
    {
        sale.SaleNumber = command.SaleNumber;
        sale.SaleDate = command.SaleDate;
        sale.CustomerId = command.CustomerId;
        sale.CustomerName = command.CustomerName;
        sale.BranchId = command.BranchId;
        sale.BranchName = command.BranchName;
    }
}
