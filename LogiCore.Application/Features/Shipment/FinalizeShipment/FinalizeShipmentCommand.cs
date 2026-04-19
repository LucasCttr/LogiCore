using MediatR;
using LogiCore.Application.Common.Models;

namespace LogiCore.Application.Features.Shipment.FinalizeShipment;

public class FinalizeShipmentCommand : IRequest<Result<bool>>
{
    public Guid ShipmentId { get; set; }

    public FinalizeShipmentCommand(Guid shipmentId)
    {
        ShipmentId = shipmentId;
    }
}
