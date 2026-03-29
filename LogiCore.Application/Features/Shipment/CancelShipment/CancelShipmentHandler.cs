using System.Threading;
using System.Threading.Tasks;
using MediatR;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.Common.Models;

namespace LogiCore.Application.Features.Shipment.CancelShipment;

public class CancelShipmentHandler : IRequestHandler<CancelShipmentCommand, Result<bool>>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelShipmentHandler(IShipmentRepository shipmentRepository, IUnitOfWork unitOfWork)
    {
        _shipmentRepository = shipmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(CancelShipmentCommand request, CancellationToken cancellationToken)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(request.ShipmentId);
        if (shipment == null) return Result<bool>.Failure("Shipment not found.", ErrorType.NotFound);

        try
        {
            shipment.Cancel();
            await _shipmentRepository.UpdateAsync(shipment);
            await _unitOfWork.CommitAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
        catch (LogiCore.Domain.Common.Exceptions.DomainException ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }
}
