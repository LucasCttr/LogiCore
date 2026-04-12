using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;
using MediatR;

namespace LogiCore.Application.Features.Package.GetPackageForScanner;

/// <summary>
/// Query to validate and retrieve package information for scanner mode using tracking number.
/// This is used when the barcode contains the tracking number (printed on shipping label).
/// </summary>
public class GetPackageForScannerByTrackingQuery : IRequest<Result<PackageForScannerDto>>
{
    public string TrackingNumber { get; }

    public GetPackageForScannerByTrackingQuery(string trackingNumber)
    {
        TrackingNumber = trackingNumber;
    }
}
