using MediatR;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;
using LogiCore.Application.Common.Interfaces.Persistence;

namespace LogiCore.Application.Features.Package.GetPackageLocation;

public class GetPackageLocationByTrackingHandler : IRequestHandler<GetPackageLocationByTrackingQuery, Result<PackagePublicLocationDto?>>
{
    private readonly IPackageRepository _packageRepository;
    private readonly IShipmentRepository _shipmentRepository;

    public GetPackageLocationByTrackingHandler(IPackageRepository packageRepository, IShipmentRepository shipmentRepository)
    {
        _packageRepository = packageRepository;
        _shipmentRepository = shipmentRepository;
    }

    public async Task<Result<PackagePublicLocationDto?>> Handle(GetPackageLocationByTrackingQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.TrackingNumber))
            return Result<PackagePublicLocationDto?>.Failure("Tracking number is required.");

        var package = await _packageRepository.GetByTrackingNumberAsync(request.TrackingNumber);
        if (package == null)
            return Result<PackagePublicLocationDto?>.Failure("Package not found.");

        var shipment = await _shipmentRepository.GetByPackageIdAsync(package.Id);

        string status = package.Status.ToString();
        string locationDescription = "Unknown location";

        if (shipment == null)
        {
            locationDescription = "Not yet assigned to a shipment";
        }
        else
        {
            switch (shipment.Status)
            {
                case LogiCore.Domain.Entities.ShipmentStatus.Arrived:
                    locationDescription = $"At distribution center {shipment.RouteCode}";
                    break;
                case LogiCore.Domain.Entities.ShipmentStatus.Loading:
                    locationDescription = "At distribution center - loading";
                    break;
                case LogiCore.Domain.Entities.ShipmentStatus.Dispatched:
                    locationDescription = "In transit";
                    break;
                case LogiCore.Domain.Entities.ShipmentStatus.Draft:
                    locationDescription = "Not yet assigned to a shipment";
                    break;
                case LogiCore.Domain.Entities.ShipmentStatus.Canceled:
                    locationDescription = "Shipment canceled";
                    break;
                default:
                    locationDescription = shipment.Status.ToString();
                    break;
            }
        }

        var dto = new PackagePublicLocationDto(package.TrackingNumber, status, locationDescription);
        return Result<PackagePublicLocationDto?>.Success(dto);
    }
}
