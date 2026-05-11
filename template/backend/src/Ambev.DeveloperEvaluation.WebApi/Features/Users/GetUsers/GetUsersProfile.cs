using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Users.GetUsers;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.Shared;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Users.GetUsers;

public class GetUsersProfile : Profile
{
    public GetUsersProfile()
    {
        CreateMap<GetUsersResult, GetUsersResponse>()
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
