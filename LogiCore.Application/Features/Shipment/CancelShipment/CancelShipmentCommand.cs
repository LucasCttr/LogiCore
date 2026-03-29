using MediatR;
using LogiCore.Application.Common.Models;

namespace LogiCore.Application.Features.Shipment.CancelShipment;

public class CancelShipmentCommand : IRequest<Result<bool>>
{
    public Guid ShipmentId { get; set; }
}
