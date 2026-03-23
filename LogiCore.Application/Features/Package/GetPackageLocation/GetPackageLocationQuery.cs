using MediatR;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;

namespace LogiCore.Application.Features.Package.GetPackageLocation;

public class GetPackageLocationQuery : IRequest<Result<ShipmentDto?>>
{
    public Guid PackageId { get; init; }
    public GetPackageLocationQuery(Guid packageId) => PackageId = packageId;
}
