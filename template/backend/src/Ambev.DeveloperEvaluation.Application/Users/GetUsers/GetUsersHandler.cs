using Ambev.DeveloperEvaluation.Application.Common;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.Application.Users.GetUsers;

public class GetUsersHandler : IRequestHandler<GetUsersQuery, PaginatedResult<GetUsersResult>>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetUsersHandler(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<GetUsersResult>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _userRepository.GetAllQueryable();

        if (!string.IsNullOrWhiteSpace(request.Order))
        {
            var parts = request.Order.Trim().Split(' ');
            var field = parts[0];
            var desc = parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

            query = (field.ToLower(), desc) switch
            {
                ("username", false) => query.OrderBy(u => u.Username),
                ("username", true)  => query.OrderByDescending(u => u.Username),
                ("email", false)    => query.OrderBy(u => u.Email),
                ("email", true)     => query.OrderByDescending(u => u.Email),
                _                   => query.OrderBy(u => u.Username)
            };
        }
        else
        {
            query = query.OrderBy(u => u.Username);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var data = await query
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .ToListAsync(cancellationToken);

        var mapped = _mapper.Map<List<GetUsersResult>>(data);
        return new PaginatedResult<GetUsersResult>(mapped, totalCount, request.Page, request.Size);
    }
}
