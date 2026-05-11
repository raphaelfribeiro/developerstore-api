using Ambev.DeveloperEvaluation.Domain.Entities;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.Application.Users.GetUsers;

public class GetUsersProfile : Profile
{
    public GetUsersProfile()
    {
        CreateMap<User, GetUsersResult>();
    }
}
