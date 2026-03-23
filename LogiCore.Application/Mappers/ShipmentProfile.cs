using AutoMapper;
using LogiCore.Application.DTOs;
using LogiCore.Domain.Entities;
using System.Linq;

namespace LogiCore.Application.Mappers;

public class ShipmentProfile : Profile
{
    public ShipmentProfile()
    {
        CreateMap<Shipment, ShipmentDto>()
            .ForMember(dest => dest.PackageIds, opt => opt.MapFrom(src => src.Packages.Select(p => p.Id)));
    }
}
