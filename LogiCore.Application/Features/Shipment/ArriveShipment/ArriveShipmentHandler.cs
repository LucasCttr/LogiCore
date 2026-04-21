using System.Threading;
using System.Threading.Tasks;
using MediatR;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.Common.Models;
using LogiCore.Domain.Common.Exceptions;

namespace LogiCore.Application.Features.Shipment.ArriveShipment;

/// <summary>
/// Handler para marcar un shipment como arrived (llegó al destino final).
/// Automáticamente sincroniza todos los paquetes a AtDepot vía el agregado Shipment.
/// </summary>
public class ArriveShipmentHandler : IRequestHandler<ArriveShipmentCommand, Result<bool>>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IPackageRepository _packageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ArriveShipmentHandler(IShipmentRepository shipmentRepository, IPackageRepository packageRepository, IUnitOfWork unitOfWork)
    {
        _shipmentRepository = shipmentRepository;
        _packageRepository = packageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(ArriveShipmentCommand request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[ArriveShipment] Starting arrival for shipment {request.ShipmentId}");
        
        var shipment = await _shipmentRepository.GetByIdAsync(request.ShipmentId);
        if (shipment == null) 
        {
            Console.WriteLine($"[ArriveShipment] Shipment not found: {request.ShipmentId}");
            return Result<bool>.Failure("Shipment not found.", ErrorType.NotFound);
        }

        Console.WriteLine($"[ArriveShipment] Shipment found. Status: {shipment.Status}, DestinationLocationId: {shipment.DestinationLocationId}");
        Console.WriteLine($"[ArriveShipment] Shipment has {shipment.Packages.Count} packages");
        foreach (var pkg in shipment.Packages)
        {
            Console.WriteLine($"[ArriveShipment]   - Package {pkg.Id}: Status = {pkg.Status}, CurrentLocationId = {pkg.CurrentLocationId}");
        }

        try
        {
            // MarkAsArrived() ya sincroniza todos los paquetes a AtDepot internamente
            Console.WriteLine($"[ArriveShipment] Calling MarkAsArrived()");
            shipment.MarkAsArrived();

            Console.WriteLine($"[ArriveShipment] After MarkAsArrived - Shipment Status: {shipment.Status}");
            foreach (var pkg in shipment.Packages)
            {
                Console.WriteLine($"[ArriveShipment]   - Package {pkg.Id}: Status = {pkg.Status}");
            }

            // Persist shipment y sus cambios en paquetes
            Console.WriteLine($"[ArriveShipment] Updating shipment in repository");
            await _shipmentRepository.UpdateAsync(shipment);
            
            // Sync package status changes to database
            Console.WriteLine($"[ArriveShipment] Syncing packages to AtDepot in database");
            await _shipmentRepository.SyncPackagesInDatabaseAsync(shipment);
            
            // Update package CurrentLocationId to DestinationLocationId (for depot-to-depot shipments)
            if (shipment.DestinationLocationId.HasValue && shipment.Packages.Any())
            {
                Console.WriteLine($"[ArriveShipment] Updating package CurrentLocationId to DestinationLocationId: {shipment.DestinationLocationId}");
                foreach (var package in shipment.Packages)
                {
                    Console.WriteLine($"[ArriveShipment]   - Package {package.Id}: CurrentLocationId = {shipment.DestinationLocationId}");
                }
                
                var packageIds = shipment.Packages.Select(p => p.Id).ToList();
                await _packageRepository.UpdateCurrentLocationBulkAsync(packageIds, shipment.DestinationLocationId.Value);
            }
            
            Console.WriteLine($"[ArriveShipment] Committing transaction");
            await _unitOfWork.CommitAsync(cancellationToken);

            Console.WriteLine($"[ArriveShipment] Shipment arrival marked successfully");
            return Result<bool>.Success(true);
        }
        catch (DomainException ex)
        {
            Console.WriteLine($"[ArriveShipment] DomainException: {ex.Message}");
            return Result<bool>.Failure(ex.Message, ErrorType.Conflict);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ArriveShipment] Exception: {ex.Message}\n{ex.StackTrace}");
            throw;
        }
    }
}
