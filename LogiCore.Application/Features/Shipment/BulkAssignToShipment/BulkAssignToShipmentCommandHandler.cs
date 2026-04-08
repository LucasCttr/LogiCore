using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.Common.Models;
using MediatR;
using LogiCore.Domain.Common.Exceptions;

namespace LogiCore.Application.Features.Shipment;

public class BulkAssignToShipmentCommandHandler : IRequestHandler<BulkAssignToShipmentCommand, Result<bool>>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IPackageRepository _packageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BulkAssignToShipmentCommandHandler(IShipmentRepository shipmentRepository, IPackageRepository packageRepository, IUnitOfWork unitOfWork)
    {
        _shipmentRepository = shipmentRepository;
        _packageRepository = packageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(BulkAssignToShipmentCommand request, CancellationToken cancellationToken)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(request.ShipmentId);
        if (shipment == null) return Result<bool>.Failure("Shipment not found.", ErrorType.NotFound);

        var packages = (await _packageRepository.GetManyByIdsAsync(request.PackageIds)).ToList();
        if (!packages.Any()) return Result<bool>.Failure("No packages found for provided ids.");

        // Validate all packages are at depot
        if (packages.Any(p => p.Status != Domain.Entities.PackageStatus.AtDepot))
            return Result<bool>.Failure("All packages must be AtDepot to assign to shipment.");

        // Validate capacity: weight and volume
        var totalWeight = packages.Sum(p => p.Weight);
        var totalVolume = packages.Sum(p => p.Dimensions?.VolumeCm3 ?? 0m);

        var availableWeight = shipment.Vehicle?.MaxWeightCapacity ?? shipment.VehicleMaxWeightCapacity;
        var availableVolume = shipment.Vehicle?.MaxVolumeCapacity ?? shipment.VehicleMaxVolumeCapacity;

        var currentWeight = shipment.GetCurrentWeight();
        var currentVolume = shipment.Packages.Sum(p => p.Dimensions?.VolumeCm3 ?? 0m);

        if (currentWeight + totalWeight > availableWeight)
            return Result<bool>.Failure("Vehicle weight capacity exceeded.");

        if (currentVolume + totalVolume > availableVolume)
            return Result<bool>.Failure("Vehicle volume capacity exceeded.");

        try
        {
            foreach (var pkg in packages)
            {
                shipment.AddPackage(pkg);
                pkg.StartTransit();
            }

            await _shipmentRepository.UpdateAsync(shipment);
            await _packageRepository.UpdateRangeAsync(packages);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (DomainException ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }
}
