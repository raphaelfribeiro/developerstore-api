using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Products.CreateProduct;
using Ambev.DeveloperEvaluation.Application.Products.GetProduct;
using Ambev.DeveloperEvaluation.Application.Products.GetProducts;
using Ambev.DeveloperEvaluation.Application.Products.UpdateProduct;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Products;

// ═══════════════════════════════════════════════════════════════════
// CREATE PRODUCT
// ═══════════════════════════════════════════════════════════════════

public class CreateProductRequest
{
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public ProductRatingRequest Rating { get; set; } = new();
}

public class ProductRatingRequest
{
    public decimal Rate { get; set; }
    public int Count { get; set; }
}

public class CreateProductResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public ProductRatingResponse Rating { get; set; } = new();
}

public class ProductRatingResponse
{
    public decimal Rate { get; set; }
    public int Count { get; set; }
}

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(p => p.Title).NotEmpty().MaximumLength(200);
        RuleFor(p => p.Price).GreaterThan(0);
        RuleFor(p => p.Description).NotEmpty().MaximumLength(2000);
        RuleFor(p => p.Category).NotEmpty().MaximumLength(100);
        RuleFor(p => p.Rating.Rate).InclusiveBetween(0, 5);
        RuleFor(p => p.Rating.Count).GreaterThanOrEqualTo(0);
    }
}

public class CreateProductProfile : Profile
{
    public CreateProductProfile()
    {
        CreateMap<CreateProductRequest, CreateProductCommand>();
        CreateMap<ProductRatingRequest, CreateProductRatingCommand>();
        CreateMap<CreateProductResult, CreateProductResponse>();
        CreateMap<CreateProductRatingResult, ProductRatingResponse>();
    }
}

// ═══════════════════════════════════════════════════════════════════
// GET PRODUCT
// ═══════════════════════════════════════════════════════════════════

public class GetProductResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public ProductRatingResponse Rating { get; set; } = new();
}

public class GetProductProfile : Profile
{
    public GetProductProfile()
    {
        CreateMap<GetProductResult, GetProductResponse>();
        CreateMap<GetProductRatingResult, ProductRatingResponse>();
    }
}

// ═══════════════════════════════════════════════════════════════════
// GET PRODUCTS (list)
// ═══════════════════════════════════════════════════════════════════

public class GetProductsResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public ProductRatingResponse Rating { get; set; } = new();
}

public class GetProductsProfile : Profile
{
    public GetProductsProfile()
    {
        CreateMap<ProductListResult, GetProductsResponse>();
        CreateMap<ProductListRatingResult, ProductRatingResponse>();
    }
}

// ═══════════════════════════════════════════════════════════════════
// UPDATE PRODUCT
// ═══════════════════════════════════════════════════════════════════

public class UpdateProductRequest
{
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public ProductRatingRequest Rating { get; set; } = new();
}

public class UpdateProductResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public ProductRatingResponse Rating { get; set; } = new();
}

public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(p => p.Title).NotEmpty().MaximumLength(200);
        RuleFor(p => p.Price).GreaterThan(0);
        RuleFor(p => p.Description).NotEmpty().MaximumLength(2000);
        RuleFor(p => p.Category).NotEmpty().MaximumLength(100);
        RuleFor(p => p.Rating.Rate).InclusiveBetween(0, 5);
        RuleFor(p => p.Rating.Count).GreaterThanOrEqualTo(0);
    }
}

public class UpdateProductProfile : Profile
{
    public UpdateProductProfile()
    {
        CreateMap<UpdateProductRequest, UpdateProductCommand>();
        CreateMap<ProductRatingRequest, UpdateProductRatingCommand>();
        CreateMap<UpdateProductResult, UpdateProductResponse>();
        CreateMap<UpdateProductRatingResult, ProductRatingResponse>();
    }
}
