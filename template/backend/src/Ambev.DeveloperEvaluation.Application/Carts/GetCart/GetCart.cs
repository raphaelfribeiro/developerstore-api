using AutoMapper;
using FluentValidation;
using MediatR;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Carts.GetCart;

public record GetCartQuery(Guid Id) : IRequest<GetCartResult>;

public class GetCartValidator : AbstractValidator<GetCartQuery>
{
    public GetCartValidator()
    {
        RuleFor(q => q.Id).NotEmpty().WithMessage("Cart ID is required.");
    }
}

public class GetCartResult
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime Date { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<GetCartItemResult> Items { get; set; } = new();
}

public class GetCartItemResult
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

public class GetCartProfile : Profile
{
    public GetCartProfile()
    {
        CreateMap<Domain.Entities.Cart, GetCartResult>();
        CreateMap<Domain.Entities.CartItem, GetCartItemResult>();
    }
}

public class GetCartHandler : IRequestHandler<GetCartQuery, GetCartResult>
{
    private readonly ICartRepository _cartRepository;
    private readonly IMapper _mapper;

    public GetCartHandler(ICartRepository cartRepository, IMapper mapper)
    {
        _cartRepository = cartRepository;
        _mapper = mapper;
    }

    public async Task<GetCartResult> Handle(GetCartQuery query, CancellationToken cancellationToken)
    {
        var validation = await new GetCartValidator().ValidateAsync(query, cancellationToken);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var cart = await _cartRepository.GetByIdAsync(query.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Cart with ID {query.Id} not found.");

        return _mapper.Map<GetCartResult>(cart);
    }
}
