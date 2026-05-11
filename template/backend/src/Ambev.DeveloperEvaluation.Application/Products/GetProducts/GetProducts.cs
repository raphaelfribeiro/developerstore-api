using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ambev.DeveloperEvaluation.Application.Common;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Products.GetProducts;

// ─────────────────────────────────────────────────────────────────────────────
// Shared result / profile for all product list queries
// ─────────────────────────────────────────────────────────────────────────────

public class ProductListResult
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public ProductListRatingResult Rating { get; set; } = new();
}

public class ProductListRatingResult
{
    public decimal Rate { get; set; }
    public int Count { get; set; }
}

public class ProductListProfile : Profile
{
    public ProductListProfile()
    {
        CreateMap<Domain.Entities.Product, ProductListResult>();
        CreateMap<Domain.Entities.ProductRating, ProductListRatingResult>();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// GET /products — paginated, filterable, sortable
// ─────────────────────────────────────────────────────────────────────────────

public class GetProductsQuery : IRequest<PaginatedResult<ProductListResult>>
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public string? Order { get; set; }
    public string? Title { get; set; }
    public string? Category { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}

public class GetProductsHandler : IRequestHandler<GetProductsQuery, PaginatedResult<ProductListResult>>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public GetProductsHandler(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<ProductListResult>> Handle(GetProductsQuery query, CancellationToken cancellationToken)
    {
        var queryable = _productRepository.GetAllQueryable();

        if (!string.IsNullOrWhiteSpace(query.Title))
            queryable = query.Title.EndsWith("*") || query.Title.StartsWith("*")
                ? queryable.Where(p => p.Title.Contains(query.Title.Trim('*')))
                : queryable.Where(p => p.Title == query.Title);

        if (!string.IsNullOrWhiteSpace(query.Category))
            queryable = queryable.Where(p => p.Category.ToLower() == query.Category.ToLower());

        if (query.MinPrice.HasValue)
            queryable = queryable.Where(p => p.Price >= query.MinPrice.Value);

        if (query.MaxPrice.HasValue)
            queryable = queryable.Where(p => p.Price <= query.MaxPrice.Value);

        queryable = ApplyOrdering(queryable, query.Order);

        var totalCount = await queryable.CountAsync(cancellationToken);
        var items = await queryable
            .Skip((query.Page - 1) * query.Size)
            .Take(query.Size)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<ProductListResult>(
            _mapper.Map<List<ProductListResult>>(items), totalCount, query.Page, query.Size);
    }

    private static IQueryable<Domain.Entities.Product> ApplyOrdering(
        IQueryable<Domain.Entities.Product> q, string? order) => order?.Trim().ToLower() switch
    {
        "price asc"   => q.OrderBy(p => p.Price),
        "price desc"  => q.OrderByDescending(p => p.Price),
        "title asc"   => q.OrderBy(p => p.Title),
        "title desc"  => q.OrderByDescending(p => p.Title),
        _ => q.OrderBy(p => p.Title)
    };
}

// ─────────────────────────────────────────────────────────────────────────────
// GET /products/categories
// ─────────────────────────────────────────────────────────────────────────────

public record GetCategoriesQuery : IRequest<IEnumerable<string>>;

public class GetCategoriesHandler : IRequestHandler<GetCategoriesQuery, IEnumerable<string>>
{
    private readonly IProductRepository _productRepository;

    public GetCategoriesHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IEnumerable<string>> Handle(GetCategoriesQuery query, CancellationToken cancellationToken)
        => await _productRepository.GetCategoriesAsync(cancellationToken);
}

// ─────────────────────────────────────────────────────────────────────────────
// GET /products/category/{category} — paginated
// ─────────────────────────────────────────────────────────────────────────────

public class GetProductsByCategoryQuery : IRequest<PaginatedResult<ProductListResult>>
{
    public string Category { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public string? Order { get; set; }
}

public class GetProductsByCategoryHandler : IRequestHandler<GetProductsByCategoryQuery, PaginatedResult<ProductListResult>>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public GetProductsByCategoryHandler(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<ProductListResult>> Handle(GetProductsByCategoryQuery query, CancellationToken cancellationToken)
    {
        var queryable = _productRepository.GetByCategoryQueryable(query.Category);

        var totalCount = await queryable.CountAsync(cancellationToken);
        var items = await queryable
            .OrderBy(p => p.Title)
            .Skip((query.Page - 1) * query.Size)
            .Take(query.Size)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<ProductListResult>(
            _mapper.Map<List<ProductListResult>>(items), totalCount, query.Page, query.Size);
    }
}
