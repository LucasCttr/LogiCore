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
            .ForMember(dest => dest.Dimensions, opt => opt.MapFrom(src => src.Dimensions))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.InternalCode, opt => opt.MapFrom(src => src.InternalCode))
            .ForMember(dest => dest.OriginAddress, opt => opt.MapFrom(src => src.OriginAddress))
            .ForMember(dest => dest.DestinationAddress, opt => opt.MapFrom(src => src.DestinationAddress));

        CreateMap<Domain.ValueObjects.Recipient, RecipientDto>();
        CreateMap<Domain.ValueObjects.Dimensions, DimensionsDto>()
            .ForMember(dest => dest.VolumeCm3, opt => opt.MapFrom(src => src.VolumeCm3));
    }
}
