using MediatR;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;

namespace LogiCore.Application.Features.Package.GetPackagePublicHistory;

public class GetPackagePublicHistoryQuery : IRequest<Result<PackagePublicHistoryDto?>>
{
    public string TrackingNumber { get; init; }
    public GetPackagePublicHistoryQuery(string trackingNumber) => TrackingNumber = trackingNumber;
}
