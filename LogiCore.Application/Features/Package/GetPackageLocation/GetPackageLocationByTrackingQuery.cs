using MediatR;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;

namespace LogiCore.Application.Features.Package.GetPackageLocation;

public class GetPackageLocationByTrackingQuery : IRequest<Result<PackagePublicLocationDto?>>
{
    public string TrackingNumber { get; init; }
    public GetPackageLocationByTrackingQuery(string trackingNumber) => TrackingNumber = trackingNumber;
}
