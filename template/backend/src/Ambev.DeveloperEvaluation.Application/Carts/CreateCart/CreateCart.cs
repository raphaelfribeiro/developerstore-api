using AutoMapper;
using FluentValidation;
using MediatR;
using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Carts.CreateCart;

// ── Command ───────────────────────────────────────────────────────────────────

public class CreateCartCommand : IRequest<CreateCartResult>
{
    public Guid UserId { get; set; }
    public DateTime Date { get; set; }
    public List<CreateCartItemCommand> Items { get; set; } = new();

    public ValidationResultDetail Validate()
    {
        var validator = new CreateCartValidator();
        var result = validator.Validate(this);
        return new ValidationResultDetail
        {
            IsValid = result.IsValid,
            Errors = result.Errors.Select(o => (ValidationErrorDetail)o)
        };
    }
}

public class CreateCartItemCommand
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

// ── Validator ─────────────────────────────────────────────────────────────────

public class CreateCartValidator : AbstractValidator<CreateCartCommand>
{
    public CreateCartValidator()
    {
        RuleFor(c => c.UserId).NotEmpty().WithMessage("User is required.");
        RuleFor(c => c.Date).NotEmpty().WithMessage("Date is required.");
        RuleFor(c => c.Items).NotEmpty().WithMessage("At least one item is required.");
        RuleForEach(c => c.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty();
            item.RuleFor(i => i.Quantity).GreaterThan(0);
        });
    }
}

// ── Result ────────────────────────────────────────────────────────────────────

public class CreateCartResult
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime Date { get; set; }
    public List<CreateCartItemResult> Items { get; set; } = new();
}

public class CreateCartItemResult
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

// ── Profile ───────────────────────────────────────────────────────────────────

public class CreateCartProfile : Profile
{
    public CreateCartProfile()
    {
        CreateMap<CreateCartItemCommand, CartItem>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ConstructUsing(src => CartItem.Create(src.ProductId, src.Quantity));
        CreateMap<Cart, CreateCartResult>();
        CreateMap<CartItem, CreateCartItemResult>();
    }
}

// ── Handler ───────────────────────────────────────────────────────────────────

public class CreateCartHandler : IRequestHandler<CreateCartCommand, CreateCartResult>
{
    private readonly ICartRepository _cartRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateCartHandler(ICartRepository cartRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _cartRepository = cartRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CreateCartResult> Handle(CreateCartCommand command, CancellationToken cancellationToken)
    {
        var validation = await new CreateCartValidator().ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var cart = new Cart { UserId = command.UserId, Date = command.Date };

        foreach (var itemCmd in command.Items)
            cart.AddItem(CartItem.Create(itemCmd.ProductId, itemCmd.Quantity));

        var created = await _cartRepository.CreateAsync(cart, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);
        return _mapper.Map<CreateCartResult>(created);
    }
}
