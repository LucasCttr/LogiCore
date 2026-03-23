using MediatR;
using LogiCore.Application.Common.Models;

namespace LogiCore.Application.Features.Shipment.DispatchShipment;

public class DispatchShipmentCommand : IRequest<Result<bool>>
{
    public Guid ShipmentId { get; set; }
}
