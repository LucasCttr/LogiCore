using System.Threading;
using System.Threading.Tasks;
using MediatR;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.Common.Models;

namespace LogiCore.Application.Features.Shipment.CompleteShipment;

public class CompleteShipmentHandler : IRequestHandler<CompleteShipmentCommand, Result<bool>>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IPackageRepository _packageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteShipmentHandler(IShipmentRepository shipmentRepository, IPackageRepository packageRepository, IUnitOfWork unitOfWork)
    {
        _shipmentRepository = shipmentRepository;
        _packageRepository = packageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(CompleteShipmentCommand request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[CompleteShipment] Starting completion for shipment {request.ShipmentId}");
        
        var shipment = await _shipmentRepository.GetByIdAsync(request.ShipmentId);
        if (shipment == null)
        {
            Console.WriteLine($"[CompleteShipment] Shipment not found: {request.ShipmentId}");
            return Result<bool>.Failure("Shipment not found.", ErrorType.NotFound);
        }

        Console.WriteLine($"[CompleteShipment] Shipment found. Status: {shipment.Status}");
        Console.WriteLine($"[CompleteShipment] Shipment has {shipment.Packages.Count} packages");
        foreach (var pkg in shipment.Packages)
        {
            Console.WriteLine($"[CompleteShipment]   - Package {pkg.Id}: Status = {pkg.Status}, CurrentLocationId = {pkg.CurrentLocationId}");
        }

        try
        {
            Console.WriteLine($"[CompleteShipment] Calling MarkAsArrived() - finalizing shipment");
            shipment.MarkAsArrived();
            
            Console.WriteLine($"[CompleteShipment] After MarkAsArrived - Shipment Status: {shipment.Status}");
            foreach (var pkg in shipment.Packages)
            {
                Console.WriteLine($"[CompleteShipment]   - Package {pkg.Id}: Status = {pkg.Status}");
            }

            Console.WriteLine($"[CompleteShipment] Updating shipment in repository");
            await _shipmentRepository.UpdateAsync(shipment);
            
            // Sync package status changes to database
            Console.WriteLine($"[CompleteShipment] Syncing packages in database");
            await _shipmentRepository.SyncPackagesInDatabaseAsync(shipment);
            
            // Update package CurrentLocationId to DestinationLocationId if applicable
            if (shipment.DestinationLocationId.HasValue && shipment.Packages.Any())
            {
                Console.WriteLine($"[CompleteShipment] Updating package CurrentLocationId to DestinationLocationId: {shipment.DestinationLocationId}");
                foreach (var package in shipment.Packages)
                {
                    Console.WriteLine($"[CompleteShipment]   - Package {package.Id}: CurrentLocationId = {shipment.DestinationLocationId}");
                }
                
                var packageIds = shipment.Packages.Select(p => p.Id).ToList();
                await _packageRepository.UpdateCurrentLocationBulkAsync(packageIds, shipment.DestinationLocationId.Value);
            }
            
            Console.WriteLine($"[CompleteShipment] Committing transaction");
            await _unitOfWork.CommitAsync(cancellationToken);
            
            Console.WriteLine($"[CompleteShipment] Shipment completed successfully");
            return Result<bool>.Success(true);
        }
        catch (LogiCore.Domain.Common.Exceptions.DomainException ex)
        {
            Console.WriteLine($"[CompleteShipment] DomainException: {ex.Message}");
            return Result<bool>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CompleteShipment] Exception: {ex.Message}\n{ex.StackTrace}");
            throw;
        }
    }
}
