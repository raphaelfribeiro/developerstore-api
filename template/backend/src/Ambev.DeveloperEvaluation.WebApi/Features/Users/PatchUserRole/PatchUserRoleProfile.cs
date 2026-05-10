using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Users.PatchUserRole;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Users.PatchUserRole;

public class PatchUserRoleProfile : Profile
{
    public PatchUserRoleProfile()
    {
        CreateMap<PatchUserRoleRequest, PatchUserRoleCommand>();

        CreateMap<PatchUserRoleResult, PatchUserRoleResponse>()
            .ForMember(dest => dest.Role,   opt => opt.MapFrom(src => src.Role.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
    }
}
