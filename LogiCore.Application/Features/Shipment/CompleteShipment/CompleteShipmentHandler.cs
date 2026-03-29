using System.Threading;
using System.Threading.Tasks;
using MediatR;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.Common.Models;

namespace LogiCore.Application.Features.Shipment.CompleteShipment;

public class CompleteShipmentHandler : IRequestHandler<CompleteShipmentCommand, Result<bool>>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteShipmentHandler(IShipmentRepository shipmentRepository, IUnitOfWork unitOfWork)
    {
        _shipmentRepository = shipmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(CompleteShipmentCommand request, CancellationToken cancellationToken)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(request.ShipmentId);
        if (shipment == null) return Result<bool>.Failure("Shipment not found.", ErrorType.NotFound);

        try
        {
            shipment.MarkAsDelivered();
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
