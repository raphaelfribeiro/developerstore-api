using AutoMapper;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Application.Auth.AuthenticateUser;

/// <summary>
/// AutoMapper profile for authentication-related mappings in Application layer
/// </summary>
public sealed class AuthenticateUserApplicationProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticateUserApplicationProfile"/> class
    /// </summary>
    public AuthenticateUserApplicationProfile()
    {
        CreateMap<User, AuthenticateUserResult>()
            .ForMember(dest => dest.Token, opt => opt.Ignore())
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));
    }
}
