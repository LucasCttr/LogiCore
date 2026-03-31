using AutoMapper;
using LogiCore.Application.DTOs;
using LogiCore.Domain.Entities;

namespace LogiCore.Application.Mappers;

public class DriverProfile : Profile
{
    public DriverProfile()
    {
        CreateMap<Driver, DriverDto>()
            .ForMember(d => d.Phone, opt => opt.MapFrom(src => src.ApplicationUser != null ? src.ApplicationUser.PhoneNumber : null));
    }
}
