using AutoMapper;
using FluentValidation;
using MediatR;
using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Carts.UpdateCart;

// ── Command ───────────────────────────────────────────────────────────────────

public class UpdateCartCommand : IRequest<UpdateCartResult>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime Date { get; set; }
    public List<UpdateCartItemCommand> Items { get; set; } = new();

    public ValidationResultDetail Validate()
    {
        var validator = new UpdateCartValidator();
        var result = validator.Validate(this);
        return new ValidationResultDetail
        {
            IsValid = result.IsValid,
            Errors = result.Errors.Select(o => (ValidationErrorDetail)o)
        };
    }
}

public class UpdateCartItemCommand
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

// ── Validator ─────────────────────────────────────────────────────────────────

public class UpdateCartValidator : AbstractValidator<UpdateCartCommand>
{
    public UpdateCartValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.Date).NotEmpty();
        RuleFor(c => c.Items).NotEmpty();
        RuleForEach(c => c.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty();
            item.RuleFor(i => i.Quantity).GreaterThan(0);
        });
    }
}

// ── Result ────────────────────────────────────────────────────────────────────

public class UpdateCartResult
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime Date { get; set; }
    public List<UpdateCartItemResult> Items { get; set; } = new();
}

public class UpdateCartItemResult
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

// ── Profile ───────────────────────────────────────────────────────────────────

public class UpdateCartProfile : Profile
{
    public UpdateCartProfile()
    {
        CreateMap<Cart, UpdateCartResult>();
        CreateMap<CartItem, UpdateCartItemResult>();
    }
}

// ── Handler ───────────────────────────────────────────────────────────────────

public class UpdateCartHandler : IRequestHandler<UpdateCartCommand, UpdateCartResult>
{
    private readonly ICartRepository _cartRepository;
    private readonly IMapper _mapper;

    public UpdateCartHandler(ICartRepository cartRepository, IMapper mapper)
    {
        _cartRepository = cartRepository;
        _mapper = mapper;
    }

    public async Task<UpdateCartResult> Handle(UpdateCartCommand command, CancellationToken cancellationToken)
    {
        var validation = await new UpdateCartValidator().ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var cart = await _cartRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Cart with ID {command.Id} not found.");

        cart.UserId = command.UserId;
        cart.Date = command.Date;

        var newItems = command.Items
            .Select(i => CartItem.Create(i.ProductId, i.Quantity))
            .ToList();

        cart.UpdateItems(newItems);

        var updated = await _cartRepository.UpdateAsync(cart, cancellationToken);
        return _mapper.Map<UpdateCartResult>(updated);
    }
}
