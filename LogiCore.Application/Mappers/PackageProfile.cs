using AutoMapper;
using LogiCore.Application.DTOs;
using LogiCore.Domain.Entities;

namespace LogiCore.Application.Mappers;

public class PackageProfile : Profile
{
    public PackageProfile()
    {
        CreateMap<Domain.Entities.Package, PackageDto>()
            .ForMember(dest => dest.Recipient, opt => opt.MapFrom(src => src.Recipient));
        CreateMap<Domain.ValueObjects.Recipient, RecipientDto>();
    }
}
