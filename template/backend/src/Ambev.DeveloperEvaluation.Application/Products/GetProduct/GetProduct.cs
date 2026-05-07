using AutoMapper;
using FluentValidation;
using MediatR;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Products.GetProduct;

public record GetProductQuery(Guid Id) : IRequest<GetProductResult>;

public class GetProductValidator : AbstractValidator<GetProductQuery>
{
    public GetProductValidator()
    {
        RuleFor(q => q.Id).NotEmpty().WithMessage("Product ID is required.");
    }
}

public class GetProductResult
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public GetProductRatingResult Rating { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class GetProductRatingResult
{
    public decimal Rate { get; set; }
    public int Count { get; set; }
}

public class GetProductProfile : Profile
{
    public GetProductProfile()
    {
        CreateMap<Domain.Entities.Product, GetProductResult>();
        CreateMap<Domain.Entities.ProductRating, GetProductRatingResult>();
    }
}

public class GetProductHandler : IRequestHandler<GetProductQuery, GetProductResult>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public GetProductHandler(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<GetProductResult> Handle(GetProductQuery query, CancellationToken cancellationToken)
    {
        var validation = await new GetProductValidator().ValidateAsync(query, cancellationToken);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var product = await _productRepository.GetByIdAsync(query.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Product with ID {query.Id} not found.");

        return _mapper.Map<GetProductResult>(product);
    }
}
