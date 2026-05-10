using Ambev.DeveloperEvaluation.Domain.Entities;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.Application.Users.PatchUserRole;

public class PatchUserRoleProfile : Profile
{
    public PatchUserRoleProfile()
    {
        CreateMap<User, PatchUserRoleResult>();
    }
}
