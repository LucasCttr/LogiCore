using AutoMapper;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;
using LogiCore.Domain.Entities;
using MediatR;

namespace LogiCore.Application.Features.Package.GetPackageForScanner;

public class GetPackageForScannerHandler : IRequestHandler<GetPackageForScannerQuery, Result<PackageForScannerDto>>
{
    private readonly IPackageRepository _repository;

    public GetPackageForScannerHandler(IPackageRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PackageForScannerDto>> Handle(GetPackageForScannerQuery request, CancellationToken cancellationToken)
    {
        var package = await _repository.GetByIdAsync(request.Id);
        if (package is null)
        {
            return Result<PackageForScannerDto>.Failure("Package not found.");
        }

        // Allow scanning packages in any status - actions will be determined on frontend based on status
        var dto = new PackageForScannerDto
        {
            Id = package.Id,
            TrackingNumber = package.TrackingNumber,
            Status = (int)package.Status,
            StatusLabel = package.Status.ToString(),
            Weight = package.Weight,
            OriginAddress = package.OriginAddress,
            DestinationAddress = package.DestinationAddress,
            RecipientName = package.Recipient?.Name,
        };

        return Result<PackageForScannerDto>.Success(dto);
    }
}
