using MediatR;
using LogiCore.Application.Common.Models;

namespace LogiCore.Application.Features.Shipment.ArriveShipment;

public class ArriveShipmentCommand : IRequest<Result<bool>>
{
    public Guid ShipmentId { get; set; }
}
