using AutoMapper;
using LogiCore.Application.DTOs;
using LogiCore.Domain.Entities;

namespace LogiCore.Application.Mappers;

public class PackageProfile : Profile
{
    public PackageProfile()
    {
        CreateMap<Package, PackageDto>();
    }
}
