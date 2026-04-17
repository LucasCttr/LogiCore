using MediatR;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.Common.Models;
using LogiCore.Domain.Common.Exceptions;

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
            // 1. Validate shipment exists
            var shipment = await _shipmentRepository.GetByIdAsync(request.ShipmentId);
            if (shipment == null)
                return Result<bool>.Failure("Shipment not found.", ErrorType.NotFound);

            // 2. Validate shipment is in Draft status
            if (shipment.Status != LogiCore.Domain.Entities.ShipmentStatus.Draft)
                return Result<bool>.Failure("Only Draft shipments can be started.", ErrorType.Conflict);

            // 3. Dispatch the shipment (state transition: Draft -> Dispatched)
            shipment.DispatchShipment();

            // 4. Persist changes
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
            return Result<bool>.Failure($"An error occurred while starting shipment: {ex.Message}", ErrorType.None);
        }
    }
}
