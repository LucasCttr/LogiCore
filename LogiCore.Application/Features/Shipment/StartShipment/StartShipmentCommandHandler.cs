using MediatR;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.Common.Models;
using LogiCore.Domain.Common.Exceptions;
using LogiCore.Domain.Entities;

namespace LogiCore.Application.Features.Shipment.StartShipment;

/// <summary>
/// Starts (activates) a shipment dispatch for drivers
/// - Changes shipment status from Draft to Dispatched
/// - Driver can have only one active shipment at a time
/// - Packages are scanned later using the existing scanner
/// </summary>
public class StartShipmentCommandHandler : IRequestHandler<StartShipmentCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IShipmentRepository _shipmentRepository;

    public StartShipmentCommandHandler(
        IUnitOfWork unitOfWork,
        IShipmentRepository shipmentRepository)
    {
        _unitOfWork = unitOfWork;
        _shipmentRepository = shipmentRepository;
    }

    public async Task<Result<bool>> Handle(StartShipmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Console.WriteLine($"[StartShipment] Starting shipment {request.ShipmentId}");
            
            // 1. Validate shipment exists
            var shipment = await _shipmentRepository.GetByIdAsync(request.ShipmentId);
            if (shipment == null)
                return Result<bool>.Failure("Shipment not found.", ErrorType.NotFound);

            Console.WriteLine($"[StartShipment] Shipment found. Status: {shipment.Status}, Type: {shipment.Type}");
            Console.WriteLine($"[StartShipment] Shipment has {shipment.Packages.Count} packages");
            foreach (var pkg in shipment.Packages)
            {
                Console.WriteLine($"[StartShipment]   - Package {pkg.Id}: Status = {pkg.Status}");
            }

            // 2. Validate shipment is in Draft status
            if (shipment.Status != ShipmentStatus.Draft)
                return Result<bool>.Failure("Only Draft shipments can be started.", ErrorType.Conflict);

            // 3. Dispatch the shipment (state transition: Draft -> Dispatched)
            // This also updates packages to InTransit in memory (for non-Pickup shipments)
            Console.WriteLine($"[StartShipment] Calling DispatchShipment()");
            shipment.DispatchShipment();
            
            Console.WriteLine($"[StartShipment] After DispatchShipment - Shipment Status: {shipment.Status}");
            foreach (var pkg in shipment.Packages)
            {
                Console.WriteLine($"[StartShipment]   - Package {pkg.Id}: Status = {pkg.Status}");
            }

            // 4. Persist changes to shipment
            Console.WriteLine($"[StartShipment] Updating shipment in repository");
            await _shipmentRepository.UpdateAsync(shipment);

            // 5. Sync package status changes to database
            // This ensures that package status updates are persisted (especially InTransit)
            Console.WriteLine($"[StartShipment] Syncing packages in database");
            await _shipmentRepository.SyncPackagesInDatabaseAsync(shipment);

            Console.WriteLine($"[StartShipment] Committing transaction");
            await _unitOfWork.CommitAsync(cancellationToken);

            Console.WriteLine($"[StartShipment] Shipment started successfully");
            return Result<bool>.Success(true);
        }
        catch (DomainException ex)
        {
            Console.WriteLine($"[StartShipment] DomainException: {ex.Message}");
            return Result<bool>.Failure(ex.Message, ErrorType.Conflict);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[StartShipment] Exception: {ex.Message}\n{ex.StackTrace}");
            return Result<bool>.Failure($"An error occurred while starting shipment: {ex.Message}", ErrorType.None);
        }
    }
}
