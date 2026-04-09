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
    private readonly IUnitOfWork _unitOfWork;

    public ArriveShipmentHandler(IShipmentRepository shipmentRepository, IUnitOfWork unitOfWork)
    {
        _shipmentRepository = shipmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(ArriveShipmentCommand request, CancellationToken cancellationToken)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(request.ShipmentId);
        if (shipment == null) 
            return Result<bool>.Failure("Shipment not found.", ErrorType.NotFound);

        try
        {
            // MarkAsArrived() ya sincroniza todos los paquetes a AtDepot internamente
            shipment.MarkAsArrived();

            // Persist shipment y sus cambios en paquetes
            await _shipmentRepository.UpdateAsync(shipment);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (DomainException ex)
        {
            return Result<bool>.Failure(ex.Message, ErrorType.Conflict);
        }
    }
}
