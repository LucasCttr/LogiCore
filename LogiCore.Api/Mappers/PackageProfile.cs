using AutoMapper;
using LogiCore.Api.Models.DTOs;
using LogiCore.Application.Commands;

namespace LogiCore.Api.Mappers;

public class PackageProfile : Profile
{
    public PackageProfile()
    {
        CreateMap<CreatePackageRequest, CreatePackageCommand>();
    }
}