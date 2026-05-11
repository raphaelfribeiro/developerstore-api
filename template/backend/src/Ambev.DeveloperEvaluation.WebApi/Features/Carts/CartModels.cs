using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Carts.CreateCart;
using Ambev.DeveloperEvaluation.Application.Carts.GetCart;
using Ambev.DeveloperEvaluation.Application.Carts.GetCarts;
using Ambev.DeveloperEvaluation.Application.Carts.UpdateCart;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Carts;

// ═══════════════════════════════════════════════════════════════════
// CREATE CART
// ═══════════════════════════════════════════════════════════════════

public class CreateCartRequest
{
    public Guid UserId { get; set; }
    public DateTime Date { get; set; }
    public List<CreateCartItemRequest> Products { get; set; } = new();
}

public class CreateCartItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

public class CreateCartResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime Date { get; set; }
    public List<CreateCartItemResponse> Products { get; set; } = new();
}

public class CreateCartItemResponse
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

public class CreateCartRequestValidator : AbstractValidator<CreateCartRequest>
{
    public CreateCartRequestValidator()
    {
        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.Date).NotEmpty();
        RuleFor(c => c.Products).NotEmpty();
        RuleForEach(c => c.Products).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty();
            item.RuleFor(i => i.Quantity).GreaterThan(0);
        });
    }
}

public class CreateCartProfile : Profile
{
    public CreateCartProfile()
    {
        CreateMap<CreateCartRequest, CreateCartCommand>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Products));
        CreateMap<CreateCartItemRequest, CreateCartItemCommand>();
        CreateMap<CreateCartResult, CreateCartResponse>()
            .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.Items));
        CreateMap<CreateCartItemResult, CreateCartItemResponse>();
    }
}

// ═══════════════════════════════════════════════════════════════════
// GET CART
// ═══════════════════════════════════════════════════════════════════

public class GetCartResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime Date { get; set; }
    public List<GetCartItemResponse> Products { get; set; } = new();
}

public class GetCartItemResponse
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

public class GetCartProfile : Profile
{
    public GetCartProfile()
    {
        CreateMap<GetCartResult, GetCartResponse>()
            .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.Items));
        CreateMap<GetCartItemResult, GetCartItemResponse>();
    }
}

// ═══════════════════════════════════════════════════════════════════
// GET CARTS (list)
// ═══════════════════════════════════════════════════════════════════

public class GetCartsResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime Date { get; set; }
    public int ItemCount { get; set; }
}

public class GetCartsProfile : Profile
{
    public GetCartsProfile()
    {
        CreateMap<GetCartsResult, GetCartsResponse>();
    }
}

// ═══════════════════════════════════════════════════════════════════
// UPDATE CART
// ═══════════════════════════════════════════════════════════════════

public class UpdateCartRequest
{
    public Guid UserId { get; set; }
    public DateTime Date { get; set; }
    public List<UpdateCartItemRequest> Products { get; set; } = new();
}

public class UpdateCartItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

public class UpdateCartResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime Date { get; set; }
    public List<UpdateCartItemResponse> Products { get; set; } = new();
}

public class UpdateCartItemResponse
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

public class UpdateCartRequestValidator : AbstractValidator<UpdateCartRequest>
{
    public UpdateCartRequestValidator()
    {
        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.Date).NotEmpty();
        RuleFor(c => c.Products).NotEmpty();
        RuleForEach(c => c.Products).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty();
            item.RuleFor(i => i.Quantity).GreaterThan(0);
        });
    }
}

public class UpdateCartProfile : Profile
{
    public UpdateCartProfile()
    {
        CreateMap<UpdateCartRequest, UpdateCartCommand>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Products));
        CreateMap<UpdateCartItemRequest, UpdateCartItemCommand>();
        CreateMap<UpdateCartResult, UpdateCartResponse>()
            .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.Items));
        CreateMap<UpdateCartItemResult, UpdateCartItemResponse>();
    }
}
