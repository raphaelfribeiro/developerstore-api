using AutoMapper;
using FluentValidation;
using MediatR;
using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Products.CreateProduct;

// ── Command ───────────────────────────────────────────────────────────────────

public class CreateProductCommand : IRequest<CreateProductResult>
{
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public CreateProductRatingCommand Rating { get; set; } = new();

    public ValidationResultDetail Validate()
    {
        var validator = new CreateProductValidator();
        var result = validator.Validate(this);
        return new ValidationResultDetail
        {
            IsValid = result.IsValid,
            Errors = result.Errors.Select(o => (ValidationErrorDetail)o)
        };
    }
}

public class CreateProductRatingCommand
{
    public decimal Rate { get; set; }
    public int Count { get; set; }
}

// ── Validator ─────────────────────────────────────────────────────────────────

public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(p => p.Title).NotEmpty().MaximumLength(200);
        RuleFor(p => p.Price).GreaterThan(0);
        RuleFor(p => p.Description).NotEmpty().MaximumLength(2000);
        RuleFor(p => p.Category).NotEmpty().MaximumLength(100);
        RuleFor(p => p.Rating.Rate).InclusiveBetween(0, 5);
        RuleFor(p => p.Rating.Count).GreaterThanOrEqualTo(0);
    }
}

// ── Result ────────────────────────────────────────────────────────────────────

public class CreateProductResult
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public CreateProductRatingResult Rating { get; set; } = new();
}

public class CreateProductRatingResult
{
    public decimal Rate { get; set; }
    public int Count { get; set; }
}

// ── Profile ───────────────────────────────────────────────────────────────────

public class CreateProductProfile : Profile
{
    public CreateProductProfile()
    {
        CreateMap<CreateProductCommand, Product>();
        CreateMap<CreateProductRatingCommand, ProductRating>();
        CreateMap<Product, CreateProductResult>();
        CreateMap<ProductRating, CreateProductRatingResult>();
    }
}

// ── Handler ───────────────────────────────────────────────────────────────────

public class CreateProductHandler : IRequestHandler<CreateProductCommand, CreateProductResult>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public CreateProductHandler(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<CreateProductResult> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var validation = await new CreateProductValidator().ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var product = _mapper.Map<Product>(command);
        var created = await _productRepository.CreateAsync(product, cancellationToken);
        return _mapper.Map<CreateProductResult>(created);
    }
}
