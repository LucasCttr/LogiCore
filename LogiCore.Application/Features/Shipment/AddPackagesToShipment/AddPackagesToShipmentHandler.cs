using MediatR;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.Common.Models;

namespace LogiCore.Application.Features.Shipment.AddPackagesToShipment;

public class AddPackagesToShipmentHandler : IRequestHandler<AddPackagesToShipmentCommand, Result<bool>>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IPackageRepository _packageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddPackagesToShipmentHandler(
        IShipmentRepository shipmentRepository,
        IPackageRepository packageRepository,
        IUnitOfWork unitOfWork)
    {
        _shipmentRepository = shipmentRepository;
        _packageRepository = packageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(AddPackagesToShipmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 1. Validate command
            if (request.ShipmentId == Guid.Empty)
                return Result<bool>.Failure("Invalid shipment ID.", ErrorType.Validation);

            if (request.PackageIds == null || !request.PackageIds.Any())
                return Result<bool>.Failure("At least one package ID is required.", ErrorType.Validation);

            // 2. Get shipment
            var shipment = await _shipmentRepository.GetByIdAsync(request.ShipmentId);
            if (shipment == null)
                return Result<bool>.Failure("Shipment not found.", ErrorType.NotFound);

            // 3. Get packages
            var packages = (await _packageRepository.GetManyByIdsAsync(request.PackageIds)).ToList();
            if (!packages.Any())
                return Result<bool>.Failure("No packages found with the provided IDs.", ErrorType.NotFound);

            if (packages.Count != request.PackageIds.Count)
                return Result<bool>.Failure("Some package IDs were not found.", ErrorType.NotFound);

            // 4. Add packages to shipment (domain logic validates status, capacity, etc.)
            shipment.AddPackages(packages);

            // 5. Persist changes
            await _shipmentRepository.UpdateAsync(shipment);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (LogiCore.Domain.Common.Exceptions.DomainException ex)
        {
            return Result<bool>.Failure(ex.Message, ErrorType.Conflict);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"An error occurred while adding packages to shipment: {ex.Message}", ErrorType.None);
        }
    }
}
