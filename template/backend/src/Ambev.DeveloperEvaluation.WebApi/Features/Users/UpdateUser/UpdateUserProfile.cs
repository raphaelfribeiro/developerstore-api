using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Users.UpdateUser;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.Shared;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Users.UpdateUser;

public class UpdateUserProfile : Profile
{
    public UpdateUserProfile()
    {
        CreateMap<UpdateUserRequest, UpdateUserCommand>()
            .ForMember(dest => dest.FirstName,     opt => opt.MapFrom(src => src.Name.Firstname))
            .ForMember(dest => dest.LastName,      opt => opt.MapFrom(src => src.Name.Lastname))
            .ForMember(dest => dest.City,          opt => opt.MapFrom(src => src.Address.City))
            .ForMember(dest => dest.Street,        opt => opt.MapFrom(src => src.Address.Street))
            .ForMember(dest => dest.Number,        opt => opt.MapFrom(src => src.Address.Number))
            .ForMember(dest => dest.ZipCode,       opt => opt.MapFrom(src => src.Address.Zipcode))
            .ForMember(dest => dest.GeoLat,        opt => opt.MapFrom(src => src.Address.Geolocation.Lat))
            .ForMember(dest => dest.GeoLong,       opt => opt.MapFrom(src => src.Address.Geolocation.Long))
            .ForMember(dest => dest.CallerIsAdmin, opt => opt.Ignore());

        CreateMap<UpdateUserResult, UpdateUserResponse>()
            .ForMember(dest => dest.Status,  opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Role,    opt => opt.MapFrom(src => src.Role.ToString()))
            .ForMember(dest => dest.Password, opt => opt.Ignore())
            .ForMember(dest => dest.Name,    opt => opt.MapFrom(src => new UserNameDto
            {
                Firstname = src.FirstName,
                Lastname  = src.LastName
            }))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => new UserAddressDto
            {
                City    = src.City,
                Street  = src.Street,
                Number  = src.Number,
                Zipcode = src.ZipCode,
                Geolocation = new UserGeolocationDto { Lat = src.GeoLat, Long = src.GeoLong }
            }));
    }
}
