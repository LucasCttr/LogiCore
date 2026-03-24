using AutoMapper;
using LogiCore.Application.DTOs;
using LogiCore.Domain.Entities;

namespace LogiCore.Application.Mappers;

public class PackageProfile : Profile
{
    public PackageProfile()
    {
        CreateMap<Domain.Entities.Package, PackageDto>()
            .ForMember(dest => dest.Recipient, opt => opt.MapFrom(src => src.Recipient))
            .ForMember(dest => dest.Dimensions, opt => opt.MapFrom(src => src.Dimensions));

        CreateMap<Domain.ValueObjects.Recipient, RecipientDto>();
        CreateMap<Domain.ValueObjects.Dimensions, DimensionsDto>()
            .ForMember(dest => dest.VolumeCm3, opt => opt.MapFrom(src => src.VolumeCm3));
    }
}
