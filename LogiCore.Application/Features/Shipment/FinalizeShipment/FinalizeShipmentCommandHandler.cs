using MediatR;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.Common.Models;
using LogiCore.Domain.Common.Exceptions;
using Microsoft.Extensions.Logging;

namespace LogiCore.Application.Features.Shipment.FinalizeShipment;

/// <summary>
/// Handler for finalizing a shipment and updating its packages based on shipment type.
/// - Pickup: Shipment marked as Delivered, packages move to AtDepot at origin location
/// - Transfer: Shipment marked as Delivered, packages move to AtDepot at destination location
/// - LastMile: Shipment marked as Delivered, packages keep their delivery status
/// </summary>
public class FinalizeShipmentCommandHandler : IRequestHandler<FinalizeShipmentCommand, Result<bool>>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<FinalizeShipmentCommandHandler> _logger;

    public FinalizeShipmentCommandHandler(
        IShipmentRepository shipmentRepository,
        IUnitOfWork unitOfWork,
        ILogger<FinalizeShipmentCommandHandler> logger)
    {
        _shipmentRepository = shipmentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(FinalizeShipmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("=== FinalizeShipment START - ShipmentId: {ShipmentId} ===", request.ShipmentId);
            
            // 1. Load shipment with packages
            var shipment = await _shipmentRepository.GetByIdAsync(request.ShipmentId);
            if (shipment == null)
            {
                _logger.LogWarning("Shipment not found: {ShipmentId}", request.ShipmentId);
                return Result<bool>.Failure("Shipment not found.", ErrorType.NotFound);
            }

            _logger.LogInformation("Shipment loaded - Type: {Type}, Status: {Status}, PackageCount: {Count}, OriginLocationId: {Origin}, DestinationLocationId: {Dest}",
                shipment.Type, shipment.Status, shipment.Packages.Count, shipment.OriginLocationId, shipment.DestinationLocationId);

            // 2. Log package states BEFORE finalize
            foreach (var pkg in shipment.Packages)
            {
                _logger.LogInformation("  Package BEFORE: Id={Id}, Status={Status}, LocationId={LocationId}",
                    pkg.Id, pkg.Status, pkg.CurrentLocationId);
            }

            // 3. Finalize the shipment
            _logger.LogInformation("Calling FinalizeShipment()...");
            shipment.FinalizeShipment();

            // 4. Log package states AFTER finalize
            foreach (var pkg in shipment.Packages)
            {
                _logger.LogInformation("  Package AFTER: Id={Id}, Status={Status}, LocationId={LocationId}",
                    pkg.Id, pkg.Status, pkg.CurrentLocationId);
            }

            // 5. Update via repository
            _logger.LogInformation("Calling UpdateAsync()...");
            await _shipmentRepository.UpdateAsync(shipment);
            
            _logger.LogInformation("Calling CommitAsync()...");
            await _unitOfWork.CommitAsync(cancellationToken);

            _logger.LogInformation("=== FinalizeShipment SUCCESS ===");
            return Result<bool>.Success(true);
        }
        catch (DomainException ex)
        {
            _logger.LogError("Domain exception: {Message}", ex.Message);
            return Result<bool>.Failure(ex.Message, ErrorType.Conflict);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finalizing shipment: {Message}", ex.Message);
            return Result<bool>.Failure($"Error finalizing shipment: {ex.Message}", ErrorType.None);
        }
    }
}
