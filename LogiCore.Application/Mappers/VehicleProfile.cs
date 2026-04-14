using AutoMapper;
using LogiCore.Application.DTOs;
using LogiCore.Domain.Entities;

namespace LogiCore.Application.Mappers;

public class VehicleProfile : Profile
{
    public VehicleProfile()
    {
        CreateMap<Vehicle, VehicleDto>()
            .ForMember(dest => dest.LicensePlate, opt => opt.MapFrom(src => src.Plate));
        CreateMap<CreateVehicleDto, Vehicle>()
            .ConstructUsing(src => Vehicle.Create(src.Plate, src.MaxWeightCapacity, src.MaxVolumeCapacity, src.Make, src.Model));
        CreateMap<UpdateVehicleDto, Vehicle>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());
    }
}
