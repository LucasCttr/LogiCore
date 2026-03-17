using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;
using MediatR;

namespace LogiCore.Application.Features.Packages;
public class GetPackageByIdQuery : IRequest<Result<PackageDto>>
{
    public Guid Id { get; }

    public GetPackageByIdQuery(Guid id)
    {
        Id = id;
    }
}