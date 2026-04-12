using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;
using MediatR;

namespace LogiCore.Application.Features.Package.GetPackageForScanner;

/// <summary>
/// Query to validate and retrieve minimal package information for scanner mode (depot ingress).
/// Used by frontend to validate packages during barcode/QR scanning.
/// </summary>
public class GetPackageForScannerQuery : IRequest<Result<PackageForScannerDto>>
{
    public Guid Id { get; }

    public GetPackageForScannerQuery(Guid id)
    {
        Id = id;
    }
}
