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
    private readonly IPackageRepository _packageRepository;

    public StartShipmentCommandHandler(
        IUnitOfWork unitOfWork,
        IShipmentRepository shipmentRepository,
        IPackageRepository packageRepository)
    {
        _unitOfWork = unitOfWork;
        _shipmentRepository = shipmentRepository;
        _packageRepository = packageRepository;
    }

    public async Task<Result<bool>> Handle(StartShipmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Console.WriteLine($"\n\n========== [StartShipment] STARTING ==========");
            Console.WriteLine($"[StartShipment] Starting shipment {request.ShipmentId}");
            
            // 1. Validate shipment exists
            var shipment = await _shipmentRepository.GetByIdAsync(request.ShipmentId);
            if (shipment == null)
            {
                Console.WriteLine($"[StartShipment] ERROR: Shipment not found");
                return Result<bool>.Failure("Shipment not found.", ErrorType.NotFound);
            }

            Console.WriteLine($"[StartShipment] ✓ Shipment found");
            Console.WriteLine($"[StartShipment] Status: {shipment.Status}");
            Console.WriteLine($"[StartShipment] Type: {shipment.Type}");
            Console.WriteLine($"[StartShipment] OriginLocationId: {shipment.OriginLocationId}");
            Console.WriteLine($"[StartShipment] DestinationLocationId: {shipment.DestinationLocationId}");
            Console.WriteLine($"[StartShipment] Packages count: {shipment.Packages.Count}");
            
            if (shipment.Packages.Count == 0)
            {
                Console.WriteLine($"[StartShipment] ❌ ERROR: Shipment has NO packages!");
                return Result<bool>.Failure("Cannot dispatch empty shipment.", ErrorType.Conflict);
            }

            foreach (var pkg in shipment.Packages)
            {
                Console.WriteLine($"[StartShipment]   - Package {pkg.Id}: Status = {pkg.Status}, Location = {pkg.CurrentLocationId}");
            }

            // 2. Validate shipment is in Draft status
            if (shipment.Status != ShipmentStatus.Draft)
            {
                Console.WriteLine($"[StartShipment] ❌ ERROR: Shipment is {shipment.Status}, not Draft");
                return Result<bool>.Failure("Only Draft shipments can be started.", ErrorType.Conflict);
            }

            // 3. Dispatch the shipment (state transition: Draft -> Dispatched)
            // This also updates packages to InTransit in memory (for non-Pickup shipments)
            Console.WriteLine($"[StartShipment] Calling DispatchShipment()...");
            shipment.DispatchShipment();
            Console.WriteLine($"[StartShipment] ✓ DispatchShipment() completed");
            
            Console.WriteLine($"[StartShipment] After DispatchShipment - Shipment Status: {shipment.Status}");
            foreach (var pkg in shipment.Packages)
            {
                Console.WriteLine($"[StartShipment]   - Package {pkg.Id}: Status = {pkg.Status}");
            }

            // 4. Persist changes to shipment
            Console.WriteLine($"[StartShipment] Updating shipment in repository");
            await _shipmentRepository.UpdateAsync(shipment);
            Console.WriteLine($"[StartShipment] ✓ Shipment updated");

            // 5. Sync package status changes to database
            // This ensures that package status updates are persisted (especially InTransit)
            Console.WriteLine($"[StartShipment] Syncing packages in database");
            await _shipmentRepository.SyncPackagesInDatabaseAsync(shipment);
            Console.WriteLine($"[StartShipment] ✓ Packages synced");

            // 6. Update package CurrentLocationId to OriginLocationId (for depot-to-depot shipments)
            // This tracks which depot the package is leaving from
            if (shipment.OriginLocationId.HasValue && shipment.Packages.Any())
            {
                Console.WriteLine($"[StartShipment] Updating package CurrentLocationId to OriginLocationId: {shipment.OriginLocationId}");
                foreach (var package in shipment.Packages)
                {
                    Console.WriteLine($"[StartShipment]   - Package {package.Id}: CurrentLocationId = {shipment.OriginLocationId}");
                }
                
                var packageIds = shipment.Packages.Select(p => p.Id).ToList();
                await _packageRepository.UpdateCurrentLocationBulkAsync(packageIds, shipment.OriginLocationId.Value);
                Console.WriteLine($"[StartShipment] ✓ Package locations updated");
            }
            else
            {
                Console.WriteLine($"[StartShipment] No OriginLocationId, skipping location update");
            }

            Console.WriteLine($"[StartShipment] Committing transaction");
            await _unitOfWork.CommitAsync(cancellationToken);

            Console.WriteLine($"[StartShipment] ✓✓✓ Shipment started successfully ✓✓✓");
            Console.WriteLine($"========== [StartShipment] COMPLETED ==========\n");
            return Result<bool>.Success(true);
        }
        catch (DomainException ex)
        {
            Console.WriteLine($"[StartShipment] ❌ DomainException: {ex.Message}");
            Console.WriteLine($"========== [StartShipment] FAILED ==========\n");
            return Result<bool>.Failure(ex.Message, ErrorType.Conflict);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[StartShipment] ❌ Exception: {ex.Message}");
            Console.WriteLine($"[StartShipment] Stack: {ex.StackTrace}");
            Console.WriteLine($"========== [StartShipment] FAILED ==========\n");
            return Result<bool>.Failure($"An error occurred while starting shipment: {ex.Message}", ErrorType.None);
        }
    }
}
