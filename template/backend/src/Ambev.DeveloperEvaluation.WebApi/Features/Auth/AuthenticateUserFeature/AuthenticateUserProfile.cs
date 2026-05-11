using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Auth.AuthenticateUser;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Auth.AuthenticateUserFeature;

/// <summary>
/// AutoMapper profile for authentication-related mappings in WebApi layer
/// </summary>
public sealed class AuthenticateUserWebApiProfile : Profile
{
    public AuthenticateUserWebApiProfile()
    {
        CreateMap<AuthenticateUserRequest, AuthenticateUserCommand>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username));

        CreateMap<AuthenticateUserResult, AuthenticateUserResponse>();
    }
}