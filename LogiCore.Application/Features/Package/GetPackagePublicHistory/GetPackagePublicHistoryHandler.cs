using MediatR;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;
using LogiCore.Application.Common.Interfaces.Persistence;

namespace LogiCore.Application.Features.Package.GetPackagePublicHistory;

public class GetPackagePublicHistoryHandler : IRequestHandler<GetPackagePublicHistoryQuery, Result<PackagePublicHistoryDto?>>
{
    private readonly IPackageRepository _packageRepository;
    private readonly IShipmentRepository _shipmentRepository;

    public GetPackagePublicHistoryHandler(IPackageRepository packageRepository, IShipmentRepository shipmentRepository)
    {
        _packageRepository = packageRepository;
        _shipmentRepository = shipmentRepository;
    }

    public async Task<Result<PackagePublicHistoryDto?>> Handle(GetPackagePublicHistoryQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.TrackingNumber))
            return Result<PackagePublicHistoryDto?>.Failure("Tracking number is required.");

        var package = await _packageRepository.GetByTrackingNumberAsync(request.TrackingNumber);
        if (package == null)
            return Result<PackagePublicHistoryDto?>.Failure("Package not found.");

        var shipment = await _shipmentRepository.GetByPackageIdAsync(package.Id);
        // Retrieve package history and map to minimal public entries (status + timestamp)
        var histories = await _packageRepository.GetHistoryAsync(package.Id);
        var orderedEntries = histories
            .OrderBy(h => h.OccurredAt)
            .Select(h => new PublicHistoryEntryDto(h.ToStatus.ToString(), h.OccurredAt))
            .ToList();

        var dto = new PackagePublicHistoryDto(package.TrackingNumber, orderedEntries);
        return Result<PackagePublicHistoryDto?>.Success(dto);
    }
}
