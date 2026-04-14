using AutoMapper;
using LogiCore.Application.DTOs;
using LogiCore.Domain.Entities;

namespace LogiCore.Application.Mappers;

public class DriverProfile : Profile
{
    public DriverProfile()
    {
        CreateMap<Driver, DriverDto>()
            .ForMember(d => d.Phone, opt => opt.MapFrom(src => src.ApplicationUser != null ? src.ApplicationUser.PhoneNumber : null))
            .ForMember(d => d.Email, opt => opt.MapFrom(src => src.ApplicationUser != null ? src.ApplicationUser.Email : null))
            .ForMember(d => d.AssignedVehicle, opt => opt.MapFrom(src => src.AssignedVehicle));

        CreateMap<Vehicle, AssignedVehicleInfoDto>()
            .ForMember(dest => dest.LicensePlate, opt => opt.MapFrom(src => src.Plate));
    }
}
