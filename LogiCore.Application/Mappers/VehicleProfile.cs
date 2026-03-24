using AutoMapper;
using LogiCore.Application.DTOs;
using LogiCore.Domain.Entities;

namespace LogiCore.Application.Mappers;

public class VehicleProfile : Profile
{
    public VehicleProfile()
    {
        CreateMap<Vehicle, VehicleDto>();
        CreateMap<CreateVehicleDto, Vehicle>()
            .ConstructUsing(src => Vehicle.Create(src.Plate, src.MaxWeightCapacity, src.MaxVolumeCapacity));
        CreateMap<UpdateVehicleDto, Vehicle>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());
    }
}
