using AutoMapper;
using LogiCore.Api.Models.DTOs;
using LogiCore.Application.Features.Packages;

namespace LogiCore.Api.Mappers;

public class PackageProfile : Profile
{
    public PackageProfile()
    {
        CreateMap<CreatePackageRequest, CreatePackageCommand>();
        CreateMap<UpdatePackageRequest, UpdatePackageCommand>();
    }
}