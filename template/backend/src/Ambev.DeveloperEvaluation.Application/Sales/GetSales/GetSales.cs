using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ambev.DeveloperEvaluation.Application.Common;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSales;

// ── Query ─────────────────────────────────────────────────────────────────────

/// <summary>
/// Query for retrieving a paginated, filterable and sortable list of sales.
/// </summary>
public class GetSalesQuery : IRequest<PaginatedResult<GetSalesResult>>
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public string? Order { get; set; }
    public string? CustomerName { get; set; }
    public bool? IsCancelled { get; set; }
    public DateTime? MinDate { get; set; }
    public DateTime? MaxDate { get; set; }
}

// ── Result ────────────────────────────────────────────────────────────────────

public class GetSalesResult
{
    public Guid Id { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public bool IsCancelled { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ItemCount { get; set; }
}

// ── Profile ───────────────────────────────────────────────────────────────────

public class GetSalesProfile : Profile
{
    public GetSalesProfile()
    {
        CreateMap<Domain.Entities.Sale, GetSalesResult>()
            .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.Items.Count));
    }
}

// ── Handler ───────────────────────────────────────────────────────────────────

public class GetSalesHandler : IRequestHandler<GetSalesQuery, PaginatedResult<GetSalesResult>>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;

    public GetSalesHandler(ISaleRepository saleRepository, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<GetSalesResult>> Handle(GetSalesQuery query, CancellationToken cancellationToken)
    {
        var queryable = _saleRepository.GetAllQueryable();

        // Filtering
        if (!string.IsNullOrWhiteSpace(query.CustomerName))
            queryable = queryable.Where(s => s.CustomerName.Contains(query.CustomerName));

        if (query.IsCancelled.HasValue)
            queryable = queryable.Where(s => s.IsCancelled == query.IsCancelled.Value);

        if (query.MinDate.HasValue)
            queryable = queryable.Where(s => s.SaleDate >= query.MinDate.Value);

        if (query.MaxDate.HasValue)
            queryable = queryable.Where(s => s.SaleDate <= query.MaxDate.Value);

        // Ordering
        queryable = ApplyOrdering(queryable, query.Order);

        // Pagination
        var totalCount = await queryable.CountAsync(cancellationToken);
        var items = await queryable
            .Skip((query.Page - 1) * query.Size)
            .Take(query.Size)
            .ToListAsync(cancellationToken);

        var mapped = _mapper.Map<List<GetSalesResult>>(items);
        return new PaginatedResult<GetSalesResult>(mapped, totalCount, query.Page, query.Size);
    }

    private static IQueryable<Domain.Entities.Sale> ApplyOrdering(
        IQueryable<Domain.Entities.Sale> queryable, string? order)
    {
        if (string.IsNullOrWhiteSpace(order))
            return queryable.OrderByDescending(s => s.SaleDate);

        return order.Trim().ToLower() switch
        {
            "saledate asc"    => queryable.OrderBy(s => s.SaleDate),
            "saledate desc"   => queryable.OrderByDescending(s => s.SaleDate),
            "totalamount asc" => queryable.OrderBy(s => s.TotalAmount),
            "totalamount desc"=> queryable.OrderByDescending(s => s.TotalAmount),
            "salenumber asc"  => queryable.OrderBy(s => s.SaleNumber),
            "salenumber desc" => queryable.OrderByDescending(s => s.SaleNumber),
            _ => queryable.OrderByDescending(s => s.SaleDate)
        };
    }
}
