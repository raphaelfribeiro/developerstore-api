using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ambev.DeveloperEvaluation.Application.Common;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Carts.GetCarts;

public class GetCartsQuery : IRequest<PaginatedResult<GetCartsResult>>
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public string? Order { get; set; }
    public Guid? UserId { get; set; }
    public DateTime? MinDate { get; set; }
    public DateTime? MaxDate { get; set; }
}

public class GetCartsResult
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime Date { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ItemCount { get; set; }
}

public class GetCartsProfile : Profile
{
    public GetCartsProfile()
    {
        CreateMap<Domain.Entities.Cart, GetCartsResult>()
            .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.Items.Count));
    }
}

public class GetCartsHandler : IRequestHandler<GetCartsQuery, PaginatedResult<GetCartsResult>>
{
    private readonly ICartRepository _cartRepository;
    private readonly IMapper _mapper;

    public GetCartsHandler(ICartRepository cartRepository, IMapper mapper)
    {
        _cartRepository = cartRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<GetCartsResult>> Handle(GetCartsQuery query, CancellationToken cancellationToken)
    {
        var queryable = _cartRepository.GetAllQueryable();

        if (query.UserId.HasValue)
            queryable = queryable.Where(c => c.UserId == query.UserId.Value);

        if (query.MinDate.HasValue)
            queryable = queryable.Where(c => c.Date >= query.MinDate.Value);

        if (query.MaxDate.HasValue)
            queryable = queryable.Where(c => c.Date <= query.MaxDate.Value);

        queryable = query.Order?.Trim().ToLower() switch
        {
            "date asc"  => queryable.OrderBy(c => c.Date),
            "date desc" => queryable.OrderByDescending(c => c.Date),
            _ => queryable.OrderByDescending(c => c.Date)
        };

        var totalCount = await queryable.CountAsync(cancellationToken);
        var items = await queryable
            .Skip((query.Page - 1) * query.Size)
            .Take(query.Size)
            .ToListAsync(cancellationToken);

        var mapped = _mapper.Map<List<GetCartsResult>>(items);
        return new PaginatedResult<GetCartsResult>(mapped, totalCount, query.Page, query.Size);
    }
}
