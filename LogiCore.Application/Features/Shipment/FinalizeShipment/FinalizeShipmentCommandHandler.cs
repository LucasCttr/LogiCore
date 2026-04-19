using MediatR;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.Common.Models;
using LogiCore.Domain.Common.Exceptions;

namespace LogiCore.Application.Features.Shipment.FinalizeShipment;

/// <summary>
/// Handler for finalizing a shipment and updating its packages based on shipment type.
/// - Pickup: Shipment marked as Delivered, packages keep their collection status (Collected/Pending)
/// - Transfer: Shipment marked as Delivered, packages move to AtDepot at destination location
/// - LastMile: Shipment marked as Delivered, packages keep their delivery status
/// </summary>
public class FinalizeShipmentCommandHandler : IRequestHandler<FinalizeShipmentCommand, Result<bool>>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public FinalizeShipmentCommandHandler(
        IShipmentRepository shipmentRepository,
        IUnitOfWork unitOfWork)
    {
        _shipmentRepository = shipmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(FinalizeShipmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 1. Validate shipment exists
            var shipment = await _shipmentRepository.GetByIdAsync(request.ShipmentId);
            if (shipment == null)
                return Result<bool>.Failure("Shipment not found.", ErrorType.NotFound);

            // 2. Finalize the shipment (handles type-specific logic)
            shipment.FinalizeShipment();

            // 3. Persist changes
            await _shipmentRepository.UpdateAsync(shipment);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (DomainException ex)
        {
            return Result<bool>.Failure(ex.Message, ErrorType.Conflict);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error finalizing shipment: {ex.Message}", ErrorType.None);
        }
    }
}
